
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine.Events;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class QuestReference
    {
        public int id;
        public List<vQuestAttribute> attributes;
        public bool changeAttributes;
        public vQuestProvider provider;
        public IQuestTarget gatherTarget;
        public vQuestState state;


        public QuestReference(int id, vQuestProvider provider, IQuestTarget gatherTarget)
        {
            this.id = id;
            this.provider = provider;
            this.gatherTarget = gatherTarget;
            //this.state = vQuestSystem.Instance.GetQuestState(id);
        }
    }

    public class vQuestProvider : vTriggerGenericAction, IQuestTarget
    {
        public bool questsHandledExternally = false;

        public delegate List<vQuest> GetQuestsDelegate();

        public GetQuestsDelegate GetQuestsHandler;

        [Header("Quest List Data to use WITH Quests")]
        public vQuestListData questListData;

        [Header("---Quests Filter---")]
        public List<vQuestType> questsFilter = new List<vQuestType>() { 0 };

        public UnityEvent ManageQuestExternally;

        public bool isTarget()
        {
            if (questsTargetOf != null)
                return true;
            else
                return false;
        }

        public bool isSeller()
        {
            return false;
        }

        public bool isProvider()
        {
            return true;
        }

        public string getTargetName()
        {
            return name;
        }

        [HideInInspector]
        public List<QuestReference> providerQuests = new List<QuestReference>();

        private List<QuestReference> _questsProvided;

        private List<vQuest> _questsProvidedScriptable = new List<vQuest>();

        public List<vQuest> QuestsProvided
        {
            get
            {
                if (_questsProvidedScriptable.Count == 0)
                {
                    var questSystem = vQuestSystemManager.Instance;
                    foreach (QuestReference qref in providerQuests)
                    {
                        var quest = providerQuests.Find(q => q.id.Equals(qref.id));
                        var questScriptableObj = questListData.quests.Find(q => q.id.Equals(quest.id));
                        var dependentQuests = questScriptableObj.DependentQuests;
                        if (dependentQuests != null && dependentQuests.Count > 0)
                        {
                            bool dependenciesCompleted = true;
                            dependentQuests.ForEach(q =>
                            {
                                var state = questSystem.GetQuestState(q.id);
                                if (state != vQuestState.Completed)
                                {
                                    dependenciesCompleted = false;
                                    return;
                                }
                            });
                            if (dependenciesCompleted)
                            {
                                _questsProvidedScriptable.Add(questListData.quests.Find(q => q.id == quest.id));
                            }
                        }
                        else
                        {
                            _questsProvidedScriptable.Add(questListData.quests.Find(q => q.id == quest.id));
                        }
                    }
                }
                return _questsProvidedScriptable;
            }
        }

        [HideInInspector]
        public List<QuestReference> _targetQuests = new List<QuestReference>();

        public List<QuestReference> questsTargetOf
        {
            get
            {
                if (_targetQuests.Count == 0)
                {
                    var instance = vQuestSystemManager.Instance;
                    foreach (QuestReference qref in providerQuests)
                    {
                        var quest = questListData.quests.Find(q => q.id.Equals(qref.id));
                        if (instance.GetTargetName(quest.id).Equals(getTargetName()))
                            _targetQuests.Add(new QuestReference(quest.id, quest.provider, quest.QuestTarget));
                    }
                }
                return _targetQuests;
            }
        }

        [HideInInspector]
        private vQuestManager _questManager;

        [HideInInspector]
        private vQuestManager playerQuestManager
        {
            get
            {
                if (_questManager != null)
                    return _questManager;
                else
                {
                    _questManager = vThirdPersonController.instance.GetComponent<vQuestManager>();
                    return _questManager;
                }
            }
        }

        private void MarkQuestAsCompleteOnProvider(QuestReference q)
        {
            _questsProvided.RemoveAll(qref => qref.id == q.id);
        }

        public void OnOpenCloseQuestsWindow(bool value = false)
        {
            playerQuestManager.inventory.SetCurrentProvider(this);
        }


        public void DoAction()
        {
            //Priority goes to finishing existing quests. Therefore if provider is a target of a quest, finishing the quest objective takes priority over providing a quest 

            var questList = new List<QuestReference>();
            questList.AddRange(_questsProvided);
            questList.Union<QuestReference>(questsTargetOf);

            var instance = vQuestSystemManager.Instance;


            var completedQuestList = questList.FindAll(q => (instance.GetQuestState(q.id) == vQuestState.Completed ||
                                                                    instance.GetQuestState(q.id) == vQuestState.PendingReward)
                                                                        && instance.GetParent(q.id) == -1 && !instance.IsAutoCompleteEnabled(q.id));

            if (completedQuestList.Count > 0)
            {
                foreach (var completedQuest in completedQuestList)
                {
                    if (completedQuest.id != instance.ActiveQuest)
                        continue;

                    var qscriptable = questListData.quests.Find(q => q.id == completedQuest.id);

                    if (instance.GetTargetName(completedQuest.id).Equals(getTargetName()))
                    {
                        playerQuestManager.inventory.onProviderVendorTargetActionEvent.Invoke(qscriptable, qscriptable.provider, this);
                        MarkQuestAsCompleteOnProvider(completedQuest);
                    }
                }
            }
            else
            {
                if (!questsHandledExternally)
                    OnOpenCloseQuestsWindow();
                else
                    ManageQuestExternally.Invoke();
            }
        }


        protected override void Start()
        {
            base.Start();

            if (_questsProvided == null)
                _questsProvided = providerQuests.vCopy<QuestReference>();

            OnDoAction.AddListener(DoAction);
        }

    }
}

