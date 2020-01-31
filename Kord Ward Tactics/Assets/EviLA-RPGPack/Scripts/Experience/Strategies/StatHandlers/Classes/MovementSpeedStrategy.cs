using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EviLA.AddOns.RPGPack.Experience

{
    public class MovementSpeedStrategy : StatStrategyBase, IStatStrategy
    {
        public void ApplyStat(StatComponent stat)
        {
            if (!stat.isNumeric)
                throw new Exception("The Speed stat can only be maintained as a numerical value");

            if (!stat.isPercentage)
                throw new Exception("The Speed stat can only be maintained as a percentage");

            double statValue = double.Parse(stat.value);

            vThirdPersonController.instance.freeSpeed.runningSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.freeSpeed.sprintSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.freeSpeed.walkSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.freeSpeed.crouchSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.speed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.strafeSpeed.crouchSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.strafeSpeed.runningSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.strafeSpeed.walkSpeed *= ((float)statValue / 100.0f);
            vThirdPersonController.instance.strafeSpeed.sprintSpeed *= ((float)statValue / 100.0f);

        }

        public List<StatComponent> Initialize(StatTrend trend)
        {
            var stats = new List<StatComponent>();

            var statC = new StatComponent();
            statC.trendID = trend.trendID;
            statC.isNumeric = true;
            statC.type = trend.type;
            statC.isPercentage = trend.isPercentage;
            statC.applyOnLevelUp = trend.applyOnLevelUp;
            stats.Add(statC);

            return stats;
        }

        public StatComponent Initialize(StatComponentSerialized stat, StatTrend meleeWeaponDamageTrend)
        {
            var trend = meleeWeaponDamageTrend as ShooterWeaponDamageTrend;

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
