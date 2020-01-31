using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class AnimatorStatTrend : StatTrend
    {
        public string parameterName;
        public AnimatorControllerParameterType parameterType;
        public bool isTrigger;
        public bool incrementOnLevelUp;
        public int incrementFactor;

        public void Awake()
        {
            type = BaseStatTypes.AnimatorParameter;
            parameterType = AnimatorControllerParameterType.Int;
        }
    }
}
