using UnityEngine;
using Invector;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class vQuestTarget : vMonoBehaviour, IQuestTarget
    {
        [SerializeField]
        public vQuest quest;
        [SerializeField]
        public OnProviderVendorTargetActionEvent onProviderVendorTargetActionEvent;

        public virtual void OnTargetAction()
        {

            var qm = vThirdPersonController.instance.GetComponent<vQuestManager>();


            if (vQuestSystemManager.Instance.GetQuestState(quest.id) != vQuestState.Failed)
            {
                qm.UpdateQuestFromTarget(quest, null, this);
                onProviderVendorTargetActionEvent.Invoke(quest, null, this);
            }
        }

        public bool isTarget()
        {
            return true;
        }

        public bool isSeller()
        {
            return false;
        }

        public bool isProvider()
        {
            return false;
        }

        public string getTargetName()
        {
            return this.name;
        }
    }

}

