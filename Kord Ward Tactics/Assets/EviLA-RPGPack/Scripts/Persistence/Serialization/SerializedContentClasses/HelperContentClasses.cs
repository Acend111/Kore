using Invector.vItemManager;
using EviLA.AddOns.RPGPack.Experience;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    [Serializable]
    public class LastCheckPoint
    {
        public string scene;
        public string checkpointName;
    }

    [Serializable]
    public class StatComponentSerialized
    {
        public string trendID;
        public BaseStatTypes type;
        public bool isPercentage;
        public bool isNumeric;
        public bool isBool;
        public string value;
        public bool destroyAfterUse;
        public bool applyOnLevelUp;
    }

    [Serializable]
    public class AnimatorStateInformation
    {
        public int layer;
        public int nameHash;
        public float layerWeight;
        public float currentTimeOfAnimation;
    }

    [Serializable]
    public class AnimationStateInformation
    {
        public string name;
        public float normalizedSpeed;
        public float normalizedTime;
    }

    [Serializable]
    public class AudioStateInformation
    {
        public float normalizedTime;
    }
}
