using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence.Serialization;
using System;
using System.Collections.Generic;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class MaxHealthStrategy : StatStrategyBase, IStatStrategy
    {
        public void ApplyStat(StatComponent stat)
        {
            if (!stat.isNumeric)
                throw new Exception("The Maximum Health stat can only be maintained as a numerical value");

            vThirdPersonController.instance.ChangeMaxHealth((int)double.Parse(stat.value) - (int)vThirdPersonController.instance.maxHealth);
        }

        public List<StatComponent> Initialize(StatTrend healthTrend)
        {
            var trend = healthTrend as MaxHealthTrend;

            List<StatComponent> stats = new List<StatComponent>();
            var statC = new StatComponent();
            statC.trendID = trend.trendID;
            statC.isNumeric = true;
            statC.type = trend.type;
            statC.applyOnLevelUp = trend.applyOnLevelUp;
            statC.isPercentage = trend.isPercentage;

            statC.value = vThirdPersonController.instance.maxHealth.ToString();

            stats.Add(statC);

            return stats;
        }

        public StatComponent Initialize(StatComponentSerialized stat, StatTrend trend)
        {
            return new StatComponent()
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
