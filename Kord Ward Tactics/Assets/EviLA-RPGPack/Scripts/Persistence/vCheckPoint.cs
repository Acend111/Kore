using System.Collections;
using Invector;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EviLA.AddOns.RPGPack
{

    public class vCheckPoint : vMonoBehaviour
    {

        public EviLA.AddOns.RPGPack.Persistence.vPersistenceManager saver;

        public bool DestroyOnSave;

        [HideInInspector]
        public bool stayDisabled = false;

        void OnTriggerEnter(Collider other)
        {
            if (!stayDisabled && other.tag == "Player")
            {
                SaveGame();
            }
        }

        void OnTriggerLeave(Collider other)
        {
            if (other.tag == "Player")
            {
                stayDisabled = false;
            }

        }

        public void SaveGame()
        {
            if (saver == null)
            {
                saver = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
                if (saver == null)
                    return;
            }



            var lastCheckPoint = saver.lastCheckPointInScene.Find(chkpt => chkpt.scene.Equals(SceneManager.GetActiveScene().name));

            if (lastCheckPoint != null)
                lastCheckPoint.checkpointName = name;
            else
            {
                var lastCheckPointNew = new EviLA.AddOns.RPGPack.Persistence.Serialization.LastCheckPoint();
                lastCheckPointNew.checkpointName = name;
                lastCheckPointNew.scene = SceneManager.GetActiveScene().name;

                saver.lastCheckPointInScene.Add(lastCheckPointNew);
            }

            if (saver.JustLoaded)
            {
                if (lastCheckPoint == null)
                {
                    saver.JustLoaded = false;
                }
                else if (lastCheckPoint.checkpointName.Equals(this.name))
                {
                    stayDisabled = true;
                    saver.JustLoaded = false;
                    StartCoroutine(DelayCheckpoint());
                    return;
                }
            }

            saver.Save();
            vHUDController.instance.ShowText("Checkpoint");
            if (DestroyOnSave)
                GameObject.Destroy(this);
        }

        IEnumerator DelayCheckpoint()
        {
            var saver = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
            var t = saver.checkPointActivationDelay;
            while (t > 0)
            {
                t -= Time.unscaledDeltaTime;
                yield return null;
            }
            stayDisabled = false;

        }
    }
}
