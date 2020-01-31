using System;
using System.Collections.Generic;
using UnityEngine;

using Invector;
using Invector.vMelee;
using Invector.vCharacterController.vActions;
using Invector.vCharacterController;
using Invector.vItemManager;

using EviLA.AddOns.RPGPack.Experience.Events;

namespace EviLA.AddOns.RPGPack.Experience
{
    [Serializable, vClassHeader("Level Manager", true, "icon_v2")]
    public partial class vLevelManager : MonoBehaviour
    {
        [vEditorToolbar("Settings")]
        public double baseXP;
        public int maximumLevel;
        public bool updateStatsWithLevelUp;
        public GameObject statTrends;
        public bool debugMode;

        [SerializeField]
        public ExperienceFunctionType experienceFunction;

        [vEditorToolbar("Events")]
        public OnLevelUp onLevelUp;
        public OnGainXP onGainXP;
        public OnGainStats onGainStats;


        private int currentLevel = 1;
        private double currentExperience;
        private double requiredXPForNextLevel = 0d;
        private double requiredXPForPreviousLevel = 0d;

        private static vLevelManager instance;
        private IExperienceCalculatorStrategy requiredXPCalculator;
        private List<StatTrend> trends = new List<StatTrend>();
        private List<StatComponent> currentStats = new List<StatComponent>();

        private bool initialized = false;


        public int CurrentLevel
        {
            get { return currentLevel; }
            set { currentLevel = value; }
        }

        public int NextLevel
        {
            get { return currentLevel + 1; }
        }

        public double CurrentExperience
        {
            get { return currentExperience; }
            set { currentExperience = value; }
        }

        public double RequiredXPForNextLevel
        {
            get { return requiredXPForNextLevel; }
            set { requiredXPForNextLevel = value; }
        }

        public double RequiredXPForPreviousLevel
        {
            get { return requiredXPForPreviousLevel; }
            set { requiredXPForPreviousLevel = value; }
        }

        public List<StatTrend> StatTrends
        {
            get
            {
                return trends;
            }
        }

        public List<StatComponent> CurrentStats
        {
            get
            {
                return currentStats;
            }
        }

        public void Awake()
        {
            if (instance != null)
                return;

            instance = this;


            DontDestroyOnLoad(instance);

        }

        public void Start()
        {

            requiredXPCalculator = GetExperienceFunction(experienceFunction);
            requiredXPCalculator.SetBaseXP(baseXP);

            if (debugMode)
                requiredXPCalculator.DisplaySampleLevels();

            requiredXPForPreviousLevel = baseXP;
            requiredXPForNextLevel = Math.Ceiling(requiredXPCalculator.CalculateRequiredXPForNextLevel(NextLevel));

            var genericAction = GetComponent<vGenericAction>();

            genericAction.OnDoAction.AddListener(AddExperience);
            genericAction.OnDoAction.AddListener(UpdateStats);

            Experience.UI.vExperienceHUD.instance.InitXPSlider(0);

            var itemManager = GameObject.FindObjectOfType<vItemManager>();

            Experience.UI.vExperienceHUD.instance.UpdateXPSlider();
        }


        public void ChangeLevel(int level)
        {
            LevelUp(level);
        }

        private void LevelUp(int explicitLevel = 0)
        {


            if (explicitLevel != 0)
            {
                if (CurrentLevel < explicitLevel)
                {
                    CurrentLevel = explicitLevel;
                }
            }
            else
            {
                CurrentLevel = ++CurrentLevel;
            }

            if (CurrentLevel > maximumLevel)
            {
                CurrentLevel = maximumLevel;
                return;
            }

            requiredXPForPreviousLevel = requiredXPForNextLevel;
            requiredXPForNextLevel = Math.Ceiling(requiredXPCalculator.CalculateRequiredXPForNextLevel(NextLevel));

            onLevelUp.Invoke(CurrentLevel);

            if (updateStatsWithLevelUp)
            {
                UpdateStats(currentStats, true);
                onGainStats.Invoke(currentStats);
            }

        }

        private void InitStats(List<StatTrend> trends)
        {
            foreach (var trend in trends)
            {

                IStatStrategy strategy = GetStatHandler(trend.type);
                var stats = strategy.Initialize(trend);
                currentStats.AddRange(stats);
            }

            initialized = true;
        }

        private void UpdateStats(vTriggerGenericAction action)
        {
            var statComponents = action.GetComponents<StatComponent>().vToList();

            if (statComponents != null && statComponents.Count > 0)
            {
                if (currentLevel == maximumLevel)
                    return;

                statComponents.RemoveAll(statC => trends.Find(trend => trend.type != statC.type) == null);

                UpdateStats(statComponents);

                onGainStats.Invoke(statComponents);

            }
        }

        private void UpdateStats(List<StatComponent> stats, bool levelUp = false)
        {
            if (!initialized)
            {
                if (statTrends != null)
                {
                    trends = statTrends.GetComponents<StatTrend>().vToList();
                    if (trends != null)
                    {
                        InitStats(trends);
                    }
                }
            }

            foreach (var stat in stats)
            {
                var currentStat = currentStats.Find(s => s.type == stat.type && (levelUp) ? s.applyOnLevelUp : true);

                IStatStrategy strategy = GetStatHandler(stat.type);

                strategy.CalculateStat(CurrentLevel, stat, levelUp, requiredXPCalculator);
                strategy.ApplyStat(stat);

                if (stat.destroyAfterUse)
                    Destroy(stat);
            }

            ResetEquippedWeapon();
        }

        public void SetStats(List<StatComponent> stats)
        {
            currentStats.Clear();
            currentStats = stats;

            foreach (var stat in currentStats)
            {
                IStatStrategy strategy = GetStatHandler(stat.type);
                strategy.ApplyStat(stat);
                if (stat.destroyAfterUse)
                    Destroy(stat);
            }

            ResetEquippedWeapon();

            initialized = true;
        }

        private void ResetEquippedWeapon()
        {
            //Will reset stats on any equipped weapon
            var itemManager = vThirdPersonController.instance.GetComponent<vItemManager>();

            var em = itemManager.GetCurrentlyEquippedWeapon(vItemType.MeleeWeapon);


// IF YOU DON'T HAVE THE SHOOTER INSTALLED
// COMMENT THE CODE WITHIN THE SECTION #IF INVECTOR_SHOOTER #ENDIF 

#if INVECTOR_SHOOTER
            if (em == null)
                em = itemManager.GetCurrentlyEquippedWeapon(vItemType.Shooter);
#endif
            if (em != null)
            {
                ApplyStatOnInstantiateWeapon(em.equipmentReference.equipedObject);
            }
        }

        private void AddExperience(vTriggerGenericAction action)
        {
            if (currentLevel == maximumLevel)
                return;

            var xpComponent = action.GetComponent<ExperienceComponent>();
            if (xpComponent != null)
            {
                AddExperience(xpComponent.experienceCost);
                if (xpComponent.destroyAfterUse)
                    Destroy(xpComponent);
            }
        }

        public void AddExperience(int experience)
        {
            if (experience <= 0)
                return;

            CurrentExperience += experience;


            if (CurrentExperience > requiredXPForNextLevel - requiredXPForPreviousLevel)
            {
                do
                {
                    CurrentExperience = CurrentExperience % (requiredXPForNextLevel - requiredXPForPreviousLevel);
                    LevelUp();
                } while (CurrentExperience > requiredXPForNextLevel - requiredXPForPreviousLevel);

                vHUDController.instance.ShowText("You have reached Level " + currentLevel);

            }
            else
            {
                onGainXP.Invoke(experience);
                vHUDController.instance.ShowText(experience.ToString() + " XP Gained");
            }

            Experience.UI.vExperienceHUD.instance.UpdateXPSlider();
        }

        public void ApplyStatOnInstantiateWeapon(GameObject obj)
        {
            if (obj.GetComponent<vMeleeWeapon>() != null)
            {
                var statComps = obj.GetComponents<MeleeWeaponStatComponent>().vToList();
                if (statComps != null)
                {
                    var stats = new List<StatComponent>();

                    statComps.ForEach(statc =>
                    {
                        var current = currentStats.Find(stat => stat.trendID.Equals(statc.trendID));
                        if (current is MeleeWeaponStatComponent)
                        {
                            var statRef = current as MeleeWeaponStatComponent;
                            var statHandler = GetStatHandler(current.type) as MeleeWeaponDamageStrategy;
                            statHandler.ApplyStat(statRef, obj);
                        }
                    });
                }
            }


// IF YOU DON'T HAVE THE SHOOTER INSTALLED
// COMMENT THE CODE WITHIN THE SECTION #IF INVECTOR_SHOOTER #ENDIF 

#if INVECTOR_SHOOTER

            if (obj.GetComponent<Invector.vShooter.vShooterWeapon>() != null)
            {
                var statComps = obj.GetComponents<ShooterWeaponStatComponent>().vToList();
                if (statComps != null)
                {
                    var stats = new List<StatComponent>();

                    statComps.ForEach(statc =>
                    {
                        var current = currentStats.Find(stat => stat.trendID.Equals(statc.trendID));
                        if (current is ShooterWeaponStatComponent)
                        {
                            var statRef = current as ShooterWeaponStatComponent;
                            var statHandler = GetStatHandler(current.type) as ShooterWeaponDamageStrategy;
                            statHandler.ApplyStat(statRef, obj);
                        }
                    });
                }
            }
#endif

        }
    }
}
