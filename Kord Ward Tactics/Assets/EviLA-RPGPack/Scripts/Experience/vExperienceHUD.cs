using UnityEngine;
using UnityEngine.UI;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack.Experience.UI
{
    public class vExperienceHUD : MonoBehaviour
    {

        // Use this for initialization
        public static vExperienceHUD instance;
        vThirdPersonController tpc;
        vLevelManager levelManager;

        [Header("XP Slider")]
        public Slider xpSlider;

        [Header("Level Indicator Text")]
        public Text levelText;

        [Header("XP Stats Text")]
        public Text xpStatsText;

        public void Awake()
        {
            if (instance != null)
                return;

            instance = this;
            DontDestroyOnLoad(instance);
        }

        public void Start()
        {
            UpdateXPSlider();
        }

        public void InitXPSlider(int value)
        {
            if (levelManager == null)
            {
                if (vThirdPersonController.instance != null)
                    levelManager = vThirdPersonController.instance.GetComponent<vLevelManager>();
            }

            xpSlider.value = value;
            if (levelManager != null && levelText != null)
                levelText.text = "Level " + levelManager.CurrentLevel.ToString();

            if (levelManager != null && xpStatsText != null)
            {
                var required = (levelManager.RequiredXPForNextLevel - levelManager.RequiredXPForPreviousLevel);
                var current = levelManager.CurrentExperience;
                xpStatsText.text = current.ToString() + " / " + required.ToString();
            }
        }

        public void UpdateXPSlider()
        {
            if (levelManager == null)
            {
                if (vThirdPersonController.instance != null)
                    levelManager = vThirdPersonController.instance.GetComponent<vLevelManager>();
            }

            levelText.text = "Level " + levelManager.CurrentLevel.ToString();
            xpStatsText.text = levelManager.CurrentExperience.ToString() + " / " + (levelManager.RequiredXPForNextLevel - levelManager.RequiredXPForPreviousLevel).ToString();

            xpSlider.value = (float)((levelManager.CurrentExperience / (levelManager.RequiredXPForNextLevel - levelManager.RequiredXPForPreviousLevel)) * 100);
        }
    }
}