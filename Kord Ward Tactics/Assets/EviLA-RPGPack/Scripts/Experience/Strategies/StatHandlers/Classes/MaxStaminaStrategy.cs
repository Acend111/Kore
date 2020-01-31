using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class MaxStaminaStrategy : StatStrategyBase, IStatStrategy
    {
        public void ApplyStat(StatComponent stat)
        {
            if (!stat.isNumeric)
                throw new Exception("The Maximum Stamina stat can only be maintained as a numerical value");

            vThirdPersonController.instance.ChangeMaxStamina((int)double.Parse(stat.value) - (int)vThirdPersonController.instance.maxStamina);
        }

        public List<StatComponent> Initialize(StatTrend staminaTrend)
        {
            var trend = staminaTrend as MaxStaminaTrend;

            List<StatComponent> stats = new List<StatComponent>();

            var statC = new StatComponent();
            statC.trendID = trend.trendID;
            statC.isNumeric = true;
            statC.type = trend.type;
            statC.isPercentage = trend.isPercentage;
            statC.applyOnLevelUp = trend.applyOnLevelUp;

            statC.value = vThirdPersonController.instance.maxStamina.ToString();

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
