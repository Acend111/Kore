using UnityEngine;
using Invector;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using Invector.vItemManager;
using EviLA.AddOns.RPGPack;

namespace EviLA.AddOns.RPGPack.VendorSystem
{
    [vClassHeader("Vendor Manager", "Vendor Manager", iconName = "icon_v2")]
    public class vVendorManager : vMonoBehaviour
    {

        private vCustomInventory inventory;
        private vQuestManager questManager;

        [vEditorToolbar("Events")]
        public OnChangeItemAmount onBuyItem, onSellItem;

        public void BuyItem(vItem itemInManager, int amount)
        {
            var itemManager = vQuestManager.Instance.itemManager;

            var item = inventory.Vendor.vendorItems.Find(i => i.id == itemInManager.id);
            var vendorItem = inventory.Vendor.items.Find(i => i.id == itemInManager.id);


            var currencyIndex = item.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value;
            var unitPrice = item.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.BuyPriceFromVendor).value;
            var cost = unitPrice * amount;
            var currency = itemManager.items.Find(i => i.type == vItemType.Currency && i.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value == currencyIndex);

            if (vendorItem != null && amount <= vendorItem.amount)
            {

                ItemReference refItem = new ItemReference(vendorItem.id);
                refItem.amount = amount;
                refItem.attributes = vendorItem.attributes.CopyAsNew();
                itemManager.AddItem(refItem);

                if (currency == null)
                {
                    currency = itemManager.itemListData.items.Find(i => i.type == vItemType.Currency && i.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value == currencyIndex);
                    ItemReference currencyRef = new ItemReference(currency.id);
                    currencyRef.amount = amount;
                    currencyRef.attributes = vendorItem.attributes.CopyAsNew();
                    itemManager.AddItem(currencyRef);
                    currency = itemManager.items.Find(i => i.id == currencyRef.id);
                }

                itemManager.DestroyItem(currency, cost);

                int vendorAmt = vendorItem.amount - amount;
                vendorItem.amount = vendorAmt;
                inventory.Vendor.UpdateItemQuantity(vendorItem, vendorAmt);

                onBuyItem.Invoke(vendorItem, amount);

            }
        }

        public void SellItem(vItem itemInManager, int amount)
        {
            var itemManager = questManager.itemManager;

            var item = inventory.Vendor.vendorItems.Find(i => i.id == itemInManager.id);
            var vendorItem = inventory.Vendor.items.Find(i => i.id == item.id);

            var currencyIndex = item.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value;
            var unitPrice = item.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.SellPriceToVendor).value;
            var sales = unitPrice * amount;
            var currency = itemManager.items.Find(i => i.type == vItemType.Currency && i.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value == currencyIndex);

            if (currency == null)
            {
                currency = itemManager.itemListData.items.Find(i => i.type == vItemType.Currency && i.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value == currencyIndex);
            }

            var _item = itemManager.items.Find(i => i.id == item.id);

            if (_item != null && amount <= _item.amount)
            {

                int vendorAmt = vendorItem.amount + amount;
                vendorItem.amount = vendorAmt;
                inventory.Vendor.UpdateItemQuantity(vendorItem, vendorAmt);

                ItemReference refItem = new ItemReference(currency.id);
                refItem.amount = sales;
                refItem.attributes = vendorItem.attributes.CopyAsNew();
                itemManager.AddItem(refItem);

                itemManager.DestroyItem(_item, amount);

                onSellItem.Invoke(vendorItem, amount);

            }
        }

        private Animator animator;

        private static vVendorManager instance;

        public static vVendorManager Instance
        {
            get
            {
                return instance;
            }
        }

        void Start()
        {
            if (instance != null) return;

            if (questManager == null)
                questManager = vQuestManager.Instance;

            instance = this;
            inventory = questManager.inventory;

            if (inventory)
            {
                inventory.onBuyItem.AddListener(BuyItem);
                inventory.onSellItem.AddListener(SellItem);
            }

        }
    }
}

