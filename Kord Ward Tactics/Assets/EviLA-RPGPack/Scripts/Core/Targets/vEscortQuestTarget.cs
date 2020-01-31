using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;
using System;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class vEscortQuestTarget : vQuestTarget
    {
        private string _tag;
        private int _tagCount;
        private bool _targetCannotLeaveArea;
        public bool destroyAfterEscortQuest;

        void Awake()
        {
            var instance = vQuestSystemManager.Instance;
            _tagCount = instance.GetQuestAmount(quest.id);
            _targetCannotLeaveArea = instance.TargetsCannotLeaveArea(quest.id);
            _tag = instance.GetTag(quest.id);
        }

        void OnTriggerEnter(Collider other)
        {

            var instance = vQuestSystemManager.Instance;

            if (instance.GetQuestType(quest.id) == vQuestType.Escort && instance.GetQuestState(quest.id) == vQuestState.InProgress && other.CompareTag(_tag))
            {

                int currentTagCount = instance.GetTaggedCount(quest.id);
                ++currentTagCount;
                instance.UpdateAttributeValue(quest.id, vQuestAttributes.QuestAmount, currentTagCount);

                OnTargetAction();
                if (currentTagCount == _tagCount)
                    if (destroyAfterEscortQuest)
                        Destroy(this);
            }
        }

        void OnTriggerExit(Collider other)
        {

            var instance = vQuestSystemManager.Instance;

            if (instance.GetQuestType(quest.id) == vQuestType.Escort && instance.GetQuestState(quest.id) == vQuestState.InProgress && _targetCannotLeaveArea
                    && other.CompareTag(_tag))
            {
                int currentTagCount = instance.GetTaggedCount(quest.id);
                if (currentTagCount != 0)
                {
                    --currentTagCount;
                    instance.UpdateAttributeValue(quest.id, vQuestAttributes.QuestAmount, currentTagCount);
                }
                OnTargetAction();
            }
        }
    }

}

