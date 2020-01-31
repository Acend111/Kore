using System.Collections.Generic;
using UnityEngine;
using System;

using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using Invector.vMelee;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Experience

{
    public class MeleeWeaponDamageStrategy : StatStrategyBase, IStatStrategy
    {
        public void ApplyStat(StatComponent stat)
        {
            /* do not apply stats here. Apply via OnInstantiateObject in item manager */
        }

        public void ApplyStat(StatComponent stat, GameObject obj)
        {
            if (!stat.isNumeric)
                throw new Exception("Stat needs to be numeric");

            var mwstat = stat as MeleeWeaponStatComponent;
            var weapon = obj.GetComponent<vMeleeWeapon>();
            var damage = weapon.damage;

            var lvlManager = vThirdPersonController.instance.GetComponent<vLevelManager>();
            var trends = lvlManager.statTrends.gameObject.GetComponents<StatTrend>().vToList();
            var thisTrend = trends.Find(trend => trend.trendID.Equals(stat.trendID)) as MeleeWeaponDamageTrend;

            if (thisTrend == null)
                throw new Exception("Unable to find the trend in the stats holder in Level Manager");

            switch (thisTrend.monitoredStat)
            {
                case MeleeWeaponStatTypes.DamageValue:
                    damage.damageValue = (int)Double.Parse(stat.value);
                    break;
                case MeleeWeaponStatTypes.StaminaBlockCost:
                    if (stat.isPercentage)
                        damage.staminaBlockCost = (int)Double.Parse(stat.value);
                    break;
                case MeleeWeaponStatTypes.StaminaRecoveryDelay:
                    damage.staminaRecoveryDelay = (int)Double.Parse(stat.value);
                    break;
                //case MeleeWeaponStatTypes.IgnoreDefense:
                //    damage.ignoreDefense = bool.Parse(stat.value);
                //    break;
                default:
                    break;
            }
        }


        public List<StatComponent> Initialize(StatTrend meleeWeaponDamageTrend)
        {
            var trend = meleeWeaponDamageTrend as MeleeWeaponDamageTrend;

            var stats = new List<MeleeWeaponStatComponent>();

            var itemManager = vThirdPersonController.instance.gameObject.GetComponent<vItemManager>();
            var weapons = itemManager.items.FindAll(item => item.type == vItemType.MeleeWeapon);

            foreach (var weapon in weapons)
            {
                var statComponents = weapon.originalObject.GetComponents<MeleeWeaponStatComponent>().vToList();
                var meleeWeapon = weapon.originalObject.GetComponent<vMeleeWeapon>();

                foreach (var statComponent in statComponents)
                {
                    if (statComponent.trendID.Equals(trend.trendID))
                    {
                        var statC = new MeleeWeaponStatComponent();
                        switch (trend.monitoredStat)
                        {
                            case MeleeWeaponStatTypes.DamageValue:
                                statC.value = meleeWeapon.damage.damageValue.ToString();
                                break;
                            case MeleeWeaponStatTypes.StaminaBlockCost:
                                statC.value = meleeWeapon.damage.staminaBlockCost.ToString();
                                break;
                            case MeleeWeaponStatTypes.StaminaRecoveryDelay:
                                statC.value = meleeWeapon.damage.staminaRecoveryDelay.ToString();
                                break;
                            //case MeleeWeaponStatTypes.IgnoreDefense:
                            //    statC.value = meleeWeapon.damage.ignoreDefense.ToString();
                            //    break;
                            default:
                                break;
                        }
                        stats.Add(statC);
                    }
                }
            }

            var statRet = new List<StatComponent>();

            stats.ForEach(stat =>
            {
                stat.trendID = trend.trendID;
                stat.isNumeric = true;
                stat.isPercentage = trend.isPercentage;
                stat.applyOnLevelUp = trend.applyOnLevelUp;
                stat.type = trend.type;
                statRet.Add(stat);
            });

            return statRet;
        }

        public StatComponent Initialize(StatComponentSerialized stat, StatTrend meleeWeaponDamageTrend)
        {
            var trend = meleeWeaponDamageTrend as MeleeWeaponDamageTrend;

            return new MeleeWeaponStatComponent()
            {
                type = stat.type,
                trendID = stat.trendID,
                isNumeric = stat.isNumeric,
                isPercentage = stat.isPercentage,
                applyOnLevelUp = stat.applyOnLevelUp,
                value = stat.value
            };
        }
    }
}