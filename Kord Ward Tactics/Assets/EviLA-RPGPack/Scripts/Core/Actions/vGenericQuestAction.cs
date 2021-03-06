using System.Collections;
using UnityEngine;
using Invector;
using Invector.vCharacterController.vActions;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack
{

    [vClassHeader("Generic Quest Action", "Sets the progress of a generic quest on trigger", iconName = "icon_v2")]
    public class vGenericQuestAction : vActionListener
    {

        [vEditorToolbar("Quest Action Properties")]
        public string actionTag = "questAction";
        public GenericInput enterInput = new GenericInput("E", "A", "A");


        protected vTriggerGenericQuestAction genericQuestTriggerAction;
        protected vThirdPersonInput tpInput;
        protected vQuestSystemManager questSystem;

        private OnAcceptQuestEvent onAcceptQuest = new OnAcceptQuestEvent();
        private OnDeclineQuestEvent onDeclineQuest = new OnDeclineQuestEvent();
        private OnCompleteQuestEvent onCompleteQuest = new OnCompleteQuestEvent();

        public void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();
            var questManager = GetComponent<vQuestManager>();
            onAcceptQuest.AddListener(questManager.AcceptQuest);
            onDeclineQuest.AddListener(questManager.DeclineQuest);
            onCompleteQuest.AddListener(questManager.UpdateQuestFromActionTrigger);
        }

        public void LateUpdate()
        {
            PerformQuestAction();
            TriggerQuestAction();
        }

        public override void OnActionStay(Collider other)
        {
            base.OnActionStay(other);
            if (other.gameObject.CompareTag(actionTag))
            {
                CheckForTriggerAction(other);
            }
        }

        public override void OnActionExit(Collider other)
        {
            base.OnActionExit(other);

            if (other.gameObject.CompareTag(actionTag))
            {
                if (genericQuestTriggerAction != null)
                    genericQuestTriggerAction.OnDoAction.RemoveListener(PerformQuestAction);
                genericQuestTriggerAction = null;
            }
        }

        void CheckForTriggerAction(Collider other)
        {
            genericQuestTriggerAction = other.gameObject.GetComponent<vTriggerGenericQuestAction>();
            if (genericQuestTriggerAction == null)
                genericQuestTriggerAction = other.gameObject.GetComponentInParent<vTriggerGenericQuestAction>();
            if (genericQuestTriggerAction == null)
                genericQuestTriggerAction = other.gameObject.GetComponentInChildren<vTriggerGenericQuestAction>();

            if (genericQuestTriggerAction != null)
                genericQuestTriggerAction.OnDoAction.AddListener(PerformQuestAction);
        }

        void PerformQuestAction()
        {
            if (genericQuestTriggerAction == null || tpInput.cc.customAction || tpInput.cc.isJumping || !tpInput.cc.isGrounded) return;
            if (genericQuestTriggerAction.triggeredOnce) return;
            if (vQuestSystemManager.Instance.GetQuestState(genericQuestTriggerAction.quest.id) == vQuestState.Completed)
                return;

            if (enterInput.GetButtonDown() && !genericQuestTriggerAction.autoAction)
            {
                QuestAction();
            }

        }

        void TriggerQuestAction()
        {
            if (genericQuestTriggerAction == null || tpInput.cc.customAction || tpInput.cc.isJumping || !tpInput.cc.isGrounded || !genericQuestTriggerAction.autoAction) return;
            if (genericQuestTriggerAction.triggeredOnce) return;

            QuestAction();
        }

        void QuestAction()
        {
            if (questSystem == null)
                questSystem = vQuestSystemManager.Instance;

            if (questSystem.IsTimedQuestInProgress())
                if (!(questSystem.ActiveQuest == genericQuestTriggerAction.quest.id
                    || questSystem.ActiveQuest == questSystem.GetParent(genericQuestTriggerAction.quest.id)))
                {
                    return;
                }
            genericQuestTriggerAction.triggeredOnce = true;
            StartCoroutine(QuestActionCoroutine());

        }

        IEnumerator QuestActionCoroutine()
        {
            yield return new WaitForSeconds(genericQuestTriggerAction.ActionDelay);

            switch (genericQuestTriggerAction.triggerType)
            {

                case vQuestTriggerType.AcceptOnly:
                    onAcceptQuest.Invoke(genericQuestTriggerAction.quest, true, false);
                    break;

                case vQuestTriggerType.AcceptAndCountDown:
                    onAcceptQuest.Invoke(genericQuestTriggerAction.quest, true, true);
                    break;

                case vQuestTriggerType.Failure:
                    onDeclineQuest.Invoke(genericQuestTriggerAction.quest);
                    break;

                case vQuestTriggerType.Complete:
                    onCompleteQuest.Invoke(genericQuestTriggerAction.quest);
                    break;

                default:
                    break;
            }
        }


    }
}
