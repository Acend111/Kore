
// IF YOU DON'T HAVE THE SHOOTER INSTALLED
// COMMENT THE CONTENTS OF THIS FILE

using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using System;

using Invector;
using Invector.vItemManager;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Experience

{
    public class ShooterWeaponDamageStrategy : StatStrategyBase, IStatStrategy
    {
        public void ApplyStat(StatComponent stat)
        {
            /* do not apply stats here. Apply via OnInstantiateObject in item manager */
        }

        public void ApplyStat(StatComponent stat, GameObject obj)
        {
#if INVECTOR_SHOOTER
            if (!stat.isNumeric)
                throw new Exception("Stat needs to be numeric");
            
            var mwstat = stat as ShooterWeaponStatComponent;
            var weapon = obj.GetComponent<Invector.vShooter.vShooterWeapon>();
            var projectileControl = weapon.projectile.GetComponent<Invector.vShooter.vProjectileControl>();


            var lvlManager = vThirdPersonController.instance.GetComponent<vLevelManager>();
            var trends = lvlManager.statTrends.gameObject.GetComponents<StatTrend>().vToList();
            var thisTrend = trends.Find(trend => trend.trendID.Equals(stat.trendID)) as ShooterWeaponDamageTrend;

            if (thisTrend == null)
                throw new Exception("Unable to find the trend in the stats holder in Level Manager");

            switch (thisTrend.monitoredStat)
            {
                case ShooterWeaponStatTypes.MinDamage:
                    weapon.minDamage = (int)Double.Parse(stat.value);
                    break;
                case ShooterWeaponStatTypes.MaxDamage:
                    weapon.maxDamage = (int)Double.Parse(stat.value);
                    break;
                default:
                    break;
            }

#endif
        }


        public List<StatComponent> Initialize(StatTrend meleeWeaponDamageTrend)
        {

            var trend = meleeWeaponDamageTrend as ShooterWeaponDamageTrend;

            var stats = new List<ShooterWeaponStatComponent>();

#if INVECTOR_SHOOTER

            var itemManager = vThirdPersonController.instance.gameObject.GetComponent<vItemManager>();
            var weapons = itemManager.items.FindAll(item => item.type == vItemType.Shooter);

            foreach (var weapon in weapons)
            {
                var statComponents = weapon.originalObject.GetComponents<ShooterWeaponStatComponent>().vToList();
                var shooterWeapon = weapon.originalObject.GetComponent<Invector.vShooter.vShooterWeapon>();

                foreach (var statComponent in statComponents)
                {
                    if (statComponent.trendID.Equals(trend.trendID))
                    {
                        var statC = new ShooterWeaponStatComponent();
                        switch (trend.monitoredStat)
                        {
                            case ShooterWeaponStatTypes.MinDamage:
                                statC.value = shooterWeapon.minDamage.ToString();
                                break;
                            case ShooterWeaponStatTypes.MaxDamage:
                                statC.value = shooterWeapon.maxDamage.ToString();
                                break;
                            default:
                                break;
                        }
                        stats.Add(statC);
                    }
                }
            }

#endif

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


        public StatComponent Initialize(StatComponentSerialized stat, StatTrend shooterWeaponDamageTrend)
        {
            var trend = shooterWeaponDamageTrend as ShooterWeaponDamageTrend;

            return new ShooterWeaponStatComponent()
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