using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class AnimatorStatComponent : StatComponent
    {
        [HideInInspector]
        public string parameterName;
        [HideInInspector]
        public AnimatorControllerParameterType parameterType;
        [HideInInspector]
        public bool isTrigger;
        [HideInInspector]
        public bool incrementOnLevelUp;
        [HideInInspector]
        public int incrementFactor;
    }
}
