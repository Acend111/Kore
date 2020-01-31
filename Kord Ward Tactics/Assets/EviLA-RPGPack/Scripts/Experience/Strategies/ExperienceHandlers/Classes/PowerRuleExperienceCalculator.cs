using System;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{
    // https://gamedev.stackexchange.com/questions/20934/how-to-create-adjustable-formula-for-rpg-level-up-requirements
    public class PowerRuleExperienceCalculator : AbstractExperienceCalculator
    {
        public override double CalculateRequiredXPForNextLevel(int currentLevel)
        {
            int maxLevels = GameObject.FindObjectOfType<vLevelManager>().maximumLevel;

            double xpFirstLevel = this.baseXP;
            double xpLastLevel = 10000;

            double level = currentLevel;

            double b = Math.Log10((double)xpLastLevel / xpFirstLevel) / (maxLevels - 1);
            double a = (double)xpFirstLevel / (Math.Exp(b) - 1.0);

            double old_xp = Math.Round(a * Math.Exp(b * (level - 1)));
            double new_xp = Math.Round(a * Math.Exp(b * level));

            double value = new_xp - old_xp;
            incrementFactor = value / baseXP;

            previousLevelXP = value;

            return value;

        }

        public override void DisplaySampleLevels()
        {
            int maxLevels = GameObject.FindObjectOfType<vLevelManager>().maximumLevel;

            for (int i = 1; i <= maxLevels; i++)
            {
                var current = Math.Ceiling(CalculateRequiredXPForNextLevel(i));
                var previous = 0d;

                if (i > 1)
                {
                    previous = Math.Ceiling(CalculateRequiredXPForNextLevel(i - 1));
                    var difference = Math.Ceiling(current - previous);
                    Debug.Log("Level " + i + " = " + current + " Difference : " + difference);
                }

                else
                {
                    Debug.Log("Level " + i + " = " + current);
                }
            }
        }
    }
}