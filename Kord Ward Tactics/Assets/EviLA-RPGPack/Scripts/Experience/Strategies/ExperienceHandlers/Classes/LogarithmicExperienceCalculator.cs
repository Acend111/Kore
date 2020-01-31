using System;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{
    // a modified version of https://gamedev.stackexchange.com/questions/55151/rpg-logarithmic-leveling-formula
    public class LogarithmicExperienceCalculator : AbstractExperienceCalculator
    {
        const double constA = 4.5d; 
        const double constB = -40d;
        const double constC = 50d;
        const double levelMultiplier = 1;


        public override double CalculateRequiredXPForNextLevel(int currentLevel)
        {
            double lowerBound = Math.Exp((((double)currentLevel * levelMultiplier) - constB) / constA) - constC;
            double upperBound = Math.Exp((((double)(currentLevel * levelMultiplier + 1)) - constB) / constA) - constC;

            var value = upperBound - lowerBound - baseXP;

            incrementFactor = value / baseXP;

            previousLevelXP = value;

            return value;
        }

        public override void DisplaySampleLevels()
        {
            int maximumLevel = GameObject.FindObjectOfType<vLevelManager>().maximumLevel;

            for (int i = 1; i <= maximumLevel; i++)
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