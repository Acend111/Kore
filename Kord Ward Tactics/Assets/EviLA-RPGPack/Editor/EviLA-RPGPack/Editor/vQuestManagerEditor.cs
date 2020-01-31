using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Invector;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    [CustomEditor(typeof(vQuestManager))]
    [System.Serializable]
    public class vQuestManagerEditor : vEditorBase
    {
        protected vQuestManager manager;
        protected SerializedProperty questReferenceList;
        GUISkin oldSkin;
        bool inAddQuest;
        int selectedQuest;
        Vector2 scroll;
        bool showManagerEvents;
        string[] ignoreQuestProperties = new string[] { "equipPoints", "applyAttributeEvents", "items", "startItems", "onUseItem", "onAddItem", "onLeaveItem", "onDropItem", "onOpenCloseInventory", "onOpenCloseQuestProviderUI", "onEquipItem", "onUnequipItem",
                                                                                          "quests", "startQuests", "onAddQuest", "m_Script", "onAcceptQuest", "onDeclineQuest", "onCompleteQuestObjective","onBuyItem","onSellItem","onProviderVendorTargetActionEvent"
        };


        bool[] inEdition;
        string[] newPointNames;
        Transform parentBone;
        Animator animator;

        List<vItem> filteredItems;
        List<vQuest> filteredQuests;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
            skin = Resources.Load("skin") as GUISkin;

            manager = (vQuestManager)target;
            questReferenceList = serializedObject.FindProperty("startQuests");
            animator = manager.GetComponent<Animator>();
        }

        public override void OnInspectorGUI()
        {
            oldSkin = GUI.skin;
            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("Quest Manager", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (!openCloseWindow)
                return;

            GUI.skin = oldSkin;
            DrawPropertiesExcluding(serializedObject, ignoreQuestProperties.Append(ignore_vMono));
            GUI.skin = skin;

            /*******************************************************************************************************************************************************/
            /*******************************************************************************************************************************************************/
            #region QuestListUI

            if (GUILayout.Button("Open Quest List"))
            {
                vQuestListWindow.CreateWindow(manager.questListData);
            }

            if (manager.questListData)
            {
                GUILayout.BeginVertical("box");
                if (questReferenceList.arraySize > manager.questListData.quests.Count)
                {
                    manager.startQuests.Resize(manager.questListData.quests.Count);
                }
                GUILayout.Box("Start Quests " + manager.startQuests.Count);
                filteredQuests = manager.questsFilter.Count > 0 ? GetQuestByFilter(manager.questListData.quests, manager.questsFilter) : manager.questListData.quests;

                if (!inAddQuest && filteredQuests.Count > 0 && GUILayout.Button("Add Quest", EditorStyles.miniButton))
                {
                    inAddQuest = true;
                }
                if (inAddQuest && filteredQuests.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedQuest = EditorGUILayout.Popup(new GUIContent("SelectQuest"), selectedQuest, GetQuestContents(filteredQuests));
                    bool isValid = true;
                    var indexSelected = manager.questListData.quests.IndexOf(filteredQuests[selectedQuest]);
                    if (manager.startQuests.Find(i => i.id == manager.questListData.quests[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This quest already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        questReferenceList.arraySize++;

                        questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = manager.questListData.quests[indexSelected].id;
                        EditorUtility.SetDirty(manager);
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
                scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                for (int i = 0; i < manager.startQuests.Count; i++)
                {
                    var quest = manager.questListData.quests.Find(t => t.id.Equals(manager.startQuests[i].id));
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
                        GUILayout.BeginHorizontal();
                        GUILayout.EndHorizontal();
                        if (quest.attributes.Count > 0)
                            manager.startQuests[i].changeAttributes = GUILayout.Toggle(manager.startQuests[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(50)))
                        {
                            questReferenceList.DeleteArrayElementAtIndex(i);
                            EditorUtility.SetDirty(target);
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
                            if (manager.questListData.inEdition)
                            {
                                if (vQuestListWindow.Instance != null)
                                    vQuestListWindow.SetCurrentSelectedQuest(manager.questListData.quests.IndexOf(quest));
                                else
                                    vQuestListWindow.CreateWindow(manager.questListData, manager.questListData.quests.IndexOf(quest));
                            }
                            else
                                vQuestListWindow.CreateWindow(manager.questListData, manager.questListData.quests.IndexOf(quest));
                        }

                        GUI.backgroundColor = backgroundColor;
                        if (quest.attributes != null && quest.attributes.Count > 0)
                        {

                            if (manager.startQuests[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    manager.startQuests[i].attributes = null;

                                }
                                if (manager.startQuests[i].attributes == null)
                                {
                                    manager.startQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else if (manager.startQuests[i].attributes.Count != quest.attributes.Count)
                                {
                                    manager.startQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < manager.startQuests[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(manager.startQuests[i].attributes[a].name.ToString());

                                        //Explicity set Parallel quest attribute to boolean
                                        bool isBool = manager.startQuests[i].attributes[a].name == vQuestAttributes.Parallel;
                                        if (isBool)
                                        {
                                            bool value = EditorGUILayout.Toggle(manager.startQuests[i].attributes[a].value > 0 ? true : false);

                                            manager.startQuests[i].attributes[a].value = value ? 1 : 0;
                                        }
                                        else
                                        {
                                            manager.startQuests[i].attributes[a].value = EditorGUILayout.IntField(manager.startQuests[i].attributes[a].value, GUILayout.MaxWidth(60));
                                        }
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
                        EditorUtility.SetDirty(manager);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(manager);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            #endregion
            /*******************************************************************************************************************************************************/

            //var onAddQuest = serializedObject.FindProperty("onAddQuest");
            var onAcceptQuest = serializedObject.FindProperty("onAcceptQuest");
            var onDeclineQuest = serializedObject.FindProperty("onDeclineQuest");
            //var onOpenCloseQuestProviderUI = serializedObject.FindProperty("onOpenCloseQuestProviderUI");
            //var onOpenCloseInventoty = serializedObject.FindProperty("onOpenCloseInventory");


            GUILayout.BeginVertical("box");
            showManagerEvents = GUILayout.Toggle(showManagerEvents, showManagerEvents ? "Close Events" : "Open Events", EditorStyles.miniButton);
            GUI.skin = oldSkin;
            if (showManagerEvents)
            {
                //if (onAddQuest != null) EditorGUILayout.PropertyField(onAddQuest);
                if (onAcceptQuest != null) EditorGUILayout.PropertyField(onAcceptQuest);
                if (onDeclineQuest != null) EditorGUILayout.PropertyField(onDeclineQuest);
                //if (onOpenCloseInventoty != null) EditorGUILayout.PropertyField(onOpenCloseInventoty);
                //if (onOpenCloseQuestProviderUI != null) EditorGUILayout.PropertyField(onOpenCloseQuestProviderUI);

            }
            GUI.skin = skin;
            GUILayout.EndVertical();
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
                serializedObject.ApplyModifiedProperties();
            }

            GUI.skin = oldSkin;
        }

        protected virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;
            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);

        }

        protected virtual GUIContent GetQuestContents(vQuest quest)
        {
            var texture = quest.icon != null ? quest.icon.texture : null;
            return new GUIContent(quest.name, texture, quest.description); ;
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

        protected virtual T[] ConvertToArray<T>(SerializedProperty prop)
        {
            T[] value = new T[prop.arraySize];
            for (int i = 0; i < prop.arraySize; i++)
            {
                object element = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                value[i] = (T)element;
            }
            return value;
        }
    }
}
