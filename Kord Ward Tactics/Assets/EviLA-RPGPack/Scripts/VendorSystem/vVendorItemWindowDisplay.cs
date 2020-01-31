using UnityEngine;
using UnityEngine.EventSystems;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    public class vVendorItemWindowDisplay : MonoBehaviour
    {
        private vCustomInventory inventory;
        public vItemWindow itemWindow;
        public vVendorItemOptionWindow optionWindow;
        [HideInInspector]
        public vItemSlot currentSelectedSlot;
        [HideInInspector]
        public int amount;

        public void OnEnable()
        {
            if (inventory == null)
                inventory = GetComponentInParent<vCustomInventory>();

            if (inventory && itemWindow)
                itemWindow.CreateEquipmentWindow(inventory.Vendor.items, OnSubmit, OnSelectSlot);
        }

        public void OnSubmit(vItemSlot slot)
        {
            currentSelectedSlot = slot;
            if (slot.item)
            {
                var rect = slot.GetComponent<RectTransform>();
                optionWindow.transform.position = rect.position;
                optionWindow.gameObject.SetActive(true);
                optionWindow.EnableOptions(slot);
                currentSelectedSlot = slot;
            }
        }

        public void OnSelectSlot(vItemSlot slot)
        {
            currentSelectedSlot = slot;
        }

        public void SetOldSelectable()
        {
            try
            {
                if (currentSelectedSlot != null)
                    SetSelectable(currentSelectedSlot.gameObject);
                else if (itemWindow.slots.Count > 0 && itemWindow.slots[0] != null)
                {
                    SetSelectable(itemWindow.slots[0].gameObject);
                }
            }
            catch
            {

            }
        }

        void SetSelectable(GameObject target)
        {
            try
            {
                var pointer = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
                EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
                ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
            }
            catch { }

        }


        public void BuyItem()
        {
            if (inventory == null)
                inventory = FindObjectOfType<vCustomInventory>();

            inventory.OnBuyItem(currentSelectedSlot.item, amount);
        }

        public void SellItem()
        {
            if (inventory == null)
                inventory = FindObjectOfType<vCustomInventory>();

            inventory.OnSellItem(currentSelectedSlot.item, amount);
        }

    }

}