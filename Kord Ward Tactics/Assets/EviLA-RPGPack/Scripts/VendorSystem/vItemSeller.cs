
using System;
using System.Collections.Generic;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    [Serializable]
    public class vItemSeller : vTriggerGenericAction, IQuestTarget
    {
        public delegate List<vItem> GetItemsDelegate();

        public GetItemsDelegate GetItemsHandler;

        [Header("Item List Data for vendor")]
        public vItemListData itemListData;

        [Header("---Item Filter---")]
        public List<vItemType> itemsFilter = new List<vItemType>() { 0 };

        [Header("Quest List Data for vendor")]
        public vQuestListData questListData;

        [Header("---Quest Filter---")]
        public List<vQuestType> questsFilter = new List<vQuestType>() { 0 };

        [HideInInspector]
        public List<ItemReference> vendorItems = new List<ItemReference>();


        public bool isTarget()
        {
            return false;
        }

        public bool isProvider()
        {
            return false;
        }

        public bool isSeller()
        {
            return true;
        }

        public string getTargetName()
        {
            return name;
        }

        public List<vItem> items
        {
            get
            {
                List<vItem> items = new List<vItem>();
                foreach (ItemReference itemref in vendorItems)
                {
                    var item = itemListData.items.Find(i => i.id.Equals(itemref.id));
                    item.amount = itemref.amount;
                    items.Add(item);
                }
                return items;
            }
        }
        [HideInInspector]
        public List<QuestReference> vendorQuests = new List<QuestReference>();

        public List<vQuest> quests
        {
            get
            {
                List<vQuest> quests = new List<vQuest>();
                foreach (QuestReference qref in vendorQuests)
                {
                    var quest = questListData.quests.Find(q => q.id.Equals(qref.id));
                    quests.Add(quest);
                }
                return quests;
            }
        }

        public void UpdateItemQuantity(vItem item, int amount)
        {

            int index = vendorItems.FindIndex(i => i.id == item.id);
            vendorItems[index].amount = amount;

            var _item = items.Find(i => i.id == item.id);
            _item.amount = amount;

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

        public void OnOpenCloseVendorWindow(bool value = false)
        {
            playerQuestManager.inventory.SetCurrentVendor(this);
        }

        public void DoAction()
        {

            /* Check if any quest deliverables are met, if so handle quest event. 
               Otherwise Open Item Window if NPC has items to sell */
            var instance = vQuestSystemManager.Instance;
            var quest = this.quests.Find(q => instance.GetQuestState(q.id) == vQuestState.PendingReward && instance.GetTargetName(q.id).Equals(getTargetName()));

            if (quest != null)
            {
                playerQuestManager.inventory.onProviderVendorTargetActionEvent.Invoke(quest, null, this);
            }
            else
                OnOpenCloseVendorWindow();

        }

        protected override void Start()
        {
            base.Start();
            OnDoAction.AddListener(DoAction);
        }
    }
}
