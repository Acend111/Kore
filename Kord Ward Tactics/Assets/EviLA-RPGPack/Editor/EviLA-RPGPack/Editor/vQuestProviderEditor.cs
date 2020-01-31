using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Invector;

namespace EviLA.AddOns.RPGPack
{
    [CustomEditor(typeof(vQuestProvider))]
    public class vQuestProviderEditor : vEditorBase
    {
        vQuestProvider provider;
        SerializedProperty questReferenceList;
        bool inAddQuest;
        int selectedQuest;
        List<vQuest> filteredQuests;
        Vector2 scroll;
        string[] ignoreProperties = { };


        protected override void OnEnable()
        {
            base.OnEnable();
            m_Logo = (Texture2D)Resources.Load("icon_v2", typeof(Texture2D));
            skin = Resources.Load("skin") as GUISkin;

            provider = (vQuestProvider)target;
            questReferenceList = serializedObject.FindProperty("providerQuests");


        }

        public override void OnInspectorGUI()
        {

            var oldSkin = GUI.skin;
            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("Quest Provider", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (!openCloseWindow)
                return;

            GUI.skin = oldSkin;
            DrawPropertiesExcluding(serializedObject, ignoreProperties.Append(ignore_vMono));
            GUI.skin = skin;

            if (provider.questListData)
            {
                GUILayout.BeginVertical("box");
                if (questReferenceList.arraySize > provider.questListData.quests.Count)
                {
                    provider.providerQuests.Resize(provider.questListData.quests.Count);
                }
                GUILayout.Box("Quest List " + provider.providerQuests.Count);
                filteredQuests = provider.questsFilter.Count > 0 ? GetQuestByFilter(provider.questListData.quests, provider.questsFilter) : provider.questListData.quests;

                if (!inAddQuest && filteredQuests.Count > 0 && GUILayout.Button("Add Quest", EditorStyles.miniButton))
                {
                    inAddQuest = true;
                }
                if (inAddQuest && filteredQuests.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedQuest = EditorGUILayout.Popup(new GUIContent("SelectQuest"), selectedQuest, GetQuestContents(filteredQuests));
                    bool isValid = true;
                    var indexSelected = provider.questListData.quests.IndexOf(filteredQuests[selectedQuest]);
                    if (provider.providerQuests.Find(i => i.id == provider.questListData.quests[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This quest already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        questReferenceList.arraySize++;
                        questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = provider.questListData.quests[indexSelected].id;
                        EditorUtility.SetDirty(provider);
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
                for (int i = 0; i < provider.providerQuests.Count; i++)
                {
                    var quest = provider.questListData.quests.Find(t => t.id.Equals(provider.providerQuests[i].id));
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
                            provider.providerQuests[i].changeAttributes = GUILayout.Toggle(provider.providerQuests[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
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
                            if (provider.questListData.inEdition)
                            {
                                if (vQuestListWindow.Instance != null)
                                    vQuestListWindow.SetCurrentSelectedQuest(provider.questListData.quests.IndexOf(quest));
                                else
                                    vQuestListWindow.CreateWindow(provider.questListData, provider.questListData.quests.IndexOf(quest));
                            }
                            else
                                vQuestListWindow.CreateWindow(provider.questListData, provider.questListData.quests.IndexOf(quest));
                        }
                        GUI.backgroundColor = backgroundColor;
                        if (quest.attributes != null && quest.attributes.Count > 0)
                        {

                            if (provider.providerQuests[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    provider.providerQuests[i].attributes = null;

                                }
                                if (provider.providerQuests[i].attributes == null)
                                {
                                    provider.providerQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else if (provider.providerQuests[i].attributes.Count != quest.attributes.Count)
                                {
                                    provider.providerQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < provider.providerQuests[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(provider.providerQuests[i].attributes[a].name.ToString());

                                        //Explicity set Parallel quest attribute to boolean
                                        bool isBool = provider.providerQuests[i].attributes[a].name == vQuestAttributes.Parallel ||
                                                      provider.providerQuests[i].attributes[a].name == vQuestAttributes.AutoComplete;
                                        if (isBool)
                                        {
                                            bool value = EditorGUILayout.Toggle(provider.providerQuests[i].attributes[a].value > 0 ? true : false);

                                            provider.providerQuests[i].attributes[a].value = value ? 1 : 0;
                                        }
                                        else
                                        {
                                            provider.providerQuests[i].attributes[a].value = EditorGUILayout.IntField(provider.providerQuests[i].attributes[a].value, GUILayout.MaxWidth(60));
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
                        EditorUtility.SetDirty(provider);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(provider);
                    serializedObject.ApplyModifiedProperties();
                }
            }
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
            GUI.skin = oldSkin;
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

        protected virtual List<vQuest> GetQuestByFilter(List<vQuest> quests, List<vQuestType> filter)
        {
            return quests.FindAll(i => filter.Contains(i.type));
        }

        protected virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
        }
    }

}

