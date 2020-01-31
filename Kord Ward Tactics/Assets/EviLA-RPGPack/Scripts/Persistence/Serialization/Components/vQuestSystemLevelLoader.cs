using System;
using Invector;

namespace EviLA.AddOns.RPGPack.Persistence
{

    public class vQuestSystemLevelLoader : vMonoBehaviour
    {

        public static vQuestSystemLevelLoader instance;

        private int currentGameID = 0;
        private int currentCheckpoint = 0;

        public string startScene;
        public string mainMenuScene = "LevelLoader";
        public bool immediateLoad = false;

        public int CurrentGameID
        {
            get { return currentGameID; }
        }

        public int CurrentCheckPointID
        {
            get { return currentCheckpoint; }
        }

        public void Awake()
        {
            if (instance != null)
                return;


            instance = this;
            DontDestroyOnLoad(instance);

            if (immediateLoad)
                LoadGame();
        }

        public void OnEnable()
        {
            GC.Collect();
        }

        public void LoadGame(bool resetInitialLoading = false)
        {
            if (resetInitialLoading)
                vSaveSystem.ResetInitialLoad();

            vSaveSystem.LoadGame();
        }

        public void LoadMainMenu()
        {
            vSaveSystem.LoadMainMenu();
        }

        public void UpdateGameID(int gameID)
        {
            this.currentGameID = gameID;
        }

        public void UpdateCheckPointID(int checkPointID)
        {
            this.currentCheckpoint = checkPointID;
        }
    }

}