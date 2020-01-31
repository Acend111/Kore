using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using Invector;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vQuest))]
    public class vQuestEditor : Editor
    {
        public vQuest quest;
        public bool inAddAttribute;
        public vQuestAttributes attribute;
        public int attributeValue;
        public int index;

        public bool inEditName;
        public string currentName;
        public GUISkin skin;
        public string[] drawPropertiesExcluding = new string[] {
            "id",
            "type",
            "description",
            "objective",
            "icon",
            "provider",
            "attributes",
            "secondaryObjectives",
            "secondaryQuestsFilter",
            "secondaryObjectivesList",
            "gatherItem",
            "rewardItems",
            "reloadLocationOnDecline"
        };

        #region SerializedProperties in Custom Editor
        [SerializeField]
        public List<QuestReference> secondary = new List<QuestReference>();
        #endregion
        protected SerializedProperty questReferenceList;

        List<vQuest> filteredQuests;

        bool inAddQuest;
        int selectedQuest;
        Vector2 scroll;

        protected virtual void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;

        }

        public override void OnInspectorGUI()
        {
            if (skin) GUI.skin = skin;
            DrawQuest();
        }

        public virtual void DrawQuest()
        {
            if (quest == null) quest = target as vQuest;

            serializedObject.Update();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            var name = " ID " + quest.id.ToString("00") + "\n - " + quest.name + "\n - " + quest.type.ToString();
            var content = new GUIContent(name);
            GUILayout.Label(content, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent(""));
            //item.icon = (Sprite)EditorGUILayout.ObjectField(item.icon, typeof(Sprite), false);
            var rect = GUILayoutUtility.GetRect(40, 40);

            if (quest.icon != null)
            {
                DrawTextureGUI(rect, quest.icon, new Vector2(40, 40));
            }

            GUILayout.EndHorizontal();

            DrawAttributes();

            GUILayout.BeginVertical("box");
            GUILayout.Box(new GUIContent("Custom Settings", "This area is used for additional properties\n in vQuest Properties in defaultInspector region"));

            if (quest.type != vQuestType.Multiple)
            {
                string[] doNotDrawSecondary = { "secondaryQuestListData", "secondaryQuestReferenceList" };
                drawPropertiesExcluding.Append(doNotDrawSecondary);
            }
            DrawPropertiesExcluding(serializedObject, drawPropertiesExcluding);

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(quest);
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawAttributes()
        {
            try
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box("Attributes", GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (!inAddAttribute && GUILayout.Button("Add Attribute", EditorStyles.miniButton))
                    inAddAttribute = true;
                if (inAddAttribute)
                {
                    GUILayout.BeginHorizontal("box");
                    attribute = (vQuestAttributes)EditorGUILayout.EnumPopup(attribute);
                    EditorGUILayout.LabelField("Value", GUILayout.MinWidth(60));

                    if (!quest.attributes.GetAttributeByType(attribute).isBool)
                        attributeValue = EditorGUILayout.IntField(attributeValue);
                    else
                    {
                        bool value = EditorGUILayout.Toggle(attributeValue > 0 ? true : false);
                        attributeValue = value ? 1 : 0;
                    }

                    GUILayout.EndHorizontal();
                    if (quest.attributes != null && quest.attributes.Contains(attribute))
                    {
                        EditorGUILayout.HelpBox("This attribute already exist ", MessageType.Error);
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            inAddAttribute = false;
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal("box");
                        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            quest.attributes.Add(new vQuestAttribute(attribute, attributeValue));

                            attributeValue = 0;
                            inAddAttribute = false;

                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            attributeValue = 0;
                            inAddAttribute = false;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space();
                var attributesProp = serializedObject.FindProperty("attributes");
                for (int i = 0; i < quest.attributes.Count; i++)
                {
                    GUILayout.BeginHorizontal("box");
                    var attributeValue = attributesProp.GetArrayElementAtIndex(i).FindPropertyRelative("value");
                    var isBool = attributesProp.GetArrayElementAtIndex(i).FindPropertyRelative("isBool");

                    attributeValue.serializedObject.Update();
                    EditorGUILayout.LabelField(quest.attributes[i].name.ToString(), GUILayout.MinWidth(60));
                    //EditorGUILayout.PropertyField(attributeValue, new GUIContent(""));
                    //item.attributes[i].value = EditorGUILayout.IntField(item.attributes[i].value);

                    if (isBool.boolValue)
                        attributeValue.intValue = EditorGUILayout.Toggle(attributeValue.intValue > 0 ? true : false, GUILayout.MaxWidth(60)) ? 1 : 0;
                    else
                        attributeValue.intValue = EditorGUILayout.IntField(attributeValue.intValue);

                    EditorGUILayout.Space();
                    if (GUILayout.Button("x", GUILayout.MaxWidth(30)))
                    {
                        quest.attributes.RemoveAt(i);
                        GUILayout.EndHorizontal();

                        break;
                    }
                    attributeValue.serializedObject.ApplyModifiedProperties();
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }
            catch { }
        }

        protected virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }

        protected virtual List<vQuest> GetQuestByFilter(List<vQuest> quests, List<vQuestType> filter)
        {
            return quests.FindAll(i => filter.Contains(i.type));
        }


        protected virtual GUIContent[] GetQuestContents(List<vQuest> quests)
        {
            GUIContent[] names = new GUIContent[quests.Count];
            for (int i = 0; i < quests.Count; i++)
            {
                var texture = quests[i].icon != null ? quests[i].icon.texture : null;
                names[i] = new GUIContent(quests[i].name, texture, quests[i].description);
            }
            return names;
        }

    }

    public class vQuestDrawer
    {
        public vQuest quest;
        bool inAddAttribute;
        vQuestAttributes attribute;
        int attributeValue;
        int index;

        bool inEditName;
        string currentName;
        // public string[] drawPropertiesExcluding = new string[] { "id", "description", "type", "icon", "stackable", "maxStack", "amount", "originalObject", "dropObject", "attributes", "isInEquipArea" };
        Editor defaultEditor;
        vQuestTargetType targetType;

        bool inAddQuest;
        bool inAddDependent;
        bool inAddItem;

        int selectedQuest;
        int selectedItem;

        protected SerializedProperty questReferenceList;
        protected SerializedProperty dependentQuestReferenceList;
        protected SerializedProperty rewardItemReferenceList;

        GameObject TargetObject;

        Vector2 scrollRewardList;
        Vector2 scrollSecondaryList;
        Vector2 scrollDepedentList;


        public vQuestDrawer(vQuest quest)
        {
            this.quest = quest;
            defaultEditor = Editor.CreateEditor(this.quest);
            questReferenceList = defaultEditor.serializedObject.FindProperty("secondaryQuestReferenceList");
            dependentQuestReferenceList = defaultEditor.serializedObject.FindProperty("dependencyQuestReferenceList");
            rewardItemReferenceList = defaultEditor.serializedObject.FindProperty("rewardItemReferenceList");
        }

        public virtual void DrawSecondaryQuestCollection(ref vQuestListData collection, bool showObject = true, bool editName = false)
        {
            if (!collection)
                return;
            SerializedObject _collection = new SerializedObject(collection);

            DrawSecondaryQuestList(collection, defaultEditor.serializedObject, questReferenceList, quest.secondaryQuestReferenceList,
                quest.secondaryQuestTypeFilter, quest.filteredQuests);

            _collection.Update();
        }

        public virtual void DrawDependentQuestCollection(ref vQuestListData collection, bool showObject = true, bool editName = false)
        {
            if (!collection)
                return;
            SerializedObject _collection = new SerializedObject(collection);

            DrawDependencyQuestList(collection, defaultEditor.serializedObject, dependentQuestReferenceList, quest.dependencyQuestReferenceList,
                quest.dependencyQuestFilter, quest.filteredDependencyQuests);

            _collection.Update();
        }

        public virtual void DrawRewardItemCollection(ref vItemListData collection, bool showObject = true, bool editName = false)
        {
            if (!collection)
                return;
            SerializedObject _collection = new SerializedObject(collection);

            DrawRewards(collection, defaultEditor.serializedObject, rewardItemReferenceList, quest.rewardItemReferenceList,
                quest.rewardFilter, quest.filteredRewardItems);

            _collection.Update();
        }


        public virtual void DrawQuest(ref List<vQuest> quests, bool showObject = true, bool editName = false, bool secondary = false)
        {
            if (!quest) return;
            SerializedObject _quest = new SerializedObject(quest);
            _quest.Update();

            GUILayout.BeginVertical("box");

            if (showObject)
                EditorGUILayout.ObjectField(quest, typeof(vQuest), false);

            if (editName)
                quest.name = EditorGUILayout.TextField("Quest name", quest.name);
            else
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(quest.name, GUILayout.ExpandWidth(true));
                if (!inEditName && GUILayout.Button("EditName", EditorStyles.miniButton))
                {
                    currentName = quest.name;
                    inEditName = true;
                }
                GUILayout.EndHorizontal();
            }
            if (inEditName)
            {
                var sameQuestName = quests.Find(i => i.name == currentName && i != quest);
                currentName = EditorGUILayout.TextField("New Name", currentName);

                GUILayout.BeginHorizontal("box");
                if (sameQuestName == null && !string.IsNullOrEmpty(currentName) && GUILayout.Button("OK", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                {
                    quest.name = currentName;
                    inEditName = false;

                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                {
                    inEditName = false;
                }
                GUILayout.EndHorizontal();
                if (sameQuestName != null)
                    EditorGUILayout.HelpBox("This name already exists", MessageType.Error);
                if (string.IsNullOrEmpty(currentName))
                    EditorGUILayout.HelpBox("This name can not be empty", MessageType.Error);
            }

            quest.type = (vQuestType)EditorGUILayout.EnumPopup("Quest Type", quest.type);

            GUILayout.BeginHorizontal();
            quest.state = (vQuestState)EditorGUILayout.EnumPopup("Quest State", quest.state);
            if (GUILayout.Button("Update Quest Status", EditorStyles.miniButton))
            {
                quest.UpdateQuestStatus(quest.state);
            }
            GUILayout.EndHorizontal();

            if (quest.parent != null)
                quest.isInitiallyActive = false;

            if ((quests.FindAll(q => q.isInitiallyActive).Count == 0 ||
                        quests.Find(q => q.isInitiallyActive && q.Equals(quest)) != null) && quest.parent == null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Set As Active Quest Initially", GUILayout.ExpandWidth(false));
                quest.isInitiallyActive = EditorGUILayout.Toggle(quest.isInitiallyActive);
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Quest Provider");
            quest.provider = (vQuestProvider)EditorGUILayout.ObjectField(quest.provider, typeof(vQuestProvider), true);

            EditorGUILayout.LabelField("Objective");
            quest.objective = EditorGUILayout.TextArea(quest.objective);

            EditorGUILayout.LabelField("Description");
            quest.description = EditorGUILayout.TextArea(quest.description);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon");
            quest.icon = (Sprite)EditorGUILayout.ObjectField(quest.icon, typeof(Sprite), false);
            var rect = GUILayoutUtility.GetRect(40, 40);

            if (quest.icon != null)
            {
                DrawTextureGUI(rect, quest.icon, new Vector2(40, 40));
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            DrawAttributes();
            GUILayout.BeginVertical("box");
            GUILayout.Box(new GUIContent("Custom Settings", "This area is used for additional properties\n in vQuest Properties in defaultInspector region"));

            defaultEditor.DrawDefaultInspector();


            if (quest.attributes != null &&
                   quest.attributes.GetAttributeByType(vQuestAttributes.ReloadAtSpecifiedLocationOnDecline) != null &&
                   quest.attributes.GetAttributeByType(vQuestAttributes.ReloadAtSpecifiedLocationOnDecline).value == 1)
            {
                GUILayout.Label("SpawnPoint to Reload If Quest is  Declined");
                quest.reloadLocationOnDecline = EditorGUILayout.ObjectField((Transform)quest.reloadLocationOnDecline, typeof(Transform), true) as Transform;
            }

            if (quest.type != vQuestType.Gather)
            {
                if (quest.attributes != null &&
                    quest.attributes.GetAttributeByType(vQuestAttributes.AutoComplete) != null &&
                    quest.attributes.GetAttributeByType(vQuestAttributes.AutoComplete).value == 0)
                {
                    GUILayout.Label("End Quest at this target");

                    quest.targetType = (vQuestTargetType)EditorGUILayout.EnumPopup(quest.targetType);
                    var convertTo = Activator.CreateInstance(typeof(vQuest).Assembly.FullName, "EviLA.AddOns.RPGPack." + quest.targetType.ToString());
                    quest.Target = (GameObject)EditorGUILayout.ObjectField((GameObject)quest.Target, typeof(GameObject), true);

                    if (quest.Target == null || (quest.Target != null && quest.Target.GetComponent(convertTo.Unwrap().GetType()) == null))
                    {
                        EditorGUILayout.HelpBox("Select an object that contains a " + quest.targetType + " component. Make sure the class is in the EviLA.AddOns.RPGPack namespace", MessageType.Error);
                        quest.Target = null;
                        quest.QuestTarget = null;
                    }
                    else
                    {
                        quest.QuestTarget = (IQuestTarget)quest.Target.GetComponent(convertTo.Unwrap().GetType());
                    }
                }
                else
                {
                    quest.Target = null;
                    quest.QuestTarget = null;
                }
            }

            //TODO: Adding gather item type
            if (quest.type == vQuestType.Gather)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box(new GUIContent("Gather Quest Settings", "This area is used for additional properties of gather quests\n"));

                GUILayout.Label("Item List for Gather Item");
                quest.gatherItemListData = (vItemListData)EditorGUILayout.ObjectField(quest.gatherItemListData, typeof(vItemListData), false);
                if (quest.gatherItemListData)
                {
                    if (quest.gatherItem != null)
                        selectedItem = quest.gatherItemListData.items.IndexOf(quest.gatherItem);
                    selectedItem = EditorGUILayout.Popup(new GUIContent("Select Gather Item"), selectedItem, GetItemContents(quest.gatherItemListData.items));
                    if (selectedItem != -1)
                        quest.gatherItem = quest.gatherItemListData.items[selectedItem];
                }
                else
                {
                    quest.gatherItem = null;
                }

                GUILayout.Label("Deliver to this target");

                quest.targetType = (vQuestTargetType)EditorGUILayout.EnumPopup(quest.targetType);
                var convertTo = Activator.CreateInstance(typeof(vQuest).Assembly.FullName, "EviLA.AddOns.RPGPack." + quest.targetType.ToString());
                quest.Target = (GameObject)EditorGUILayout.ObjectField((GameObject)quest.Target, typeof(GameObject), true);

                if (quest.Target == null || (quest.Target != null && quest.Target.GetComponent(convertTo.Unwrap().GetType()) == null))
                {
                    EditorGUILayout.HelpBox("Select an object that contains a " + quest.targetType + " component. Make sure the class is in the EviLA.AddOns.RPGPack namespace", MessageType.Error);
                    quest.Target = null;
                }
                else
                {
                    quest.QuestTarget = (IQuestTarget)quest.Target.GetComponent(convertTo.Unwrap().GetType());
                }


                GUILayout.EndVertical();
            }

            //Tag for Escort
            if (quest.type == vQuestType.Escort)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box(new GUIContent("Escort Quest Settings", "This area is used for additional properties of Escort quests\n"));

                GUILayout.Label("Escort Object Tag");
                quest.tagObjectTag = EditorGUILayout.TextField(quest.tagObjectTag);

                GUILayout.EndVertical();
            }

            //TODO: Adding assassinate with Item type
            if (quest.type == vQuestType.Assassinate)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box(new GUIContent("Assassinate Quest Settings", "This area is used for additional properties of Assassinate quests\n"));

                GUILayout.Label("Assassinate Object Tag");
                quest.tagObjectTag = EditorGUILayout.TextField(quest.tagObjectTag);

                GUILayout.Label("Item List for Assassinate Weapon");
                quest.killWithItemListData = (vItemListData)EditorGUILayout.ObjectField(quest.killWithItemListData, typeof(vItemListData), false);

                if (quest.killWithItemListData)
                {
                    if (quest.killWithThis != null)
                        selectedItem = quest.killWithItemListData.items.IndexOf(quest.killWithThis);
                    selectedItem = EditorGUILayout.Popup(new GUIContent("Select Assassinate Item"), selectedItem, GetItemContents(quest.killWithItemListData.items));
                    quest.killWithThis = quest.killWithItemListData.items[selectedItem];
                }
                else
                {
                    quest.killWithThis = null;
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            if (quest.dependencyQuestListData != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box(new GUIContent("Dependent Quest List", "Add quests that need to be completed for this one to appear on provider\n"));

                //defaultEditor.DrawDefaultInspector();
                DrawDependentQuestCollection(ref quest.dependencyQuestListData);
                GUILayout.EndVertical();
            }

            if (quest.type == vQuestType.Multiple && !secondary && quest.secondaryQuestListData != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box(new GUIContent("Secondary Quest List", "This area is used for secondary quests\n"));

                //defaultEditor.DrawDefaultInspector();
                DrawSecondaryQuestCollection(ref quest.secondaryQuestListData);
                GUILayout.EndVertical();
            }

            if (quest.rewardItemListData != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Box(new GUIContent("Reward List", "This area is used for reward items\n"));

                //defaultEditor.DrawDefaultInspector();
                DrawRewardItemCollection(ref quest.rewardItemListData);
                GUILayout.EndVertical();
            }

            //Always set quest accepted status in editor to false
            if (quest.state == vQuestState.InProgress)
                quest.isAccepted = true;
            else
                quest.isAccepted = false;

            if (GUI.changed || _quest.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(quest);
            }
        }

        public virtual void DrawSecondaryQuestList(vQuestListData questListData, SerializedObject serializedObject, SerializedProperty questReferenceList, List<QuestReference> secondaryQuests,
            List<vQuestType> secondaryQuestFilter, List<vQuest> filteredQuests)
        {

            if (questListData)
            {
                string secondaryGUITitle = secondaryQuests.Count + " Secondary Quests";

                GUILayout.BeginVertical("box");
                if (questReferenceList.arraySize > questListData.quests.Count)
                {
                    secondaryQuests.Resize(questListData.quests.Count);
                }

                GUILayout.Box(secondaryGUITitle);

                filteredQuests = secondaryQuestFilter.Count > 0 ? GetQuestByFilter(questListData.quests, secondaryQuestFilter) : questListData.quests;

                if (!inAddQuest && filteredQuests.Count > 0 && GUILayout.Button("Add Quest", EditorStyles.miniButton))
                {
                    inAddQuest = true;
                }
                if (inAddQuest && filteredQuests.Count > 0)
                {
                    GUILayout.BeginVertical("box");

                    string secondaryCombo = secondaryQuests.Count + "SelectSecondaryQuest";

                    selectedQuest = EditorGUILayout.Popup(new GUIContent(secondaryCombo), selectedQuest, GetQuestContents(filteredQuests));

                    if (filteredQuests.Count < selectedQuest)
                        selectedQuest = 0;

                    bool isValid = true;
                    var indexSelected = questListData.quests.IndexOf(filteredQuests[selectedQuest]);
                    if (secondaryQuests.Find(i => i.id == questListData.quests[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This quest already exists", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        questReferenceList.arraySize++;

                        questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = questListData.quests[indexSelected].id;
                        //questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("amount").intValue = 1;
                        EditorUtility.SetDirty(quest);
                        serializedObject.ApplyModifiedProperties();
                        inAddQuest = false;
                    }
                    if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                    {
                        inAddQuest = false;
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.EndVertical();
                }

                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                scrollSecondaryList = GUILayout.BeginScrollView(scrollSecondaryList, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

                for (int i = 0; i < secondaryQuests.Count; i++)
                {
                    var quest = questListData.quests.Find(t => t.id.Equals(secondaryQuests[i].id));
                    if (quest)
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();

                        GUILayout.BeginHorizontal("box");
                        var rect = GUILayoutUtility.GetRect(20, 20);

                        if (quest.icon != null)
                        {
                            DrawTextureGUI(rect, quest.icon, new Vector2(30, 30));
                        }

                        var name = " ID " + quest.id.ToString("00") + "\n - " + quest.name + "\n - " + quest.type.ToString();
                        var content = new GUIContent(name, null, "Click to Open");
                        GUILayout.Label(content, EditorStyles.miniLabel);
                        GUILayout.BeginVertical("box", GUILayout.Height(20), GUILayout.Width(100));

                        if (quest.attributes.Count > 0)
                            secondaryQuests[i].changeAttributes = GUILayout.Toggle(secondaryQuests[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(50)))
                        {
                            //Remove parent
                            var q = questReferenceList.GetArrayElementAtIndex(i).serializedObject.targetObject as vQuest;
                            q.parent = null;

                            questReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(quest);
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }

                        GUILayout.EndHorizontal();
                        Color backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.clear;
                        var _rec = GUILayoutUtility.GetLastRect();
                        _rec.width -= 100;

                        EditorGUIUtility.AddCursorRect(_rec, MouseCursor.Link);

                        if (GUI.Button(_rec, ""))
                        {
                            if (questListData.inEdition)
                            {
                                if (vQuestListWindow.Instance != null)
                                    vQuestListWindow.SetCurrentSelectedQuest(questListData.quests.IndexOf(quest));
                                else
                                    vQuestListWindow.CreateWindow(questListData, questListData.quests.IndexOf(quest));
                            }
                            else
                                vQuestListWindow.CreateWindow(questListData, questListData.quests.IndexOf(quest));
                        }

                        GUI.backgroundColor = backgroundColor;
                        if (quest.attributes != null && quest.attributes.Count > 0)
                        {

                            if (secondaryQuests[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    secondaryQuests[i].attributes = null;

                                }
                                if (secondaryQuests[i].attributes == null)
                                {
                                    secondaryQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else if (secondaryQuests[i].attributes.Count != quest.attributes.Count)
                                {
                                    secondaryQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < secondaryQuests[i].attributes.Count; a++)
                                    {

                                        //Disabled attributes based on quest Type 
                                        switch (secondaryQuests[i].attributes[a].name)
                                        {
                                            case vQuestAttributes.Duration:
                                                break;
                                            case vQuestAttributes.QuestAmount:
                                                if (quest.type != vQuestType.Gather)
                                                    if (quest.type != vQuestType.Assassinate)
                                                        if (quest.type != vQuestType.Escort)
                                                            continue;
                                                break;
                                            case vQuestAttributes.AutoComplete:
                                                break;
                                            case vQuestAttributes.Parallel:
                                                break;
                                            case vQuestAttributes.HasImpactOnParent:
                                                break;
                                            case vQuestAttributes.HasImpactOnChildren:
                                                break;
                                            /*case vQuestAttributes.TagCount:
                                           		if(quest.type != vQuestType.Assassinate)
                                           		 if(quest.type != vQuestType.Escort)
                                           		 	continue; 
                                           		break;*/
                                            default:
                                                break;
                                        }

                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(secondaryQuests[i].attributes[a].name.ToString());

                                        //Explicitly set Parallel attribute to type bool
                                        if (secondaryQuests[i].attributes[a].name == vQuestAttributes.Parallel ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.AutoComplete ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.HasImpactOnParent ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.HasImpactOnChildren ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.TargetCannotLeaveArea ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.ReloadAtSpecifiedLocationOnDecline ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.CheckpointOnStateChange ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.QuestCanBeDeclined ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.ScriptedCountDownEnabled ||
                                            secondaryQuests[i].attributes[a].name == vQuestAttributes.ForceStartOnAccept)
                                        {
                                            bool toggle = EditorGUILayout.Toggle(secondaryQuests[i].attributes[a].value > 0 ? true : false, GUILayout.MaxWidth(60));
                                            secondaryQuests[i].attributes[a].value = toggle ? 1 : 0;
                                        }
                                        else
                                            secondaryQuests[i].attributes[a].value = EditorGUILayout.IntField(secondaryQuests[i].attributes[a].value, GUILayout.MaxWidth(60));
                                        GUILayout.EndHorizontal();
                                    }
                                }
                            }
                        }

                        GUILayout.EndVertical();
                    }
                    else
                    {
                        questReferenceList.DeleteArrayElementAtIndex(i);
                        if (quest)
                            EditorUtility.SetDirty(quest);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(quest);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }


        public virtual void DrawDependencyQuestList(vQuestListData questListData, SerializedObject serializedObject, SerializedProperty questReferenceList, List<QuestReference> depdencyQuest,
            List<vQuestType> dependencyQuestFilter, List<vQuest> filteredDepedent)
        {

            if (questListData)
            {
                string dependencyGUITitle = depdencyQuest.Count + " Dependent Quests";

                GUILayout.BeginVertical("box");
                if (questReferenceList.arraySize > questListData.quests.Count)
                {
                    depdencyQuest.Resize(questListData.quests.Count);
                }

                GUILayout.Box(dependencyGUITitle);

                filteredDepedent = dependencyQuestFilter.Count > 0 ? GetQuestByFilter(questListData.quests, dependencyQuestFilter) : questListData.quests;

                if (!inAddDependent && filteredDepedent.Count > 0 && GUILayout.Button("Add Quest", EditorStyles.miniButton))
                {
                    inAddDependent = true;
                }
                if (inAddDependent && filteredDepedent.Count > 0)
                {
                    GUILayout.BeginVertical("box");

                    string dependencyCombo = depdencyQuest.Count + "SelectDependentQuest";

                    selectedQuest = EditorGUILayout.Popup(new GUIContent(dependencyCombo), selectedQuest, GetQuestContents(filteredDepedent));

                    if (filteredDepedent.Count < selectedQuest)
                        selectedQuest = 0;

                    bool isValid = true;
                    var indexSelected = questListData.quests.IndexOf(filteredDepedent[selectedQuest]);
                    if (depdencyQuest.Find(i => i.id == questListData.quests[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This quest already exists", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        questReferenceList.arraySize++;

                        questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = questListData.quests[indexSelected].id;
                        //questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("amount").intValue = 1;
                        EditorUtility.SetDirty(quest);
                        serializedObject.ApplyModifiedProperties();
                        inAddDependent = false;
                    }
                    if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                    {
                        inAddDependent = false;
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.EndVertical();
                }

                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                scrollDepedentList = GUILayout.BeginScrollView(scrollDepedentList, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

                for (int i = 0; i < depdencyQuest.Count; i++)
                {
                    var quest = questListData.quests.Find(t => t.id.Equals(depdencyQuest[i].id));
                    if (quest)
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();

                        GUILayout.BeginHorizontal("box");
                        var rect = GUILayoutUtility.GetRect(20, 20);

                        if (quest.icon != null)
                        {
                            DrawTextureGUI(rect, quest.icon, new Vector2(30, 30));
                        }

                        var name = " ID " + quest.id.ToString("00") + "\n - " + quest.name + "\n - " + quest.type.ToString();
                        var content = new GUIContent(name, null, "Click to Open");
                        GUILayout.Label(content, EditorStyles.miniLabel);
                        GUILayout.BeginVertical("box", GUILayout.Height(20), GUILayout.Width(100));

                        if (quest.attributes.Count > 0)
                            depdencyQuest[i].changeAttributes = GUILayout.Toggle(depdencyQuest[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(50)))
                        {
                            questReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(quest);
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }

                        GUILayout.EndHorizontal();
                        Color backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.clear;
                        var _rec = GUILayoutUtility.GetLastRect();
                        _rec.width -= 100;

                        EditorGUIUtility.AddCursorRect(_rec, MouseCursor.Link);

                        if (GUI.Button(_rec, ""))
                        {
                            if (questListData.inEdition)
                            {
                                if (vQuestListWindow.Instance != null)
                                    vQuestListWindow.SetCurrentSelectedQuest(questListData.quests.IndexOf(quest));
                                else
                                    vQuestListWindow.CreateWindow(questListData, questListData.quests.IndexOf(quest));
                            }
                            else
                                vQuestListWindow.CreateWindow(questListData, questListData.quests.IndexOf(quest));
                        }

                        GUI.backgroundColor = backgroundColor;
                        if (quest.attributes != null && quest.attributes.Count > 0)
                        {

                            if (depdencyQuest[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    depdencyQuest[i].attributes = null;

                                }
                                if (depdencyQuest[i].attributes == null)
                                {
                                    depdencyQuest[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else if (depdencyQuest[i].attributes.Count != quest.attributes.Count)
                                {
                                    depdencyQuest[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < depdencyQuest[i].attributes.Count; a++)
                                    {

                                        //Disabled attributes based on quest Type 
                                        switch (depdencyQuest[i].attributes[a].name)
                                        {
                                            case vQuestAttributes.Duration:
                                                break;
                                            case vQuestAttributes.QuestAmount:
                                                if (quest.type != vQuestType.Gather)
                                                    if (quest.type != vQuestType.Assassinate)
                                                        if (quest.type != vQuestType.Escort)
                                                            continue;
                                                break;
                                            case vQuestAttributes.AutoComplete:
                                                break;
                                            case vQuestAttributes.Parallel:
                                                break;
                                            case vQuestAttributes.HasImpactOnParent:
                                                break;
                                            case vQuestAttributes.HasImpactOnChildren:
                                                break;
                                            /*case vQuestAttributes.TagCount:
                                           		if(quest.type != vQuestType.Assassinate)
                                           		 if(quest.type != vQuestType.Escort)
                                           		 	continue; 
                                           		break;*/
                                            default:
                                                break;
                                        }

                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(depdencyQuest[i].attributes[a].name.ToString());

                                        //Explicitly set Parallel attribute to type bool
                                        if (depdencyQuest[i].attributes[a].name == vQuestAttributes.Parallel ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.AutoComplete ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.HasImpactOnParent ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.HasImpactOnChildren ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.TargetCannotLeaveArea ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.ReloadAtSpecifiedLocationOnDecline ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.CheckpointOnStateChange ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.QuestCanBeDeclined ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.ScriptedCountDownEnabled ||
                                            depdencyQuest[i].attributes[a].name == vQuestAttributes.ForceStartOnAccept)
                                        {
                                            bool toggle = EditorGUILayout.Toggle(depdencyQuest[i].attributes[a].value > 0 ? true : false, GUILayout.MaxWidth(60));
                                            depdencyQuest[i].attributes[a].value = toggle ? 1 : 0;
                                        }
                                        else
                                            depdencyQuest[i].attributes[a].value = EditorGUILayout.IntField(depdencyQuest[i].attributes[a].value, GUILayout.MaxWidth(60));
                                        GUILayout.EndHorizontal();
                                    }
                                }
                            }
                        }

                        GUILayout.EndVertical();
                    }
                    else
                    {
                        questReferenceList.DeleteArrayElementAtIndex(i);
                        if (quest)
                            EditorUtility.SetDirty(quest);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(quest);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        protected virtual void DrawRewards(vItemListData itemListData, SerializedObject serializedObject, SerializedProperty itemReferenceList, List<ItemReference> rewardItems,
            List<vItemType> rewardItemsFilter, List<vItem> filteredRewardItems)
        {
            if (itemListData)
            {
                GUILayout.BeginVertical("box");
                if (itemReferenceList.arraySize > itemListData.items.Count)
                {
                    rewardItems.Resize(itemListData.items.Count);
                }
                GUILayout.Box("Reward List " + rewardItems.Count);

                filteredRewardItems = rewardItemsFilter.Count > 0 ? GetItemsByFilter(itemListData.items, rewardItemsFilter) : itemListData.items;

                if (!inAddItem && filteredRewardItems.Count > 0 && GUILayout.Button("Add Item", EditorStyles.miniButton))
                {
                    inAddItem = true;
                }
                if (inAddItem && filteredRewardItems.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedItem = EditorGUILayout.Popup(new GUIContent("SelectItem"), selectedItem, GetItemContents(filteredRewardItems));
                    bool isValid = true;
                    var indexSelected = itemListData.items.IndexOf(filteredRewardItems[selectedItem]);
                    if (rewardItems.Find(i => i.id == itemListData.items[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This item already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        itemReferenceList.arraySize++;
                        itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = itemListData.items[indexSelected].id;
                        EditorUtility.SetDirty(quest);
                        serializedObject.ApplyModifiedProperties();
                        inAddItem = false;
                    }
                    if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                    {
                        inAddItem = false;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }

                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                scrollRewardList = GUILayout.BeginScrollView(scrollRewardList, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

                #region Reward Item List
                for (int i = 0; i < rewardItems.Count; i++)
                {
                    var item = itemListData.items.Find(t => t.id.Equals(rewardItems[i].id));
                    if (item)
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.BeginHorizontal();
                        GUILayout.BeginHorizontal("box");
                        var rect = GUILayoutUtility.GetRect(20, 20);
                        if (item.icon != null)
                        {
                            DrawTextureGUI(rect, item.icon, new Vector2(30, 30));
                        }

                        var name = " ID " + item.id.ToString("00") + "\n - " + item.name + "\n - " + item.type.ToString();
                        var content = new GUIContent(name, null, "Click to Open");
                        GUILayout.Label(content, EditorStyles.miniLabel);
                        GUILayout.BeginVertical("box", GUILayout.Height(20), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Amount", EditorStyles.miniLabel);
                        rewardItems[i].amount = EditorGUILayout.IntField(rewardItems[i].amount, GUILayout.Width(40));

                        if (rewardItems[i].amount < 1)
                        {
                            rewardItems[i].amount = 1;
                        }

                        GUILayout.EndHorizontal();
                        if (item.attributes.Count > 0)
                            rewardItems[i].changeAttributes = GUILayout.Toggle(rewardItems[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(50)))
                        {
                            itemReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(quest);
                            serializedObject.ApplyModifiedProperties();
                            break;
                        }

                        GUILayout.EndHorizontal();
                        Color backgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.clear;
                        var _rec = GUILayoutUtility.GetLastRect();
                        _rec.width -= 100;

                        EditorGUIUtility.AddCursorRect(_rec, MouseCursor.Link);

                        if (GUI.Button(_rec, ""))
                        {
                            if (itemListData.inEdition)
                            {
                                if (vItemListWindow.Instance != null)
                                    vItemListWindow.SetCurrentSelectedItem(itemListData.items.IndexOf(item));
                                else
                                    vItemListWindow.CreateWindow(itemListData, itemListData.items.IndexOf(item));
                            }
                            else
                                vItemListWindow.CreateWindow(itemListData, itemListData.items.IndexOf(item));
                        }
                        GUI.backgroundColor = backgroundColor;
                        if (item.attributes != null && item.attributes.Count > 0)
                        {

                            if (rewardItems[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    rewardItems[i].attributes = null;

                                }
                                if (rewardItems[i].attributes == null)
                                {
                                    rewardItems[i].attributes = item.attributes.CopyAsNew();
                                }
                                else if (rewardItems[i].attributes.Count != item.attributes.Count)
                                {
                                    rewardItems[i].attributes = item.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < rewardItems[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(rewardItems[i].attributes[a].name.ToString());
                                        rewardItems[i].attributes[a].value = EditorGUILayout.IntField(rewardItems[i].attributes[a].value, GUILayout.MaxWidth(60));
                                        GUILayout.EndHorizontal();
                                    }
                                }
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        itemReferenceList.DeleteArrayElementAtIndex(i);
                        EditorUtility.SetDirty(quest);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }




                }
                #endregion
                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(quest);
                    serializedObject.ApplyModifiedProperties();
                }
            }

        }

        protected virtual void DrawAttributes()
        {
            try
            {

                GUILayout.BeginVertical("box");
                GUILayout.Box("Attributes", GUILayout.ExpandWidth(true));
                EditorGUILayout.Space();
                if (!inAddAttribute && GUILayout.Button("Add Attribute", EditorStyles.miniButton))
                    inAddAttribute = true;
                if (inAddAttribute)
                {
                    GUILayout.BeginHorizontal("box");
                    attribute = (vQuestAttributes)EditorGUILayout.EnumPopup(attribute);
                    EditorGUILayout.LabelField("Value", GUILayout.MinWidth(60));

                    //Explicitly set Parallel attribute to type bool
                    bool isBool = (attribute == vQuestAttributes.Parallel ||
                                    attribute == vQuestAttributes.AutoComplete ||
                                    attribute == vQuestAttributes.HasImpactOnParent ||
                                    attribute == vQuestAttributes.HasImpactOnChildren ||
                                    attribute == vQuestAttributes.TargetCannotLeaveArea ||
                                    attribute == vQuestAttributes.ReloadAtSpecifiedLocationOnDecline ||
                                    attribute == vQuestAttributes.CheckpointOnStateChange ||
                                    attribute == vQuestAttributes.QuestCanBeDeclined ||
                                    attribute == vQuestAttributes.ScriptedCountDownEnabled ||
                                    attribute == vQuestAttributes.ForceStartOnAccept
                                ) ? true : false;

                    if (isBool)
                    {
                        bool value = EditorGUILayout.Toggle(attribute > 0 ? true : false);
                        attributeValue = value ? 1 : 0;
                    }
                    else
                    {
                        attributeValue = EditorGUILayout.IntField(attributeValue);
                    }

                    GUILayout.EndHorizontal();
                    if (quest.attributes != null && quest.attributes.Contains(attribute))
                    {
                        EditorGUILayout.HelpBox("This attribute already exist ", MessageType.Error);
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            inAddAttribute = false;
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal("box");
                        if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            quest.attributes.Add(new vQuestAttribute(attribute, attributeValue));

                            attributeValue = 0;
                            inAddAttribute = false;

                        }
                        if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.MinWidth(60)))
                        {
                            attributeValue = 0;
                            inAddAttribute = false;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space();
                for (int i = 0; i < quest.attributes.Count; i++)
                {
                    GUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(quest.attributes[i].name.ToString(), GUILayout.MinWidth(60));
                    //Explicitly set Parallel attribute to type bool
                    if (quest.attributes[i].name == vQuestAttributes.Parallel ||
                       quest.attributes[i].name == vQuestAttributes.AutoComplete ||
                       quest.attributes[i].name == vQuestAttributes.HasImpactOnParent ||
                       quest.attributes[i].name == vQuestAttributes.HasImpactOnChildren ||
                       quest.attributes[i].name == vQuestAttributes.TargetCannotLeaveArea ||
                       quest.attributes[i].name == vQuestAttributes.ReloadAtSpecifiedLocationOnDecline ||
                       quest.attributes[i].name == vQuestAttributes.CheckpointOnStateChange ||
                       quest.attributes[i].name == vQuestAttributes.QuestCanBeDeclined ||
                        quest.attributes[i].name == vQuestAttributes.ScriptedCountDownEnabled ||
                        quest.attributes[i].name == vQuestAttributes.ForceStartOnAccept

                   )
                    {

                        bool value = EditorGUILayout.Toggle(quest.attributes[i].value > 0 ? true : false);
                        quest.attributes[i].value = value ? 1 : 0;

                    }
                    else
                    {
                        quest.attributes[i].value = EditorGUILayout.IntField(quest.attributes[i].value);
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button("x", GUILayout.MaxWidth(30)))
                    {
                        quest.attributes.RemoveAt(i);
                        GUILayout.EndHorizontal();
                        break;
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }
            catch { }
        }

        protected virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }

        protected virtual List<vItem> GetItemsByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        protected virtual List<vQuest> GetQuestByFilter(List<vQuest> quests, List<vQuestType> filter)
        {
            return quests.FindAll(i => filter.Contains(i.type));
        }


        protected virtual GUIContent[] GetQuestContents(List<vQuest> quests)
        {
            GUIContent[] names = new GUIContent[quests.Count];
            for (int i = 0; i < quests.Count; i++)
            {
                var texture = quests[i].icon != null ? quests[i].icon.texture : null;
                names[i] = new GUIContent(quests[i].name, texture, quests[i].description);
            }
            return names;
        }

        protected virtual GUIContent[] GetItemContents(List<vItem> items)
        {
            GUIContent[] names = new GUIContent[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var texture = items[i].icon != null ? items[i].icon.texture : null;
                names[i] = new GUIContent(items[i].name, texture, items[i].description);
            }
            return names;
        }

        protected virtual GUIContent GetQuestContent(vQuest quest)
        {
            var texture = quest.icon != null ? quest.icon.texture : null;
            return new GUIContent(quest.name, texture, quest.description); ;
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }

}


