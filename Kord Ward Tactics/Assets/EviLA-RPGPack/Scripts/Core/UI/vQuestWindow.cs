using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Text;
using System;

namespace EviLA.AddOns.RPGPack
{
    public delegate void OnSubmitQuestSlot(vQuestSlot slot);
    public delegate void OnSelectQuestSlot(vQuestSlot slot);
    public delegate void OnCompleteQuestSlotList(List<vQuestSlot> slots);

    public class vQuestWindow : MonoBehaviour
    {
        public vQuestSlot slotPrefab;
        public bool isProviderWindow;
        public RectTransform contentWindow;
        public Text questText;
        private OnSubmitQuestSlot onSubmitSlot;
        private OnSelectQuestSlot onSelectSlot;
        public OnCompleteQuestSlotList onCompleteSlotListCallBack;
        public List<vQuestSlot> slots;
        private vQuest currentQuest;
        private StringBuilder text;

        public void CreateEquipmentWindow(List<vQuest> questList, OnSubmitQuestSlot onPickUpItemCallBack = null, OnSelectQuestSlot onSelectSlotCallBack = null, bool destroyAdictionSlots = true)
        {
            var instance = vQuestSystemManager.Instance;

            List<vQuest> quests = new List<vQuest>();

            foreach (var quest in questList)
            {
                if (isProviderWindow)
                {
                    if (instance.GetParent(quest.id) == -1 && instance.GetQuestState(quest.id) != vQuestState.Completed)
                    {
                        quests.Add(quest);
                    }
                }
                else
                {
                    if (instance.GetParent(quest.id) == -1 && instance.IsQuestAccepted(quest.id))
                    {
                        quests.Add(quest);
                    }
                }
            }

            if (quests.Count == 0)
            {
                if (questText) questText.text = "";
                if (slots.Count > 0 && destroyAdictionSlots)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        Destroy(slots[i].gameObject);
                    }
                    slots.Clear();
                }
                return;
            }

            bool selecItem = false;
            onSubmitSlot = onPickUpItemCallBack;
            onSelectSlot = onSelectSlotCallBack;
            if (slots == null) slots = new List<vQuestSlot>();
            var count = slots.Count;
            if (count < quests.Count)
            {
                for (int i = count; i < quests.Count; i++)
                {
                    var slot = Instantiate(slotPrefab) as vQuestSlot;
                    SetDescription(slot);
                    slots.Add(slot);
                }
            }
            else if (count > quests.Count)
            {
                for (int i = count - 1; i > quests.Count - 1; i--)
                {
                    Destroy(slots[slots.Count - 1].gameObject);
                    slots.RemoveAt(slots.Count - 1);
                }
            }
            count = slots.Count;
            for (int i = 0; i < quests.Count; i++)
            {
                vQuestSlot slot = null;
                if (i < quests.Count)
                {
                    slot = slots[i];
                    slot.AddQuest(quests[i]);
                    slot.SetQuestText();
                    SetDescription(slot);
                    slot.CheckQuest(quests[i].isInEquipArea);

                    if (isProviderWindow)
                    {
                        if (instance.GetQuestState(quests[i].id) == vQuestState.NotStarted
                            || instance.GetQuestState(quests[i].id) == vQuestState.Failed
                            || instance.GetQuestState(quests[i].id) == vQuestState.NotAcceptedButComplete)
                            slot.SetValid(true);
                        else
                            slot.SetValid(false);
                    }
                    else
                    {
                        if (instance.GetQuestState(quests[i].id) == vQuestState.InProgress ||
                            instance.GetQuestState(quests[i].id) == vQuestState.PendingReward)
                            slot.SetValid(true);
                        else
                            slot.SetValid(false);
                    }

                    slot.onSubmitSlotCallBack = OnSubmit;
                    slot.onSelectSlotCallBack = OnSelect;
                    var rectTranform = slot.GetComponent<RectTransform>();
                    rectTranform.SetParent(contentWindow);
                    rectTranform.localPosition = Vector3.zero;

                    rectTranform.localScale = Vector3.one;
                    if (currentQuest != null && currentQuest == quests[i])
                    {
                        selecItem = true;
                        SetSelectable(slot.gameObject);
                    }
                }
            }

            if (slots.Count > 0 && !selecItem)
            {
                StartCoroutine(SetSelectableHandle(slots[0].gameObject));
            }

            if (onCompleteSlotListCallBack != null)
            {
                onCompleteSlotListCallBack(slots);
            }
        }

        public void CreateEquipmentWindow(List<vQuest> quests, vQuest currentQuest = null, OnSubmitQuestSlot onPickUpQuestCallback = null, OnSelectQuestSlot onSelectSlotCallBack = null)
        {
            this.currentQuest = currentQuest;
            var _quests = quests.FindAll(quest => currentQuest.id.Equals(quest.id));

            CreateEquipmentWindow(_quests, onPickUpQuestCallback);
        }

        IEnumerator SetSelectableHandle(GameObject target)
        {
            if (this.enabled)
            {
                yield return new WaitForEndOfFrame();
                SetSelectable(target);
            }
        }

        void SetSelectable(GameObject target)
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, pointer, ExecuteEvents.pointerExitHandler);
            EventSystem.current.SetSelectedGameObject(target, new BaseEventData(EventSystem.current));
            ExecuteEvents.Execute(target, pointer, ExecuteEvents.selectHandler);
        }

        public void OnSubmit(vQuestSlot slot)
        {
            if (onSubmitSlot != null)
                onSubmitSlot(slot);
        }

        public void SetDescription(vQuestSlot slot)
        {
            try
            {
                var questSystem = vQuestSystemManager.Instance;

                var quest = questSystem.QuestManager.questListData.quests.Find(q => q.id.Equals(slot.quest.id));

                if (questText != null)
                {
                    if (quest == null)
                    {
                        questText.text = "";
                    }
                    else
                    {
                        text = new StringBuilder();
                        if (!isProviderWindow && questSystem.GetQuestState(quest.id) == vQuestState.InProgress)
                        {
                            var secondaryList = quest.SecondaryQuests;
                            secondaryList.ForEach(q =>
                            {
                                text.AppendLine(q.objective + " [" + questSystem.GetQuestState(q.id).ToString().ToUpper() + "]");
                            });
                        }
                        else
                        {
                            text.AppendLine(quest.description);
                        }


                        if (quest.Rewards.Count > 0)
                        {
                            text.AppendLine();
                            text.AppendLine("REWARDS");
                            text.AppendLine();
                            quest.Rewards.ForEach(
                                reward =>
                                {
                                    var reward_itm = questSystem.ItemManager.itemListData.items.Find(item => item.id.Equals(reward.id));
                                    if (reward_itm != null)
                                        text.AppendLine(reward_itm.name + " : " + reward.amount);
                                });
                        }
                        questText.text = text.ToString();

                    }
                }
            }

            catch (Exception e)
            {

            }
        }
        public void OnSelect(vQuestSlot slot)
        {
            SetDescription(slot);

            if (onSelectSlot != null)
                onSelectSlot(slot);
        }
        public string InsertSpaceBeforeUpperCAse(string input)
        {
            var result = "";

            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    // if not the first letter, insert space before uppercase
                    if (!string.IsNullOrEmpty(result))
                    {
                        result += " ";
                    }
                }
                // start new word
                result += c;
            }

            return result;
        }

        public void OnCancel()
        {

        }
    }
}