using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using UnityEngine.Events;

namespace EviLA.AddOns.RPGPack.MenuSystem
{
    public class vPauseAction : vMonoBehaviour
    {
        public GameObject pauseMenuPrefab;
        public bool hideHudOnPause = true;

        public GenericInput pauseInput = new GenericInput("Escape", "A", "A");
        public GenericInput secondaryPauseInput = new GenericInput("Pause", "A", "A");

        public UnityEvent OnPause;
        public UnityEvent OnResume;

        private float oldTimeScale;
        private GameObject pauseMenu;
        private GameObject hud;
        private vThirdPersonInput tpInput;

        private static vPauseAction instance;
        public static vPauseAction Instance
        {
            get
            {
                return instance;
            }
        }


        private bool paused;

        void Start()
        {
            if (instance != null)
                return;

            instance = this;

            paused = false;
            hud = GameObject.FindGameObjectWithTag("PlayerUI");
            tpInput = GetComponent<vThirdPersonInput>();

            DontDestroyOnLoad(this);

        }

        void LateUpdate()
        {
            if (pauseInput.GetButtonDown() || secondaryPauseInput.GetButtonDown())
            {
                PauseUnpause();
            }
        }

        public void PauseUnpause()
        {
            if (!paused)
            {
                paused = true;
                oldTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                tpInput.ShowCursor(true);
                tpInput.LockCursor(true); //Lock Cursor = true sets the lock mode of cursor to none, meaning the cursor is free to move on screen.
            }
            else
            {
                paused = false;
                Time.timeScale = oldTimeScale;
                tpInput.ShowCursor(tpInput.showCursorOnStart);
                tpInput.LockCursor(tpInput.unlockCursorOnStart);

            }

            ShowHidePauseMenu();
            ShowHideHUD();
            EnableDisableInput();
        }

        void ShowHidePauseMenu()
        {
            if (pauseMenu == null)
            {
                pauseMenu = GameObject.Instantiate(pauseMenuPrefab);
                pauseMenu.gameObject.SetActive(false);
            }

            if (paused)
                pauseMenu.gameObject.SetActive(true);
            else
                pauseMenu.gameObject.SetActive(false);
        }

        void ShowHideHUD()
        {
            if (hud == null)
                return;

            if (hideHudOnPause)
            {
                if (!paused)
                    hud.gameObject.SetActive(true);
                else
                    hud.gameObject.SetActive(false);
            }
        }

        void EnableDisableInput()
        {
            if (paused)
            {
                tpInput.SetLockBasicInput(true);
                tpInput.SetLockCameraInput(true);
                OnPause.Invoke();
            }
            else
            {
                tpInput.SetLockBasicInput(false);
                tpInput.SetLockCameraInput(false);
                OnResume.Invoke();
            }
        }
    }
}
