using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

namespace EviLA.AddOns.RPGPack
{
    public delegate void QuestSlotEvent(vQuestSlot quest);

    public class vQuestSlot : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image icon;
        public Image blockIcon;
        public Image checkIcon;
        public Text questNameText;
        public vQuest quest;
        public bool isValid = true;
        [HideInInspector]
        public bool isChecked;
        public QuestSlotEvent onSubmitSlotCallBack, onSelectSlotCallBack, onDeselectSlotCallBack;
        Color color = Color.white;
        public OnHandleQuestEvent onAddQuest, onRemoveQuest;

        public void Start()
        {
            SetQuestText();
            SetValid(isValid);
            CheckQuest(false);
        }

        public void SetQuestText()
        {
            if (questNameText != null)
            {
                if (quest == null)
                {
                    questNameText.text = "";
                }
                else
                {
                    var text = new StringBuilder();
                    text.AppendLine(quest.description);

                    questNameText.text = text.ToString();

                    text = new StringBuilder();
                    text.AppendLine(quest.name);

                    questNameText.text = text.ToString();
                }
            }
        }

        public virtual void CheckQuest(bool value)
        {
            isChecked = value;
            if (checkIcon)
            {
                checkIcon.gameObject.SetActive(isChecked);
            }
        }

        public virtual void SetValid(bool value)
        {
            isValid = value;
            Selectable sectable = GetComponent<Selectable>();
            if (sectable)
            {
                sectable.interactable = value;
            }
            if (blockIcon == null) return;
            blockIcon.color = value ? Color.clear : Color.white;
            blockIcon.SetAllDirty();
            isValid = value;
        }

        public virtual void AddQuest(vQuest quest)
        {
            if (quest != null)
            {
                onAddQuest.Invoke(quest);
                this.quest = quest;
                icon.sprite = quest.icon;
                color.a = 1;
                icon.color = color;

            }
            else
                RemoveQuest();
        }

        public virtual void RemoveQuest()
        {
            onRemoveQuest.Invoke(quest);
            color.a = 0;
            icon.color = color;
            this.quest = null;
            //amountText.text = "";
            icon.sprite = null;
            icon.SetAllDirty();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                if (isValid)
                    if (onSubmitSlotCallBack != null)
                        onSubmitSlotCallBack(this);
        }

        public virtual bool isOcupad()
        {
            return quest != null;
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (onSelectSlotCallBack != null)
                onSelectSlotCallBack(this);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            if (onDeselectSlotCallBack != null)
                onDeselectSlotCallBack(this);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (isValid)
                if (onSubmitSlotCallBack != null)
                    onSubmitSlotCallBack(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            if (onSelectSlotCallBack != null)
                onSelectSlotCallBack(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (onDeselectSlotCallBack != null)
                onDeselectSlotCallBack(this);
        }
    }
}

