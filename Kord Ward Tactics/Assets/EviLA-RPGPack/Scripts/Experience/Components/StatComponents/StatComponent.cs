using System;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{
    [Serializable]
    public class StatComponent : MonoBehaviour
    {
        public string trendID;
        public BaseStatTypes type;
        public bool isNumeric;
        public bool isBool;
        public bool destroyAfterUse;
        [HideInInspector]
        public bool isPercentage;
        [HideInInspector]
        public string value;

        [HideInInspector]
        public bool applyOnLevelUp;

        public virtual void ApplyStat() { }
    }
}