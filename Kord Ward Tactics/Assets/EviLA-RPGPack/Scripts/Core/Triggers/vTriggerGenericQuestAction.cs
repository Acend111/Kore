﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Invector;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using UnityEngine.UI;

namespace EviLA.AddOns.RPGPack
{
    [vClassHeader("Trigger Quest Action", "Trigger Quest Action", iconName = "icon_v2")]
    public class vTriggerGenericQuestAction : vMonoBehaviour
    {
        [vEditorToolbar("Quest")]
        public vQuest quest;
        public vQuestTriggerType triggerType;
        [HideInInspector]
        public bool triggeredOnce = false;


        [vEditorToolbar("Options")]

        [Tooltip("Automatically execute the action without the need to press a Button")]
        public bool autoAction;
        [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
        public string playAnimation;
        [Tooltip("Use a transform to help the character climb any height, take a look at the Example Scene ClimbUp, StepUp, JumpOver objects.")]
        public Transform matchTarget;
        [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
        public string exitAnimation;
        [Tooltip("Start the match target of the animation")]
        public float startMatchTarget;
        [Tooltip("End the match target of the animation")]
        public float endMatchTarget;
        [Tooltip("Use this to limit the trigger to active if forward of character is close to this forward")]
        public bool activeFromForward;
        [Tooltip("Rotate Character for this rotation when active")]
        public bool useTriggerRotation;
        [Tooltip("Action Delay")]
        public float ActionDelay;

        [vEditorToolbar("Events")]
        public UnityEvent OnDoAction;
        public UnityEvent OnPlayerEnter;
        public UnityEvent OnPlayerStay;
        public UnityEvent OnPlayerExit;

    }
}

