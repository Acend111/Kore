using UnityEngine;
using System.Collections;
using Invector.vItemManager;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack {

	public class vQuestItemCollection : vItemCollection {

		vQuestTarget target;
        private vItemManager itemManager;

        private vItem questItem;
        private vItem itemInInventory;
        private int amountInCollection;

        public void Awake() {
            target = GetComponent<vQuestTarget>();
            questItem = target.quest.gatherItem;
            amountInCollection = this.items.Find(item => item.id == questItem.id).amount;
        }

        public new void Start()
        {
            base.Start();
            itemManager = vThirdPersonController.instance.GetComponent<vItemManager>();
        }

        public void CollectItem() {

            itemInInventory = itemManager.items.Find(item => item.id == questItem.id);
            var originalAmount = (itemInInventory == null) ? 0 : itemInInventory.amount;
            StartCoroutine(UpdateTargetQuest(originalAmount));
		}

        IEnumerator UpdateTargetQuest(int originalAmount)
        {
            yield return new WaitForSeconds(onCollectDelay);
            yield return new WaitUntil(() => HasItemBeenCollected(originalAmount));
            target.OnTargetAction();
        }

        bool HasItemBeenCollected(int originalAmount)
        {
            if(itemInInventory == null)
                itemInInventory = itemManager.items.Find(item => item.id == questItem.id);

            int currentAmount = (itemInInventory == null) ? 0 : itemInInventory.amount;
            return currentAmount == amountInCollection + originalAmount;

        }

    }
}
