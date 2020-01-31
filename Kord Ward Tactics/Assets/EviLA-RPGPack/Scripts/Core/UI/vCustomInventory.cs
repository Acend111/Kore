using UnityEngine;
using System.Collections.Generic;
using Invector.vCharacterController;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    public class vCustomInventory : vInventory
    {
        public delegate List<vQuest> GetQuestsDelegate();

        public GetQuestsDelegate GetQuestsHandler;

        public vInventoryWindow providerWindow;
        public vInventoryWindow vendorWindow;

        [HideInInspector]
        public OnOpenCloseQuestProviderUI onOpenCloseQuestProviderUI;
        [HideInInspector]
        public OnChangeItemAmount onBuyItem, onSellItem;
        [HideInInspector]
        public OnAcceptQuestEvent onAcceptQuest;
        [HideInInspector]
        public OnDeclineQuestEvent onDeclineQuest;
        [HideInInspector]
        public OnCompleteQuestEvent onCompleteQuest;
        [HideInInspector]
        public OnProviderVendorTargetActionEvent onProviderVendorTargetActionEvent;

        private vQuestProvider provider;
        private vItemSeller vendor;

        internal void SetCurrentProvider(vQuestProvider provider)
        {
            this.provider = provider;
            SetCurrentWindow(providerWindow);
        }

        internal void SetCurrentVendor(vItemSeller vendor)
        {
            this.vendor = vendor;
            SetCurrentWindow(vendorWindow);
        }

        private void SetPlayerInventory()
        {
            GetItemsHandler = () => { return vThirdPersonController.instance.GetComponent<vItemManager>().items; };
        }

        private void SetVendorInventory()
        {
            GetItemsHandler = () => { return vendor.items; };
        }

        private void SetQuestProviderQuests()
        {
            GetQuestsHandler = () =>
            {
                return provider.QuestsProvided;
            };
        }

        private void SetPlayerQuests()
        {
            GetQuestsHandler = () => { return vThirdPersonController.instance.GetComponent<vQuestManager>().quests; };
        }

        internal vQuestProvider Provider
        {
            get
            {
                return provider;
            }
        }

        internal vItemSeller Vendor
        {
            get
            {
                return vendor;
            }
        }


        public List<vQuest> quests
        {
            get
            {
                if (GetQuestsHandler != null) return GetQuestsHandler();
                return new List<vQuest>();
            }
        }

        public override void ControlWindowsInput()
        {
            // enable first window
            if ((windows.Count == 0 || windows[windows.Count - 1] == firstWindow))
            {
                if (!firstWindow.gameObject.activeSelf && openInventory.GetButtonDown())
                {
                    SetPlayerInventory();
                    firstWindow.gameObject.SetActive(true);
                    isOpen = true;
                    onOpenCloseInventory.Invoke(true);
                    Time.timeScale = timeScaleWhileIsOpen;
                }

                else if (firstWindow.gameObject.activeSelf && (openInventory.GetButtonDown() || cancel.GetButtonDown()))
                {
                    firstWindow.gameObject.SetActive(false);
                    isOpen = false;
                    onOpenCloseInventory.Invoke(false);
                    Time.timeScale = 1;
                }
            }

            // enable provider window
            if ((windows.Count == 0 || windows[windows.Count - 1] == providerWindow))
            {

                if (!providerWindow.gameObject.activeSelf && provider)
                {
                    SetQuestProviderQuests();
                    providerWindow.gameObject.SetActive(true);
                    isOpen = true;
                    onOpenCloseInventory.Invoke(true);
                    Time.timeScale = timeScaleWhileIsOpen;
                }

                else if (providerWindow.gameObject.activeSelf && provider && (cancel.GetButtonDown()))
                {
                    SetPlayerQuests();
                    provider = null;
                    providerWindow.gameObject.SetActive(false);
                    isOpen = false;
                    onOpenCloseInventory.Invoke(false);
                    Time.timeScale = 1;
                }
            }

            // enable vendor window
            if ((windows.Count == 0 || windows[windows.Count - 1] == vendorWindow))
            {
                SetVendorInventory();
                if (!vendorWindow.gameObject.activeSelf && vendor)
                {
                    vendorWindow.gameObject.SetActive(true);
                    isOpen = true;
                    onOpenCloseInventory.Invoke(true);
                    Time.timeScale = timeScaleWhileIsOpen;
                }

                else if (vendorWindow.gameObject.activeSelf && vendor && (cancel.GetButtonDown()))
                {
                    vendor = null;
                    vendorWindow.gameObject.SetActive(false);
                    isOpen = false;
                    onOpenCloseInventory.Invoke(false);
                    Time.timeScale = 1;
                }
            }

            //if (!isOpen) return;
            // disable current window
            if ((windows.Count > 0 && windows[windows.Count - 1] != firstWindow) && cancel.GetButtonDown())
            {
                if (windows[windows.Count - 1].ContainsPop_up())
                {
                    windows[windows.Count - 1].RemoveLastPop_up();
                    return;
                }
                else
                {
                    windows[windows.Count - 1].gameObject.SetActive(false);
                    windows.RemoveAt(windows.Count - 1);//remove last window of the window list
                    if (windows.Count > 0)
                    {
                        windows[windows.Count - 1].gameObject.SetActive(true);
                        currentWindow = windows[windows.Count - 1];
                    }
                    else
                        currentWindow = null; //clear currentWindow if  window list count == 0        
                }

            }
            //check if currenWindow  that was closed
            if (currentWindow != null && !currentWindow.gameObject.activeSelf)
            {
                //remove currentWindow of the window list
                if (windows.Contains(currentWindow))
                    windows.Remove(currentWindow);
                // set currentWindow if window list have more windows
                if (windows.Count > 0)
                {
                    windows[windows.Count - 1].gameObject.SetActive(true);
                    currentWindow = windows[windows.Count - 1];
                }
                else
                    currentWindow = null;//clear currentWindow if  window list count == 0  

            }
        }

        internal void OnBuyItem(vItem item, int amount)
        {

            onBuyItem.Invoke(item, amount);
        }

        internal void OnSellItem(vItem item, int amount)
        {
            onSellItem.Invoke(item, amount);
        }

    }
}
