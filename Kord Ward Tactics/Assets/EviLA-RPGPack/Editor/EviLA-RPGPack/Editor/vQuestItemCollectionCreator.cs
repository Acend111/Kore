using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using EviLA.AddOns.RPGPack;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

public class vQuestItemCollectionCreator : EditorWindow
{
    GUISkin skin;

    GameObject itemCollection;
    vQuest quest;
    vQuestTarget target;
    vQuestManager questManager;
    vItemManager itemManager;

    Vector2 rect = new Vector2(500, 300);
    Vector2 scrool;

    /// <summary>
    /// 3rdPersonController Menu 
    /// </summary>    
	[MenuItem("Invector/Quests/Items/Create Quest Item Collection", false, 1)]
    public static void CreateNewItemCollection()
    {
        GetWindow<vQuestItemCollectionCreator>();
    }

    protected virtual void OnGUI()
    {
        if (!skin) skin = Resources.Load("skin") as GUISkin;
        GUI.skin = skin;

        this.minSize = rect;
        this.titleContent = new GUIContent("Item Collection", null, "Create Quest Item Collection From Item Collection");

        GUILayout.BeginVertical("Quest Item Collection Creator", "window");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");


        if (!itemCollection)
        {
            EditorGUILayout.HelpBox("Item Collection is mandatory", MessageType.Error);
        }

        if (itemCollection && !quest)
            EditorGUILayout.HelpBox("Please set a quest to generate the Quest Target Script Associated", MessageType.Warning);

        if (!questManager)
            EditorGUILayout.HelpBox("Unable to hook up to the right events of Quest Target Script without the quest Manager", MessageType.Error);

        if (!itemCollection)
            itemCollection = EditorGUILayout.ObjectField("Item Collection : ", itemCollection, typeof(GameObject), true) as GameObject;
        else
        {

            itemCollection = EditorGUILayout.ObjectField("Item Collection : ", itemCollection, typeof(GameObject), true) as GameObject;

            if (!target)
            {

                target = EditorGUILayout.ObjectField("Quest Target : ", target, typeof(vQuestTarget), true) as vQuestTarget;


            }
            else
            {

                if (!target.quest)
                {

                    target = EditorGUILayout.ObjectField("Quest Target : ", target, typeof(vQuestTarget), true) as vQuestTarget;

                }

            }

        }


        if (itemCollection != null && itemCollection.GetComponent<vQuestTarget>() != null)
        {
            target = itemCollection.GetComponent<vQuestTarget>();
        }

        questManager = EditorGUILayout.ObjectField("Quest Manager : ", questManager, typeof(vQuestManager), true, GUILayout.ExpandWidth(true)) as vQuestManager;

        if (questManager != null)
        {
            itemManager = questManager.gameObject.GetComponent<vItemManager>();
        }

        quest = EditorGUILayout.ObjectField("Quest for Target : ", quest, typeof(vQuest), false) as vQuest;



        GUILayout.EndVertical();

        if (CanCreate())
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
                Create();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    protected virtual bool CanCreate()
    {
        return itemCollection != null;
    }

    protected virtual void Create()
    {
        // base for the character

        var _target = Instantiate(itemCollection, itemCollection.transform.position, itemCollection.transform.rotation, itemCollection.transform.parent);

        _target.name = itemCollection.name;

        var _questCollection = _target.AddComponent<vQuestItemCollection>();

        var collection = itemCollection.GetComponent<vItemCollection>();

        _questCollection.autoAction = collection.autoAction;
        _questCollection.disableCollision = collection.disableCollision;
        _questCollection.disableGravity = collection.disableGravity;
        _questCollection.resetPlayerSettings = collection.resetPlayerSettings;
        _questCollection.playAnimation = collection.playAnimation;
        _questCollection.endExitTimeAnimation = collection.endExitTimeAnimation;
        var avatarTarget = collection.avatarTarget;
        _questCollection.avatarTarget = avatarTarget;
        var matchTarget = collection.matchTarget;
        _questCollection.matchTarget = matchTarget;
        _questCollection.startMatchTarget = collection.startMatchTarget;
        _questCollection.endMatchTarget = collection.endMatchTarget;
        _questCollection.activeFromForward = collection.activeFromForward;
        _questCollection.useTriggerRotation = collection.useTriggerRotation;
        _questCollection.destroyAfter = collection.destroyAfter;
        _questCollection.destroyDelay = collection.destroyDelay;
        _questCollection.onDoActionDelay = collection.onDoActionDelay;
        var OnDoAction = collection.OnDoAction;
        _questCollection.OnDoAction = OnDoAction;
        var OnPlayerEnter = collection.OnPlayerEnter;
        _questCollection.OnPlayerEnter = OnPlayerEnter;
        var OnPlayerExit = collection.OnPlayerExit;
        _questCollection.OnPlayerExit = OnPlayerExit;
        _questCollection.onCollectDelay = collection.onCollectDelay;
        _questCollection.immediate = collection.immediate;
        _questCollection.itemListData = collection.itemListData;
        _questCollection.itemsFilter = collection.itemsFilter;
        _questCollection.items = collection.items;

        DestroyImmediate(_target.GetComponent<vItemCollection>());


        _target.transform.position = itemCollection.transform.position;
        _target.transform.rotation = itemCollection.transform.rotation;
        _target.transform.parent = itemCollection.transform.parent;

        if (!_target)
            return;

        var layer = LayerMask.NameToLayer("Default");

        if (target == null)
        {
            _target.layer = layer;
            _target.tag = "Untagged";
            target = _target.AddComponent<vQuestTarget>();
        }

        if (target.quest == null && quest != null)
        {

            target.quest = quest;

            if (questManager != null)
            {

                target.onProviderVendorTargetActionEvent = new OnProviderVendorTargetActionEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener((UnityEvent<vQuest, vQuestProvider, IQuestTarget>)target.onProviderVendorTargetActionEvent, questManager.UpdateQuestFromTarget);

            }

            UnityEditor.Events.UnityEventTools.AddPersistentListener(_questCollection.OnDoAction, _questCollection.CollectItem);
        }

        _target.name = _target.name.Replace("(Clone)", string.Empty);
        this.Close();

    }

}

