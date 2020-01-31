using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Invector;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Spawners;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Persistence
{

    public class vPersistenceManager : vMonoBehaviour
    {

        public UnityEvent onPlayerLoaded;
        [HideInInspector]
        public vThirdPersonController controller;
        public float checkPointActivationDelay;

        [HideInInspector]
        public List<vQuestSystemSpawner> spawners = new List<vQuestSystemSpawner>();
        [HideInInspector]
        public List<MonoBehaviour> saveMono = new List<MonoBehaviour>();

        [HideInInspector]
        public List<LastCheckPoint> lastCheckPointInScene = new List<LastCheckPoint>();
        [HideInInspector]
        public List<vCheckPoint> sceneCheckPoints = new List<vCheckPoint>();

        [HideInInspector]
        public bool noItemManager = false;

        [HideInInspector]
        public bool noQuestManager = false;


        [HideInInspector]
        private Scene activeScene;

        private static vPersistenceManager instance;

        private List<string> deletedGameObjects = new List<string>();

        private vSaveSystem saver;

        private bool justLoaded = true;

        public bool JustLoaded
        {
            get
            {
                return justLoaded;
            }
            set
            {
                justLoaded = value;
            }
        }

        public void Start()
        {

        }

        public void AddToDeletedList(vCanSaveYou savable)
        {
            if (activeScene == null)
                activeScene = SceneManager.GetActiveScene();
            deletedGameObjects.Add(activeScene.name + "_" + savable.guid);
        }

        public void AddToDeletedList(List<string> savable)
        {
            deletedGameObjects.AddRange(savable);
        }

        public List<string> DeletedGameObjects
        {
            get
            {
                return deletedGameObjects;
            }
        }

        void Awake()
        {

            if (!this.isActiveAndEnabled)
                return;

            if (instance == null)
            {
                instance = this;

                AttachToScene();
            }

            if (instance != null)
                DontDestroyOnLoad(instance);

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadCheckPoints();

            activeScene = SceneManager.GetActiveScene();
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "Yes");
            saver = vSaveSystem.Instance;

            Resources.FindObjectsOfTypeAll<vCanSaveYou>().vToList().ForEach(
                save =>
                {
                    saveMono.Add(save);
                });

            ManageAfterLoadItems();
            StartCoroutine(vSaveSystem.Instance.StartLoading(saveMono, noItemManager, noQuestManager));

        }

        public void DetachFromScene()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void AttachToScene()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void LoadCheckPoints()
        {
            sceneCheckPoints.Clear();
            var checkpoints = Resources.FindObjectsOfTypeAll<vCheckPoint>().vToList();
            checkpoints.ForEach(checkpoint => { sceneCheckPoints.Add(checkpoint); });
        }

        public void Save()
        {

            if (saver == null)
                saver = vSaveSystem.Instance;
            StartCoroutine(saver.Save(saveMono));
        }

        void ManageAfterLoadItems()
        {
            vSaveSystem.Instance.loadingObjects = true;
            vSaveSystem.Instance.loadingPlayer = true;
        }

        IEnumerator ManageCheckpoints()
        {

            var checkpoints = Resources.FindObjectsOfTypeAll<vCheckPoint>().vToList();
            this.sceneCheckPoints = checkpoints;
            checkpoints.ForEach(checkpoint => checkpoint.gameObject.SetActive(false));

            yield return new WaitForSeconds(checkPointActivationDelay);
            yield return new WaitUntil(() => !vSaveSystem.IsLoading);
            yield return new WaitForSeconds(checkPointActivationDelay);

            checkpoints.ForEach(checkpoint =>
            {
                var lastchkpt = lastCheckPointInScene.Find(chkpt => chkpt.scene.Equals(SceneManager.GetActiveScene().name));
                if (lastCheckPointInScene != null && lastCheckPointInScene.Count > 0 && lastchkpt != null && lastchkpt.checkpointName.Equals(checkpoint.name))
                    checkpoint.stayDisabled = true;
                checkpoint.gameObject.SetActive(true);
            });

        }

    }

}