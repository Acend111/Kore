using System.Collections.Generic;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    public class vSettingsPanel : vMonoBehaviour
    {

        public List<vSettingsTab> tabs = new List<vSettingsTab>();

        public void OnEnable()
        {
            DisplayGameSettings();
        }

        public void DisplayControlSettings()
        {
            DisplaySettings(Settings.Controls);
        }

        public void DisplayVideoSettings()
        {
            DisplaySettings(Settings.Video);
        }

        public void DisplayGameSettings()
        {
            DisplaySettings(Settings.Game);
        }

        public void DisplayKeyBindings()
        {
            DisplaySettings(Settings.KeyBindings);
            DisplayMovementKeyBindings();
        }

        public void DisplayMovementKeyBindings()
        {
            DisplaySettings(Settings.Movement, true);
        }

        public void DisplayCombatKeyBindings()
        {
            DisplaySettings(Settings.Combat, true);
        }

        public void DisplayGeneralKeyBindings()
        {
            DisplaySettings(Settings.General, true);
        }

        public void DisplaySettings(Settings displaySetting, bool enableCurrentSelection = false)
        {
            var controlTab = tabs.Find(tab => tab.setting == displaySetting);
            var otherTabs = tabs.FindAll(tab => tab.setting != displaySetting);
            var currentSelection = tabs.Find(tab => tab.Selected && !tab.IsChildTab);

            otherTabs.ForEach(tab =>
            {
                if (enableCurrentSelection)
                {
                    if (!tab.Equals(currentSelection))
                        tab.DeSelectThis();
                }
                else
                {
                    tab.DeSelectThis();
                }
            });

            controlTab.SelectThis();

        }
    }

    public enum Settings
    {
        Game,
        Controls,
        Video,
        KeyBindings,
        Movement,
        Combat,
        General
    }

}