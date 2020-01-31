using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Invector.vItemManager;
using System;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class vTriggerCountDown : vTriggerGenericAction
    {
        [SerializeField]
        public vQuest quest;

        private bool triggered = false;

        protected override void Start()
        {
            base.Start();
            this.OnDoAction.AddListener(StartCountDown);
        }
        public void StartCountDown()
        {
            if (triggered)
                return;

            if (vQuestSystemManager.Instance.IsScriptedCountDownEnabled(quest.id))
            {
                var questManager = vThirdPersonController.instance.GetComponent<vQuestManager>();
                questManager.StartCountDown(quest.id);
                triggered = true;
            }
        }
    }
}

