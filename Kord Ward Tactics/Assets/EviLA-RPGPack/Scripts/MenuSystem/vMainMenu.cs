using UnityEngine;
using Invector;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    public class vMainMenu : vMonoBehaviour
    {

        public bool exitToTitleScreen = false;

        private Animator animator;
        private AudioSource audio;

        public void Start()
        {
            //since keyboard/controller support isn't currently implemented
            Cursor.visible = true;

            var camera = Camera.main;
            animator = camera.GetComponent<Animator>();
            this.audio = GetComponent<AudioSource>();
        }

        public void ShowMainMenuPage()
        {
            if (animator != null)
                animator.SetInteger("Navigation", (int)Page.Main);
        }

        public void ShowSettingsPage()
        {
            if (animator != null)
                animator.SetInteger("Navigation", (int)Page.Settings);
        }

        public void ShowPlayPage()
        {
            if (animator != null)
                animator.SetInteger("Navigation", (int)Page.Play);
        }

        public void ShowExitPage()
        {
            if (animator != null)
                animator.SetInteger("Navigation", (int)Page.Exit);
        }

        public void ShowLoadGamePage()
        {
            if (animator != null)
                animator.SetInteger("Navigation", (int)Page.Loading_GameID);
        }

        public void ShowLoadGameSlotPage()
        {
            if (animator != null)
                animator.SetInteger("Navigation", (int)Page.Loading_SlotID);
        }

        public void ResumeGame()
        {
            if (vThirdPersonController.instance != null)
            {
                var pauseAction = vThirdPersonController.instance.GetComponent<vPauseAction>();
                pauseAction.PauseUnpause();
            }
        }

        public void QuitGame()
        {
            if (!exitToTitleScreen)
                Application.Quit();
            else
            {
                vQuestSystemLevelLoader.instance.LoadMainMenu();
            }
        }

        public void LoadNewGame()
        {
            var instance = vQuestSystemLevelLoader.instance;
            instance.UpdateGameID(-1);
            instance.UpdateCheckPointID(0);
            instance.LoadGame(true);
        }
    }

    public enum Page
    {
        Main,
        Settings,
        Play,
        Exit,
        Loading_GameID,
        Loading_SlotID
    }

}

