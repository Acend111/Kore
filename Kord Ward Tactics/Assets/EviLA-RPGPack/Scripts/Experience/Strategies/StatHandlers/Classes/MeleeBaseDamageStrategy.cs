using System;
using System.Collections.Generic;
using Invector.vCharacterController;
using Invector.vMelee;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class MeleeBaseDamageStrategy : StatStrategyBase, IStatStrategy
    {
        public void ApplyStat(StatComponent stat)
        {
            if (!stat.isNumeric)
                throw new Exception("The Base Damage stat can only be maintained as a numerical value");

            var meleeManager = vThirdPersonController.instance.GetComponent<vMeleeManager>();

            meleeManager.defaultDamage.damageValue = (int)double.Parse(stat.value);

        }

        public List<StatComponent> Initialize(StatTrend meleeBaseDamageTrend)
        {
            var trend = meleeBaseDamageTrend as MeleeBaseDamageTrend;

            List<StatComponent> stats = new List<StatComponent>();

            var statC = new StatComponent();
            statC.trendID = trend.trendID;
            statC.isNumeric = true;
            statC.isPercentage = trend.isPercentage;
            statC.type = trend.type;
            statC.applyOnLevelUp = trend.applyOnLevelUp;
            statC.value = trend.value;

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
