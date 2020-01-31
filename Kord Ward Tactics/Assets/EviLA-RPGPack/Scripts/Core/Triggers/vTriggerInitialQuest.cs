using UnityEngine;
using System.Collections;
using Invector;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence;

namespace EviLA.AddOns.RPGPack
{
    public class vTriggerInitialQuest : vMonoBehaviour
    {
        public void Start()
        {
            ActivateInitialQuest();
        }


        void ActivateInitialQuest()
        {
            StartCoroutine(WaitAndActivate());
        }

        IEnumerator WaitAndActivate()
        {

            yield return new WaitUntil(() => vThirdPersonController.instance != null);

            var persistence = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
            if (persistence != null)
            {
                yield return new WaitUntil(() => !vSaveSystem.IsLoading);
            }

            yield return new WaitUntil(() => vQuestSystemManager.Instance.QuestManager != null);

            vQuestSystemManager.Instance.SetInitiallyActiveQuest();

        }
    }
}

