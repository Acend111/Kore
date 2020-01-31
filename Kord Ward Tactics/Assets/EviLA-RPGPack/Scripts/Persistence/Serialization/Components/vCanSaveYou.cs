using UnityEngine;
using EviLA.AddOns.RPGPack.Helpers;
using Invector;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack.Persistence
{

    [System.Serializable]
    public class vCanSaveYou : vMonoBehaviour
    {

        [ReadOnly]
        public string guid;
        public bool ObjectHasLegacyAnimations;
        [HideInInspector]
        public bool finishedPlaying = false;

        public void Start()
        {
            var animation = GetComponent<Animation>();
        }

        public void OnDestroy()
        {
            if (vThirdPersonController.instance != null)
            {
                var persistenceManager = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
                if (persistenceManager != null)
                    persistenceManager.AddToDeletedList(this);
            }
        }

        public void SetPlayingFinished()
        {
            finishedPlaying = true;
        }
    }
}