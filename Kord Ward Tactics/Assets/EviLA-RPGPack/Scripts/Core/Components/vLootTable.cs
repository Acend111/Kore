using System;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vItemManager;

namespace Invector
{

    [Serializable]
    public class vLootTable : vMonoBehaviour
    {

        [SerializeField]
        public vItemListData lootableItemListData;

        [SerializeField]
        public int numberofItemsToDrop;

        [SerializeField]
        GameObject dropObject;

        [HideInInspector, SerializeField]
        public List<ItemReference> lootableItems;

        [Header("---Lootables Filter---")]
        public List<vItemType> lootableFilter = new List<vItemType>() { 0 };

        private Dictionary<ItemReference, int> dropRates = new Dictionary<ItemReference, int>();

        public void DropLootables()
        {

            if (dropRates.Count == 0)
            {

                foreach (var lootable in lootableItems)
                {

                    //var item = lootableItemListData.items.Find( itm => itm.id == lootable.id ) ;
                    var dropRate = lootable.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.WeightedDropRate);

                    if (dropRate != null)
                    {
                        dropRates.Add(lootable, dropRate.value);
                    }
                }

            }

            if (dropRates.Count == 0)
                return;

            var drop = Instantiate(dropObject, transform.position, transform.rotation) as GameObject;
            var collection = drop.GetComponent<vItemCollection>();
            collection.items.Clear();

            for (int i = 0; i < numberofItemsToDrop; i++)
            {

                var dropItemIndex = GetRandomItem();

                if (dropItemIndex == -1 || collection.items.Find(item => item.id == lootableItems[dropItemIndex].id) != null)
                    continue;

                var dropItemReference = lootableItems[dropItemIndex];

                collection.items.Add(dropItemReference);

            }

            if (collection.items.Count == 0)
                Destroy(drop);

        }

        private int GetRandomItem()
        {

            float range = 0;

            for (int i = 0; i < lootableItems.Count; i++)
            {

                int rate = 0;
                dropRates.TryGetValue(lootableItems[i], out rate);
                range += rate;
            }

            var rand = UnityEngine.Random.Range(0, range);
            var top = 0;

            for (int i = 0; i < lootableItems.Count; i++)
            {

                int rate = 0;
                dropRates.TryGetValue(lootableItems[i], out rate);
                top += rate;

                if (rand <= top)
                {
                    return i;
                }
            }

            return -1;
        }
    }



}
