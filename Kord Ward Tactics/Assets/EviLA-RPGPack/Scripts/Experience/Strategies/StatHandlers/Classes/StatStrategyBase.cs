using Invector.vCharacterController;
using System;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class StatStrategyBase
    {
        public virtual void CalculateStat(int currentLevel, StatComponent stat, bool levelUp, IExperienceCalculatorStrategy calculator)
        {
            var levelManager = vThirdPersonController.instance.GetComponent<vLevelManager>();

            var calc = calculator as AbstractExperienceCalculator;

            int maxLevels = levelManager.maximumLevel;
            StatComponent currentStat = levelManager.CurrentStats.Find(c => c.trendID.Equals(stat.trendID));
            float trend = levelManager.StatTrends.Find(t => t.trendID.Equals(stat.trendID)).trend.Evaluate(currentLevel / maxLevels);

            double currentStatValue = Double.Parse(currentStat.value);
            double statValue = Double.Parse(stat.value);

            if (levelUp && levelManager.updateStatsWithLevelUp)
                currentStatValue = statValue * calc.IncrementFactor * trend;
            else if (stat.isPercentage)
            {
                if (statValue <= 0 || statValue > 100)
                    throw new Exception("Stat can't be negative, 0 or greater than 100 if used as a percentage");
                currentStatValue += (currentStatValue * statValue / 100.0d);
            }
            else
            {
                currentStatValue += statValue;
            }

            currentStat.value = currentStatValue.ToString();
        }

    }
}
