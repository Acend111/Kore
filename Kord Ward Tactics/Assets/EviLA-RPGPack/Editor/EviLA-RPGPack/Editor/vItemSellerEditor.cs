using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Invector;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    [CustomEditor(typeof(vItemSeller))]
    public class vItemSellerEditor : vEditorBase
    {
        vItemSeller vendor;
        SerializedProperty itemReferenceList;
        SerializedProperty questReferenceList;

        bool inAddItem;
        int selectedItem;
        List<vItem> filteredItems;

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

            vendor = (vItemSeller)target;
            itemReferenceList = serializedObject.FindProperty("vendorItems");
            questReferenceList = serializedObject.FindProperty("vendorQuests");

        }

        public override void OnInspectorGUI()
        {
            var oldSkin = GUI.skin;
            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("Vendor / Item Seller", "window");
            GUILayout.Label(m_Logo, GUILayout.MaxHeight(25));

            openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (!openCloseWindow)
                return;

            GUI.skin = oldSkin;
            DrawPropertiesExcluding(serializedObject, ignoreProperties.Append(ignore_vMono));
            GUI.skin = skin;

            if (vendor.itemListData)
            {
                GUILayout.BeginVertical("box");
                if (itemReferenceList.arraySize > vendor.itemListData.items.Count)
                {
                    vendor.vendorItems.Resize(vendor.itemListData.items.Count);
                }
                GUILayout.Box("Item List " + vendor.vendorItems.Count);
                filteredItems = vendor.itemsFilter.Count > 0 ? GetItemsByFilter(vendor.itemListData.items, vendor.itemsFilter) : vendor.itemListData.items;

                if (!inAddItem && filteredItems.Count > 0 && GUILayout.Button("Add Item", EditorStyles.miniButton))
                {
                    inAddItem = true;
                }
                if (inAddItem && filteredItems.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedItem = EditorGUILayout.Popup(new GUIContent("SelectItem"), selectedItem, GetItemContents(filteredItems));
                    bool isValid = true;
                    var indexSelected = vendor.itemListData.items.IndexOf(filteredItems[selectedItem]);
                    if (vendor.vendorItems.Find(i => i.id == vendor.itemListData.items[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This item already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        itemReferenceList.arraySize++;
                        itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = vendor.itemListData.items[indexSelected].id;
                        EditorUtility.SetDirty(vendor);
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
                scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinHeight(200), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

                #region Vendor Item List
                for (int i = 0; i < vendor.vendorItems.Count; i++)
                {
                    var item = vendor.itemListData.items.Find(t => t.id.Equals(vendor.vendorItems[i].id));
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
                        vendor.vendorItems[i].amount = EditorGUILayout.IntField(vendor.vendorItems[i].amount, GUILayout.Width(40));

                        if (vendor.vendorItems[i].amount < 1)
                        {
                            vendor.vendorItems[i].amount = 1;
                        }

                        GUILayout.EndHorizontal();
                        if (item.attributes.Count > 0)
                            vendor.vendorItems[i].changeAttributes = GUILayout.Toggle(vendor.vendorItems[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(50)))
                        {
                            itemReferenceList.DeleteArrayElementAtIndex(i);
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
                            if (vendor.itemListData.inEdition)
                            {
                                if (vItemListWindow.Instance != null)
                                    vItemListWindow.SetCurrentSelectedItem(vendor.itemListData.items.IndexOf(item));
                                else
                                    vItemListWindow.CreateWindow(vendor.itemListData, vendor.itemListData.items.IndexOf(item));
                            }
                            else
                                vItemListWindow.CreateWindow(vendor.itemListData, vendor.itemListData.items.IndexOf(item));
                        }
                        GUI.backgroundColor = backgroundColor;
                        if (item.attributes != null && item.attributes.Count > 0)
                        {

                            if (vendor.vendorItems[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    vendor.vendorItems[i].attributes = null;

                                }
                                if (vendor.vendorItems[i].attributes == null)
                                {
                                    vendor.vendorItems[i].attributes = item.attributes.CopyAsNew();
                                }
                                else if (vendor.vendorItems[i].attributes.Count != item.attributes.Count)
                                {
                                    vendor.vendorItems[i].attributes = item.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < vendor.vendorItems[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(vendor.vendorItems[i].attributes[a].name.ToString());
                                        vendor.vendorItems[i].attributes[a].value = EditorGUILayout.IntField(vendor.vendorItems[i].attributes[a].value, GUILayout.MaxWidth(60));
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
                        EditorUtility.SetDirty(vendor);
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
                    EditorUtility.SetDirty(vendor);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (vendor.questListData)
            {
                GUILayout.BeginVertical("box");
                if (questReferenceList.arraySize > vendor.questListData.quests.Count)
                {
                    vendor.vendorQuests.Resize(vendor.questListData.quests.Count);
                }
                GUILayout.Box("Quest List " + vendor.vendorQuests.Count);
                filteredQuests = vendor.questsFilter.Count > 0 ? GetQuestByFilter(vendor.questListData.quests, vendor.questsFilter) : vendor.questListData.quests;

                if (!inAddQuest && filteredQuests.Count > 0 && GUILayout.Button("Add Quest", EditorStyles.miniButton))
                {
                    inAddQuest = true;
                }
                if (inAddQuest && filteredQuests.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedQuest = EditorGUILayout.Popup(new GUIContent("SelectQuest"), selectedQuest, GetQuestContents(filteredQuests));
                    bool isValid = true;
                    var indexSelected = vendor.questListData.quests.IndexOf(filteredQuests[selectedQuest]);
                    if (vendor.vendorQuests.Find(i => i.id == vendor.questListData.quests[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This quest already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        questReferenceList.arraySize++;
                        questReferenceList.GetArrayElementAtIndex(questReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = vendor.questListData.quests[indexSelected].id;
                        EditorUtility.SetDirty(vendor);
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
                for (int i = 0; i < vendor.vendorQuests.Count; i++)
                {
                    var quest = vendor.questListData.quests.Find(t => t.id.Equals(vendor.vendorQuests[i].id));
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
                            vendor.vendorQuests[i].changeAttributes = GUILayout.Toggle(vendor.vendorQuests[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original quest attributes"), EditorStyles.miniButton, GUILayout.Width(100));
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
                            if (vendor.questListData.inEdition)
                            {
                                if (vQuestListWindow.Instance != null)
                                    vQuestListWindow.SetCurrentSelectedQuest(vendor.questListData.quests.IndexOf(quest));
                                else
                                    vQuestListWindow.CreateWindow(vendor.questListData, vendor.questListData.quests.IndexOf(quest));
                            }
                            else
                                vQuestListWindow.CreateWindow(vendor.questListData, vendor.questListData.quests.IndexOf(quest));
                        }
                        GUI.backgroundColor = backgroundColor;
                        if (quest.attributes != null && quest.attributes.Count > 0)
                        {

                            if (vendor.vendorQuests[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    vendor.vendorQuests[i].attributes = null;

                                }
                                if (vendor.vendorQuests[i].attributes == null)
                                {
                                    vendor.vendorQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else if (vendor.vendorQuests[i].attributes.Count != quest.attributes.Count)
                                {
                                    vendor.vendorQuests[i].attributes = quest.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < vendor.vendorQuests[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(vendor.vendorQuests[i].attributes[a].name.ToString());

                                        //Explicity set Parallel quest attribute to boolean
                                        bool isBool = vendor.vendorQuests[i].attributes[a].name == vQuestAttributes.Parallel ||
                                                      vendor.vendorQuests[i].attributes[a].name == vQuestAttributes.AutoComplete;
                                        if (isBool)
                                        {
                                            bool value = EditorGUILayout.Toggle(vendor.vendorQuests[i].attributes[a].value > 0 ? true : false);

                                            vendor.vendorQuests[i].attributes[a].value = value ? 1 : 0;
                                        }
                                        else
                                        {
                                            vendor.vendorQuests[i].attributes[a].value = EditorGUILayout.IntField(vendor.vendorQuests[i].attributes[a].value, GUILayout.MaxWidth(60));
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
                        EditorUtility.SetDirty(vendor);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }

                GUILayout.EndScrollView();
                GUI.skin.box = boxStyle;

                GUILayout.EndVertical();
                if (GUI.changed)
                {
                    EditorUtility.SetDirty(vendor);
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

        protected virtual List<vItem> GetItemsByFilter(List<vItem> items, List<vItemType> filter)
        {
            return items.FindAll(i => filter.Contains(i.type));
        }

        protected virtual void DrawTextureGUI(Rect position, Sprite sprite, Vector2 size)
        {
            Rect spriteRect = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height,
                                       sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
            Vector2 actualSize = size;

            actualSize.y *= (sprite.rect.height / sprite.rect.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + (size.y - actualSize.y) / 2, actualSize.x, actualSize.y), sprite.texture, spriteRect);
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
    }

}

