using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
namespace EviLA.AddOns.RPGPack
{
    public class vQuestOptionWindow : MonoBehaviour
    {
        public Button useQuestButton;
        public Button setActiveQuestButton;

        public bool isProvider;
		public List<vQuestState> questStatesToDisplayProvider = new List<vQuestState>();
		public List<vQuestState> questStatesToDisplay = new List<vQuestState>();


        public void EnableOptions(vQuestSlot slot)
        {
			var questSystem = vQuestSystemManager.Instance;
			var state = questSystem.GetQuestState(slot.quest.id);

            /* Quest Decline | Accept Quest */
            if (slot == null || slot.quest == null) return;
            if (isProvider)
                useQuestButton.interactable = questStatesToDisplayProvider.Contains(state);
            else
                useQuestButton.interactable = questStatesToDisplay.Contains(state);

            /* Set Active Quest | Decline Quest */
            if (!isProvider)
            {
				if (questSystem.QuestCanBeDeclined(slot.quest.id) && questSystem.GetQuestState(slot.quest.id) != vQuestState.PendingReward )
                    useQuestButton.interactable = true;
                else
                    useQuestButton.interactable = false;

				if (slot.quest.id == questSystem.ActiveQuest)
                    setActiveQuestButton.interactable = false;
                else
                {
					if (!questSystem.IsTimedQuestInProgress ())
						setActiveQuestButton.interactable = true;
					else
						setActiveQuestButton.interactable = false;
                }

            }
        }
    }
}

