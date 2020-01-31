using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using Invector.vItemManager;

namespace Invector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(vLootTable))]
    public class vLootTableEditor : Editor
    {
        vLootTable lootTable;
        SerializedProperty itemReferenceList;
        GUISkin skin;

        bool inAddItem;
        int selectedItem;
        List<vItem> filteredItems;

        Vector2 scroll;

        protected virtual void OnEnable()
        {
            lootTable = (vLootTable)target;
            skin = Resources.Load("skin") as GUISkin;
            itemReferenceList = serializedObject.FindProperty("lootableItems");
        }

        public override void OnInspectorGUI()
        {
            var oldSkin = GUI.skin;

            serializedObject.Update();
            if (skin) GUI.skin = skin;
            GUILayout.BeginVertical("Loot Table", "window");
            GUILayout.Space(30);
            GUI.skin = oldSkin;
            base.OnInspectorGUI();
            if (skin) GUI.skin = skin;

            if (lootTable.lootableItemListData)
            {
                GUILayout.BeginVertical("box");

                var lootTableListData = lootTable.lootableItemListData;
                var lootableItems = lootTable.lootableItems;
                var lootableFilter = lootTable.lootableFilter;

                if (itemReferenceList.arraySize > lootTableListData.items.Count)
                {
                    lootableItems.Resize(lootTableListData.items.Count);
                }
                GUILayout.Box("Item List " + lootableItems.Count);
                filteredItems = lootableFilter.Count > 0 ? GetItemsByFilter(lootTableListData.items, lootableFilter) : lootTableListData.items;

                if (!inAddItem && filteredItems.Count > 0 && GUILayout.Button("Add Item", EditorStyles.miniButton))
                {
                    inAddItem = true;
                }
                if (inAddItem && filteredItems.Count > 0)
                {
                    GUILayout.BeginVertical("box");
                    selectedItem = EditorGUILayout.Popup(new GUIContent("SelectItem"), selectedItem, GetItemContents(filteredItems));
                    bool isValid = true;
                    var indexSelected = lootTableListData.items.IndexOf(filteredItems[selectedItem]);
                    if (lootableItems.Find(i => i.id == lootTableListData.items[indexSelected].id) != null)
                    {
                        isValid = false;
                        EditorGUILayout.HelpBox("This item already exist", MessageType.Error);
                    }
                    GUILayout.BeginHorizontal();

                    if (isValid && GUILayout.Button("Add", EditorStyles.miniButton))
                    {
                        itemReferenceList.arraySize++;
                        itemReferenceList.GetArrayElementAtIndex(itemReferenceList.arraySize - 1).FindPropertyRelative("id").intValue = lootTableListData.items[indexSelected].id;
                        EditorUtility.SetDirty(lootTable);
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
                for (int i = 0; i < lootableItems.Count; i++)
                {
                    var item = lootTableListData.items.Find(t => t.id.Equals(lootableItems[i].id));
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
                        lootableItems[i].amount = EditorGUILayout.IntField(lootableItems[i].amount, GUILayout.Width(40));

                        if (lootableItems[i].amount < 1)
                        {
                            lootableItems[i].amount = 1;
                        }

                        GUILayout.EndHorizontal();
                        if (item.attributes.Count > 0)
                            lootableItems[i].changeAttributes = GUILayout.Toggle(lootableItems[i].changeAttributes, new GUIContent("Change Attributes", "This is a override of the original loot item attributes"), EditorStyles.miniButton, GUILayout.Width(100));
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
                            if (lootTableListData.inEdition)
                            {
                                if (vItemListWindow.Instance != null)
                                    vItemListWindow.SetCurrentSelectedItem(lootTableListData.items.IndexOf(item));
                                else
                                    vItemListWindow.CreateWindow(lootTableListData, lootTableListData.items.IndexOf(item));
                            }
                            else
                                vItemListWindow.CreateWindow(lootTableListData, lootTableListData.items.IndexOf(item));
                        }
                        GUI.backgroundColor = backgroundColor;
                        if (item.attributes != null && item.attributes.Count > 0)
                        {

                            if (lootableItems[i].changeAttributes)
                            {
                                if (GUILayout.Button("Reset", EditorStyles.miniButton))
                                {
                                    lootableItems[i].attributes = null;

                                }
                                if (lootableItems[i].attributes == null)
                                {
                                    lootableItems[i].attributes = item.attributes.CopyAsNew();
                                }
                                else if (lootableItems[i].attributes.Count != item.attributes.Count)
                                {
                                    lootableItems[i].attributes = item.attributes.CopyAsNew();
                                }
                                else
                                {
                                    for (int a = 0; a < lootableItems[i].attributes.Count; a++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Label(lootableItems[i].attributes[a].name.ToString());
                                        lootableItems[i].attributes[a].value = EditorGUILayout.IntField(lootableItems[i].attributes[a].value, GUILayout.MaxWidth(60));
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
                        EditorUtility.SetDirty(lootTable);
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
                    EditorUtility.SetDirty(lootTable);
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

    }

}

