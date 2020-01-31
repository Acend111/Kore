using System;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack.Experience
{
    public partial class vLevelManager : MonoBehaviour
    {

        private IExperienceCalculatorStrategy GetExperienceFunction(ExperienceFunctionType type)
        {
            IExperienceCalculatorStrategy strategy;
            switch (type)
            {
                case ExperienceFunctionType.PowerRule:
                    strategy = new PowerRuleExperienceCalculator();
                    break;
                case ExperienceFunctionType.Logarithmic:
                    strategy = new LogarithmicExperienceCalculator();
                    break;
                default:
                    throw new Exception("Experience Function not yet implemented for given Experience Function Type");
            }

            return strategy;
        }

        public static IStatStrategy GetStatHandler(BaseStatTypes type)
        {
            IStatStrategy strategy;
            switch (type)
            {
                case BaseStatTypes.Health:
                    strategy = new MaxHealthStrategy();
                    break;
                case BaseStatTypes.Stamina:
                    strategy = new MaxStaminaStrategy();
                    break;
                case BaseStatTypes.MeleeBaseDamage:
                    strategy = new MeleeBaseDamageStrategy();
                    break;
                case BaseStatTypes.MovementSpeed:
                    strategy = new MovementSpeedStrategy();
                    break;
                case BaseStatTypes.AnimatorParameter:
                    strategy = new AnimatorParameterStrategy();
                    break;
                case BaseStatTypes.MeleeWeapon:
                    strategy = new MeleeWeaponDamageStrategy();
                    break;
                case BaseStatTypes.ShooterWeapon:
                    strategy = new ShooterWeaponDamageStrategy();
                    break;
                default:
                    throw new Exception("Stat Handler not implemented for given Stat Type");
            }

            return strategy;
        }
    }
}
