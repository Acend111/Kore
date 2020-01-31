using System.Collections.Generic;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Experience
{
    public interface IStatStrategy
    {
        void CalculateStat(int currentLevel, StatComponent stat, bool levelUp, IExperienceCalculatorStrategy calculator);
        void ApplyStat(StatComponent stat);
        List<StatComponent> Initialize(StatTrend trend);
        StatComponent Initialize(StatComponentSerialized stat, StatTrend trend);
    }
}
