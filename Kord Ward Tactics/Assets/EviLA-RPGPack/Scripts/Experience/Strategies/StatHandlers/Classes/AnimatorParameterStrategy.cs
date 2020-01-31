using System.Collections.Generic;
using Invector.vCharacterController;
using UnityEngine;
using System;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Experience
{
    class AnimatorParameterStrategy : StatStrategyBase, IStatStrategy
    {
        public override void CalculateStat(int currentLevel, StatComponent stat, bool levelUp, IExperienceCalculatorStrategy calculator)
        {
            var animStat = stat as AnimatorStatComponent;
            if (!levelUp)
                return;

            if (animStat.incrementOnLevelUp)
            {
                if (animStat.parameterType != AnimatorControllerParameterType.Float || animStat.parameterType != AnimatorControllerParameterType.Int)
                    throw new Exception("Cannot apply increment factor to non numeric animation state parameters");

                animStat.value = (int.Parse(animStat.value) + animStat.incrementFactor).ToString();
                stat = animStat;
            }
        }

        public void ApplyStat(StatComponent stat)
        {
            if (stat is AnimatorStatComponent)
            {
                var animatorStat = stat as AnimatorStatComponent;
                var animator = vThirdPersonController.instance.GetComponent<Animator>();


                switch (animatorStat.parameterType)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(animatorStat.parameterName, float.Parse(animatorStat.value));
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(animatorStat.parameterName, int.Parse(animatorStat.value));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(animatorStat.parameterName, bool.Parse(animatorStat.value));
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        animator.SetTrigger(animatorStat.parameterName);
                        break;
                    default:
                        throw new System.Exception("This exception will never be reached unless Unity has introduced a new animator controller parameter type. :) ");
                }

            }
        }

        public List<StatComponent> Initialize(StatTrend trend)
        {
            List<StatComponent> stats = new List<StatComponent>();

            var statC = new AnimatorStatComponent();

            var animatorTrend = trend as AnimatorStatTrend;

            statC.trendID = animatorTrend.trendID;
            statC.type = animatorTrend.type;
            statC.applyOnLevelUp = animatorTrend.applyOnLevelUp;
            statC.parameterName = animatorTrend.parameterName;
            statC.parameterType = animatorTrend.parameterType;
            statC.incrementOnLevelUp = animatorTrend.incrementOnLevelUp;
            statC.incrementFactor = animatorTrend.incrementFactor;

            stats.Add(statC);

            return stats;

        }


        public StatComponent Initialize(StatComponentSerialized stat, StatTrend animatorTrend)
        {
            var trend = animatorTrend as AnimatorStatTrend;

            return new AnimatorStatComponent()
            {
                type = stat.type,
                trendID = stat.trendID,
                isNumeric = stat.isNumeric,
                isPercentage = stat.isPercentage,
                applyOnLevelUp = stat.applyOnLevelUp,
                parameterName = trend.parameterName,
                parameterType = trend.parameterType,
                value = stat.value
            };
        }
    }
}
