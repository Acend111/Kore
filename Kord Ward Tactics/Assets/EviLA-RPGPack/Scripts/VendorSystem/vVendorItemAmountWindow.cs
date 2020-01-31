using UnityEngine;
using UnityEngine.UI;

using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    public class vVendorItemAmountWindow : vWindowPop_up
    {
        public vVendorItemWindowDisplay itemWindowDisplay;
        public GameObject singleAmountControl;
        public GameObject multAmountControl;
        public Text amountDisplay;

        private vCustomInventory customInventory;

        protected override void OnEnable()
        {
            base.OnEnable();
            customInventory = gameObject.GetComponentInParent<vCustomInventory>();
            if (itemWindowDisplay)
            {
                if (itemWindowDisplay.currentSelectedSlot.item)
                {
                    singleAmountControl.SetActive(!(itemWindowDisplay.currentSelectedSlot.item.amount > 1));
                    multAmountControl.SetActive(itemWindowDisplay.currentSelectedSlot.item.amount > 1);
                    if (amountDisplay)
                        amountDisplay.text = (1).ToString("00");
                    itemWindowDisplay.amount = 1;
                }
            }
        }

        public virtual void ChangeBuyVendorAmount(int value)
        {

            if (itemWindowDisplay && itemWindowDisplay.currentSelectedSlot.item)
            {
                itemWindowDisplay.amount += value;

                var currencyIndex = itemWindowDisplay.currentSelectedSlot.item.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value;
                var unitPrice = itemWindowDisplay.currentSelectedSlot.item.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.BuyPriceFromVendor).value;
                var cashInHand = vQuestManager.Instance.itemManager.items.Find(i => i.type == vItemType.Currency && i.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value == currencyIndex).amount;

                int max = cashInHand / unitPrice;

                max = (max > itemWindowDisplay.currentSelectedSlot.item.amount) ? itemWindowDisplay.currentSelectedSlot.item.amount : max;

                if (customInventory.Vendor != null)
                {
                    itemWindowDisplay.amount = Mathf.Clamp(itemWindowDisplay.amount, 1, max);
                }
                if (amountDisplay)
                    amountDisplay.text = itemWindowDisplay.amount.ToString("00");
            }
        }

        public virtual void ChangeSellVendorAmount(int value)
        {

            if (itemWindowDisplay && itemWindowDisplay.currentSelectedSlot.item)
            {
                itemWindowDisplay.amount += value;

                if (customInventory.Vendor != null)
                {
                    var item = vQuestManager.Instance.itemManager.items.Find(i => i.id == itemWindowDisplay.currentSelectedSlot.item.id);
                    itemWindowDisplay.amount = Mathf.Clamp(itemWindowDisplay.amount, 1, item.amount);
                }
                if (amountDisplay)
                    amountDisplay.text = itemWindowDisplay.amount.ToString("00");
            }
        }
    }

}

