using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector.vCharacterController;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using Invector;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    [vClassHeader("Quest Manager", "Quest Manager", iconName = "icon_v2")]
    public class vQuestManager : vMonoBehaviour
    {
        [vEditorToolbar("Mandatory")]
        public vCustomInventory inventoryPrefab;
        public vQuestListData questListData;
        public vItemManager itemManager;

        [vEditorToolbar("Generic")]
        public bool goToNextQuestInListOnFailure = false;
        public bool StartQuestOnAccept = false;

        [vEditorToolbar("Starting Quests")]

        [Header("---Quests Filter---")]
        public List<vQuestType> questsFilter = new List<vQuestType>() { 0 };


        #region SerializedProperties in Custom Editor

        [SerializeField]
        public List<QuestReference> startQuests = new List<QuestReference>();

        public List<vQuest> quests;

        [vEditorToolbar("Events")]
        public OnHandleQuestEvent onAddQuest;
        public OnAcceptQuestEvent onAcceptQuest;
        public OnDeclineQuestEvent onDeclineQuest;
        public OnCompleteQuestEvent onCompleteQuestObjective;
        public OnProviderVendorTargetActionEvent onProviderVendorTargetActionEvent;
        public OnOpenCloseInventory onOpenCloseInventory;
        public OnOpenCloseQuestProviderUI onOpenCloseQuestProviderUI;


        [HideInInspector]
        public vCustomInventory inventory;

        #endregion

        private Animator animator;
        private static vQuestManager instance;

        public static vQuestManager Instance
        {
            get
            {
                return instance;
            }
        }

        void OnEnable()
        {

            GameObject gObj = new GameObject("InitialQuestTrigger");
            gObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            gObj.AddComponent<vTriggerInitialQuest>();

            if (instance != null) return;

            instance = this;

            inventory = FindObjectOfType<vCustomInventory>();

            if (!inventory && inventoryPrefab)
            {
                inventory = Instantiate(inventoryPrefab);
            }

            DontDestroyOnLoad(instance);

            var tpInput = GetComponent<vMeleeCombatInput>();
            if (tpInput) tpInput.onUpdateInput.AddListener(UpdateInput);

        }

        void Start()
        {
            if (inventory)
            {
                inventory.GetQuestsHandler = GetQuests;
                inventory.onAcceptQuest.AddListener(AcceptQuest);
                inventory.onDeclineQuest.AddListener(DeclineQuest);
                inventory.onCompleteQuest.AddListener(UpdateQuest);
                inventory.onProviderVendorTargetActionEvent.AddListener(UpdateQuestFromTarget);
                inventory.onOpenCloseInventory.AddListener(OnOpenCloseInventory);
                inventory.onOpenCloseQuestProviderUI.AddListener(OnOpenCloseQuestProviderUI);
            }

            quests = new List<vQuest>();
            if (questListData)
            {
                for (int i = 0; i < startQuests.Count; i++)
                {
                    AddQuest(startQuests[i]);
                }
            }

            animator = GetComponent<Animator>();

        }

        public void LockInventory(bool value)
        {
            itemManager.inventory.lockInput = value;
        }

        public List<vQuest> GetQuests()
        {
            return quests;
        }


        public void AddQuest(QuestReference questReference)
        {
            if (questReference != null && questListData != null && questListData.quests.Count > 0)
            {
                var quest = questListData.quests.Find(t => t.id.Equals(questReference.id));
                if (quest)
                {
                    var sameQuests = quests.FindAll(i => i.id == quest.id);
                    if (sameQuests.Count == 0)
                    {
                        var _quest = Instantiate(quest);
                        _quest.name = _quest.name.Replace("(Clone)", string.Empty);
                        if (questReference.attributes != null && _quest.attributes != null && quest.attributes.Count == questReference.attributes.Count)
                            _quest.attributes = new List<vQuestAttribute>(questReference.attributes);
                        quests.Add(_quest);
                        onAddQuest.Invoke(_quest);
                        AddQuest(questReference);
                    }
                }
            }
        }

        public void RemoveQuests(int i)
        {
            quests.RemoveAll(q => q.id == i);
        }

        public void DeclineQuest(vQuest quest)
        {
            if (quest)
            {
                inventory.quests.RemoveAll(q => q.id == quest.id);

                vQuestSystemManager.Instance.UpdateQuestState(quest.id, vQuestState.Failed);

                bool activeQuestChanged = (quest.id == vQuestSystemManager.Instance.ActiveQuest) ? true : false;

                vQuestSystemManager.Instance.onActiveQuestChanged.Invoke(quest.id, true, false, activeQuestChanged);

                UpdateQuest(quest);

                vQuestSystemManager.Instance.UpdateQuestAccepted(quest.id, false);

                onDeclineQuest.Invoke(quest);


                if (quest.provider == null)
                    quest.provider = inventory.Provider;
            }
        }


        public void AcceptQuest(vQuest quest)
        {
            var questSystem = vQuestSystemManager.Instance;

            AcceptQuest(quest, false, false);

            var forceStartOnAccept = questSystem.IsForceStartOnAccept(quest.id) ? true : questSystem.QuestManager.StartQuestOnAccept;

            vQuestSystemManager.Instance.onActiveQuestChanged.Invoke(quest.id, true, true, forceStartOnAccept);
        }

        public void AcceptQuest(vQuest quest, bool forced = false, bool countdown = false)
        {
            if (quest != null && !vQuestSystemManager.Instance.IsQuestAccepted(quest.id))
            {
                var state = vQuestSystemManager.Instance.GetQuestState(quest.id);
                var parent = vQuestSystemManager.Instance.GetParent(quest.id);

                vQuestSystemManager.Instance.UpdateQuestAccepted(quest.id, true);
                vQuestSystemManager.Instance.UpdateQuest(quest.id, quest.provider);

                state = vQuestSystemManager.Instance.GetQuestState(quest.id);

                if (state == vQuestState.Completed)
                {
                    if (parent == -1)
                        TriggerActiveQuestChanged(quest.id);
                    else
                        RemoveQuests(quest.id);
                }
                else if (state == vQuestState.Failed)
                {
                    inventory.onDeclineQuest.Invoke(quest);
                    RemoveQuests(quest.id);
                    return;
                }
                else
                {
                    inventory.quests.RemoveAll(q => q.id == quest.id);
                    inventory.quests.Add(quest);
                    if (quest.provider == null)
                        quest.provider = inventory.Provider;
                    vQuestSystemManager.Instance.UpdateQuestState(quest.id, vQuestState.InProgress);
                    AddQuest(new QuestReference(quest.id, quest.provider, quest.QuestTarget));
                    onAcceptQuest.Invoke(quest, false, countdown);
                }
            }


            if (forced && quest != null)
            {
                TriggerActiveQuestChanged(quest.id, true);
            }

            if (countdown && quest != null)
            {
                CountDown(quest);
            }
        }

        public void UpdateQuest(vQuest quest)
        {

            var questSystem = vQuestSystemManager.Instance;
            questSystem.UpdateQuest(quest.id, null, null);
            TriggerActiveQuestChanged(quest.id);

        }

        public void UpdateQuestFromActionTrigger(vQuest quest)
        {
            var questSystem = vQuestSystemManager.Instance;
            questSystem.UpdateQuestFromActionTrigger(vQuestTriggerType.Complete, quest.id);
            TriggerActiveQuestChanged(quest.id);
        }

        public void UpdateQuestFromTarget(vQuest quest, vQuestProvider fromProvider = null, IQuestTarget fromTarget = null)
        {

            var questSystem = vQuestSystemManager.Instance;
            vQuestSystemManager.Instance.UpdateQuest(quest.id, fromProvider, fromTarget);
            TriggerActiveQuestChanged(quest.id);

        }

        public void CountDown(vQuest quest)
        {
            if (vQuestSystemManager.Instance.IsQuestAccepted(quest.id))
            {
                if (vQuestSystemManager.Instance.IsScriptedCountDownEnabled(quest.id))
                    StartCountDown(quest.id);
            }
            else
            {
                throw new UnityException("Quest needs to be accepted before countdown is started");
            }
        }

        public void TriggerActiveQuestChanged(int id, bool force = false)
        {
            var questSystem = vQuestSystemManager.Instance;

            var state = questSystem.GetQuestState(id);
            var parent = questSystem.GetParent(id);

            //11/01/2017 - Note that the parent will go to NotStarted State from the UpdateQuest set of methods.
            bool changeActiveQuest = parent == -1 ?
                                                (state == vQuestState.Completed || state == vQuestState.PendingReward || state == vQuestState.Failed) ? true : false
                : (questSystem.GetQuestState(parent) == vQuestState.Failed || questSystem.GetQuestState(parent) == vQuestState.NotStarted) ? true : false;

            if (force)
                changeActiveQuest = true;

            //31/08/2017 - If the quest doesn't have a parent, and is not accepted, do not show status. 
            //             If the quest has a parent, and the parent is in progress, 
            bool showStatus = parent == -1 ? (state == vQuestState.InProgress || questSystem.IsQuestAccepted(id)) ? true : false
                                                : true;

            questSystem.onActiveQuestChanged.Invoke((parent == -1 ? id : parent), showStatus, false, changeActiveQuest);
        }

        void OnOpenCloseInventory(bool value)
        {
            if (value)
                animator.SetTrigger("ResetState");

            onOpenCloseInventory.Invoke(value);
        }

        void OnOpenCloseQuestProviderUI(bool value)
        {
            if (value)
                animator.SetTrigger("ResetState");

            onOpenCloseQuestProviderUI.Invoke(value);
        }

        public void UpdateInput(vThirdPersonInput tpInput)

        {

            inventory.lockInput = inventory.isOpen;

            //tpInput.lockInputByItemManager = inventory.isOpen;

        }

        //Coroutines can only be done via mono-behaviors. As a result the functionality has been moved to QuestManager monobehavior
        public void StartCountDown(int questID)
        {
            StopAllCoroutines();
            StartCoroutine(CountDown(questID));
        }

        protected IEnumerator CountDown(int questID)
        {
            var questSystem = vQuestSystemManager.Instance;
            var duration = questSystem.GetDuration(questID);
            var currentDuration = questSystem.GetElapsedDuration(questID);

            vQuestState state = vQuestState.InProgress;

            while (questSystem.UpdateElapsedDuration(questID, currentDuration))
            {

                if (Time.timeScale > 0f && (questSystem.ActiveQuest == questID || questSystem.ActiveQuest == questSystem.GetParent(questID)))
                {

                    state = questSystem.GetQuestState(questID);

                    if (state == vQuestState.Completed || state == vQuestState.PendingReward || state == vQuestState.Failed)
                        break;
                    currentDuration += Time.unscaledDeltaTime;
                }

                yield return null;

            }

            state = questSystem.GetQuestState(questID);
            if (state != vQuestState.Completed && state != vQuestState.PendingReward)
            {

                var quest = questListData.quests.Find(q => q.id == questID);
                var parent = questListData.quests.Find(q => q.id == questSystem.GetParent(questID));

                if (state != vQuestState.Failed)
                    DeclineQuest(quest);
            }

            yield return null;
        }

        public void ForceStartQuests()
        {

        }
    }

}

