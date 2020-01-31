using UnityEngine;
using System.Collections.Generic;
using Invector;

namespace EviLA.AddOns.RPGPack.UI
{
    public enum QuestDisplayComponent
    {
        Name,
        Desc,
        Objective,
        Custom
    }
    public class vQuestStatusDisplay : MonoBehaviour
    {
        private static vQuestStatusDisplay instance;
        public static vQuestStatusDisplay Instance
        {
            get
            {
                if (instance == null) { instance = GameObject.FindObjectOfType<vQuestStatusDisplay>(); }
                return vQuestStatusDisplay.instance;
            }
        }       
       
        public GameObject QuestNameElement;
        public GameObject QuestDescElement;
        public GameObject QuestObjElement;
        public GameObject CustomTextElement;

        public Transform QuestNameTransform;
        public Transform QuestDescTransform;
        public Transform QuestObjTransform;
        public Transform CustomTextTransform;

        [HideInInspector]
        public List<vQuestProxy> stayingQuestIDs;
		[HideInInspector]
        public List<vQuestProxy> fadingQuestIDs;

        public void ShowFadingText(int questID, string message, float timeToStay, float timeToFadeOut)
        { 
             if(fadingQuestIDs.Find(q => q.Id == questID) != null)
                return;
        
            lock (fadingQuestIDs) {
                fadingQuestIDs.Add(vQuestSystemManager.Instance.GetProxyByID(questID));
            }

            var itemObj = Instantiate(CustomTextElement) as GameObject;
            itemObj.transform.SetParent(CustomTextTransform, false);

            vQuestDisplayElement element = itemObj.GetComponent<vQuestDisplayElement>();

            if (!element.inUse)
            {
                element.transform.SetAsLastSibling();
                element.Init();
                element.ShowFadingText(questID, message, timeToStay, timeToFadeOut);                    
            }            
        }

        public void ShowStayingText(int questID, QuestDisplayComponent displayElement, string message, vQuestState stateToWaitFor = vQuestState.Completed, float timeToStay = 1, float timeToFadeOut = 1)
        {

            GameObject questElement;
            Transform content;

            bool update = false;

			if (stayingQuestIDs.Find(q => q.Id == questID) != null)
                update = true;

			var questSystem = vQuestSystemManager.Instance;

            if (!update)
            { 
                lock (stayingQuestIDs)
                {
                    stayingQuestIDs.Add(vQuestSystemManager.Instance.GetProxyByID(questID));
                }

                switch (displayElement)
                {
                    case QuestDisplayComponent.Name:
                        questElement = QuestNameElement;
                        content = QuestNameTransform;
                        break;
                    case QuestDisplayComponent.Desc:
                        questElement = QuestDescElement;
                        content = QuestDescTransform;
                        break;
                    case QuestDisplayComponent.Objective:
                        questElement = QuestObjElement;
                        content = QuestObjTransform;
                        break;
                    case QuestDisplayComponent.Custom:
                        questElement = CustomTextElement;
                        content = CustomTextTransform;
                        break;
                    default:
                        questElement = CustomTextElement;
                        content = CustomTextTransform;
                        break;
                }

                var obj = Instantiate(questElement) as GameObject;
				obj.transform.SetParent(content, false);

                vQuestDisplayElement element = obj.GetComponent<vQuestDisplayElement>();

                if (!element.inUse)
                {
                    element.transform.SetAsFirstSibling();
                    element.Init();
                    element.ShowStayingText(questID, message, stateToWaitFor, timeToStay, timeToFadeOut);
                }
            }
            else
            {
                switch (displayElement)
                {
                    case QuestDisplayComponent.Name:
                        questElement = QuestNameElement;
                        content = QuestNameTransform;
                        break;
                    case QuestDisplayComponent.Desc:
                        questElement = QuestDescElement;
                        content = QuestDescTransform;
                        break;
                    case QuestDisplayComponent.Objective:
                        questElement = QuestObjElement;
                        content = QuestObjTransform;
                        break;
                    case QuestDisplayComponent.Custom:
                        questElement = CustomTextElement;
                        content = CustomTextTransform;
                        break;
                    default:
                        questElement = CustomTextElement;
                        content = CustomTextTransform;
                        break;
                }

                var contents = content.gameObject.GetComponentsInChildren<vQuestDisplayElement>().vToList();
                var questContent = contents.Find(c => c.questID == questID);

                if (questContent != null)
                {
                    questContent.UpdateMessage(message);
                }
                else
                {
                    lock (stayingQuestIDs)
                    {
                        stayingQuestIDs.Add(vQuestSystemManager.Instance.GetProxyByID(questID));
                    }

                    var obj = Instantiate(questElement) as GameObject;
                    obj.transform.SetParent(content, false);

                    vQuestDisplayElement element = obj.GetComponent<vQuestDisplayElement>();

                    if (!element.inUse)
                    {
                        element.transform.SetAsLastSibling();
                        element.Init();
                        element.ShowStayingText(questID, message, stateToWaitFor, timeToStay, timeToFadeOut);
                    }
                }
            }
        }

        public void Reset()
        {
            if (QuestNameTransform.childCount > 0)
				for(int i = QuestNameTransform.childCount - 1; i >= 0; i--)
				{
					Destroy(QuestNameTransform.GetChild(i).gameObject);
				}

            if (QuestDescTransform.childCount > 0)
				for(int i = QuestDescTransform.childCount - 1; i >= 0; i--)
				{
					Destroy(QuestDescTransform.GetChild(i).gameObject);
				}

            if (QuestObjTransform.childCount > 0)
				for(int i = QuestObjTransform.childCount - 1; i >= 0; i--)
				{
					Destroy(QuestObjTransform.GetChild(i).gameObject);
				}

             //TODO : Find an alternative to hide without perma delete on this text
             /*if (CustomTextTransform.childCount > 0)
                for(int i = CustomTextTransform.childCount - 1; i >= 0; i--)
				{
					Destroy(CustomTextTransform.GetChild(i).gameObject);
				}
             */
        }
    }
}

