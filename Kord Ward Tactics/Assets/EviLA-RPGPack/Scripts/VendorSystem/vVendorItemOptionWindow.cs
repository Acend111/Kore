using UnityEngine;
using UnityEngine.UI;

using System;

using Invector;
using Invector.vItemManager;

using EviLA.AddOns.RPGPack;

namespace EviLA.AddOns.RPGPack
{
    public class vVendorItemOptionWindow : MonoBehaviour
    {
        public Button SellItemButton;
        public Button BuyItemButton;

        public void EnableOptions(vItemSlot slot)
        {

            if (slot == null || slot.item == null) return;
            EnableBuyButton(slot);
            EnableSellButton(slot);
        }

        public virtual void EnableBuyButton(vItemSlot slot)
        {


            vItem buyItem = vQuestManager.Instance.inventory.items.Find(i => i.id == slot.item.id);

            int cashInHand = 0;

            try
            {

                var currencyIndex = buyItem.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value;
                var unitPrice = buyItem.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.BuyPriceFromVendor).value;
                cashInHand = vQuestManager.Instance.itemManager.items.Find(i => i.type == vItemType.Currency && i.attributes.GetAttributeByType(Invector.vItemManager.vItemAttributes.CurrencyIndexInManager).value == currencyIndex).amount;

                if (cashInHand >= unitPrice)
                    BuyItemButton.interactable = true;
                else
                    BuyItemButton.interactable = false;
            }

            catch (Exception)
            {
                BuyItemButton.interactable = false;
            }
        }

        public virtual void EnableSellButton(vItemSlot slot)
        {

            vItem sellItem = vQuestManager.Instance.itemManager.items.Find(i => i.id == slot.item.id);
            if (sellItem == null)
                SellItemButton.interactable = false;
            else
                SellItemButton.interactable = true;
        }

    }
}


