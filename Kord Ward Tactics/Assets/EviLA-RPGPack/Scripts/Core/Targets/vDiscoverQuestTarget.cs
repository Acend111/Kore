using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;
using System;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class vDiscoverQuestTarget : vQuestTarget
    {
        private bool triggered = false;

        void OnTriggerEnter(Collider other)
        {

            if (triggered)
                return;

            var questSystem = vQuestSystemManager.Instance;
            if (!triggered && questSystem.GetQuestState(quest.id) == vQuestState.InProgress)
            {
                if (quest.type == vQuestType.Discover && other.CompareTag("Player"))
                {
                    var collider = this.GetComponent<BoxCollider>();
                    OnTargetAction();
                    triggered = true;
                }
            }
        }
    }
}

