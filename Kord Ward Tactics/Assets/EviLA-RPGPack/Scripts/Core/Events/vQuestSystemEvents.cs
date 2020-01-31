using UnityEngine;
using System.Collections;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class OnOpenCloseQuestsWindow : UnityEngine.Events.UnityEvent<bool> { }
    [System.Serializable]
    public class OnOpenCloseQuestManagerUI : UnityEngine.Events.UnityEvent<bool> { }
    [System.Serializable]
    public class OnOpenCloseQuestProviderUI : UnityEngine.Events.UnityEvent<bool> { }
    [System.Serializable]
    public class OnHandleQuestEvent : UnityEngine.Events.UnityEvent<vQuest> { }
    [System.Serializable]
    public class OnAcceptQuestEvent : UnityEngine.Events.UnityEvent<vQuest,bool,bool> { }
    [System.Serializable]
    public class OnDeclineQuestEvent : UnityEngine.Events.UnityEvent<vQuest> { }
    [System.Serializable]
    public class OnCompleteQuestEvent : UnityEngine.Events.UnityEvent<vQuest> { }
    [System.Serializable]
    public class OnProviderVendorTargetActionEvent : UnityEngine.Events.UnityEvent<vQuest,vQuestProvider,IQuestTarget> { }
    [System.Serializable]
    public class OnInstantiateItemObjectEvent : UnityEngine.Events.UnityEvent<GameObject> { }
    [System.Serializable]
    public class OnQuestItemAquire : UnityEngine.Events.UnityEvent<vQuest> { }
	[System.Serializable]
	public class OnQuestFailureEvent : UnityEngine.Events.UnityEvent<vQuest> { }
}
