using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace EviLA.AddOns.RPGPack.UI
{
    public enum QuestDisplayElementType
    {
        Name,
        Description,
        Objective,
        Custom
    }

    public class vQuestDisplayElement : MonoBehaviour
    {
        public Text Message;
        [HideInInspector]
        public int questID = -1;

        [HideInInspector]
        public bool inUse = false;

        public void ShowFadingText(int questID, string message, float timeToStay = 1, float timeToFadeOut = 1)
        {
            this.questID = questID;
            inUse = true;
            Message.text = message;
            StartCoroutine(Timer(this.questID, timeToStay, timeToFadeOut));
        }

        public void ShowStayingText(int questID, string message, vQuestState stateToWaitFor = vQuestState.Completed, float timeToStay = 1, float timeToFadeOut = 1)
        {
            this.questID = questID;
            inUse = true;
            Message.text = message;
            StartCoroutine(Conditional(this.questID, stateToWaitFor, timeToStay, timeToFadeOut));
        }

        public void UpdateMessage(string message)
        {
            Message.text = message;
        }

        IEnumerator Timer(int questID,float timeToStay = 1, float timeToFadeOut = 1)
        {
            Message.CrossFadeAlpha(1, 0.5f, false);

            yield return new WaitForSeconds(timeToStay);
            Message.CrossFadeAlpha(0, timeToFadeOut, false);

            yield return new WaitForSeconds(timeToFadeOut + 0.1f);
            Destroy(gameObject);
            inUse = false;

            vQuestStatusDisplay.Instance.fadingQuestIDs.RemoveAll( q => q.Id == questID );

        }

        IEnumerator Conditional(int questID, vQuestState stateToWaitFor = vQuestState.Completed, float timeToStay = 1, float timeToFadeOut = 1)
        {
            Message.CrossFadeAlpha(1, 0.5f, false);

            var instance = vQuestSystemManager.Instance;
			var duration = instance.GetDuration (questID);

			if (duration <= 0f) {
				yield return new WaitUntil (() => instance.GetQuestState (questID) == stateToWaitFor || instance.GetQuestState (questID) == vQuestState.Failed);
			}
			else {
				var elapsedDuration = instance.GetElapsedDuration (questID);
				var message = Message.text;
				while (elapsedDuration <= duration) {
					elapsedDuration = instance.GetElapsedDuration (questID);
					var difference = duration - elapsedDuration;
					Mathf.Clamp (difference, 0f, duration);
					TimeSpan ts = TimeSpan.FromSeconds (difference);
					if (elapsedDuration > 0) {
						string formattedTs = string.Format ("{0:D2}:{1:D2}:{2:D3}", ts.Minutes, ts.Seconds, ts.Milliseconds);
						Message.text = message + " " + formattedTs;
					} else {
						Message.text = message;
					}
					if (instance.GetQuestState (questID) == stateToWaitFor || instance.GetQuestState (questID) == vQuestState.Failed) {
						break;
					}
					yield return null;
				}
			}

            yield return new WaitForSeconds(timeToStay);
            Message.CrossFadeAlpha(0, timeToFadeOut, false);

            yield return new WaitForSeconds(timeToFadeOut + 0.1f);
            Destroy(gameObject);
            inUse = false;

            vQuestStatusDisplay.Instance.stayingQuestIDs.RemoveAll(q => q.Id == questID);

        }

        public void Init()
        {
            Message.text = "";
            Message.CrossFadeAlpha(0, 0, false);
        }
    }
}
