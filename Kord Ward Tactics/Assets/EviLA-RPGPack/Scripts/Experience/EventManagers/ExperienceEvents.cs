using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace EviLA.AddOns.RPGPack.Experience.Events
{
    [Serializable]
    public class OnLevelUp : UnityEvent<int> { }

    [Serializable]
    public class OnGainXP : UnityEvent<int> { }

    [Serializable]
    public class OnGainStats : UnityEvent<List<StatComponent>> { }
    
}
