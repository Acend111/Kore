using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    public class vQuestWindowDisplay : MonoBehaviour
    {
        public vCustomInventory customInventory;
        public vQuestWindow questWindow;
        public vQuestOptionWindow optionWindow;
        [HideInInspector]
        public vQuestSlot currentSelectedSlot;
        [HideInInspector]
        public int amount;

        public void OnEnable()
        {
            if (customInventory == null)
                customInventory = GetComponentInParent<vCustomInventory>();

            questWindow.CreateEquipmentWindow(customInventory.quests, OnSubmit, OnSelectSlot);
            /*if (customInventory && !questWindow.isProviderWindow)
                questWindow.CreateEquipmentWindow(customInventory.quests, OnSubmit, OnSelectSlot);
            if (customInventory && questWindow.isProviderWindow)
            {
                List<vQuest> quests = new List<vQuest>();
                questWindow.CreateEquipmentWindow(customInventory.Provider.QuestsProvided, OnSubmit, OnSelectSlot);
            }*/
        }

        public void OnSubmit(vQuestSlot slot)
        {
            currentSelectedSlot = slot;
            if (slot.quest)
            {
                var rect = slot.GetComponent<RectTransform>();
                optionWindow.transform.position = rect.position;
                optionWindow.gameObject.SetActive(true);
                optionWindow.EnableOptions(slot);
                currentSelectedSlot = slot;
            }
        }

        public void OnSelectSlot(vQuestSlot slot)
        {
            currentSelectedSlot = slot;
        }

        public void AcceptQuest()
        {
            var questSystem = vQuestSystemManager.Instance;

            customInventory.onAcceptQuest.Invoke(currentSelectedSlot.quest, false, false);

            var forceStartOnAccept = questSystem.IsForceStartOnAccept(currentSelectedSlot.quest.id) ? true : questSystem.QuestManager.StartQuestOnAccept;

            vQuestSystemManager.Instance.onActiveQuestChanged.Invoke(currentSelectedSlot.quest.id, true, true, forceStartOnAccept);

            currentSelectedSlot.SetValid(false);
        }

        public void DeclineQuest()
        {
            customInventory.onDeclineQuest.Invoke(currentSelectedSlot.quest);
            currentSelectedSlot.SetValid(false);
        }


        public void UpdateActiveQuest()
        {
            vQuestSystemManager.Instance.onActiveQuestChanged.Invoke(currentSelectedSlot.quest.id, true, false, true);
        }

        public void SetOldSelectable()
        {
            try
            {
                if (currentSelectedSlot != null)
                    SetSelectable(currentSelectedSlot.gameObject);
                else if (questWindow.slots.Count > 0 && questWindow.slots[0] != null)
                {
                    SetSelectable(questWindow.slots[0].gameObject);
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

    }

}
