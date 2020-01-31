using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace EviLA.AddOns.RPGPack
{
    [CustomEditor(typeof(vQuestListData))]
    public class vQuestListEditor : Editor
    {
        [SerializeField]
        protected GUISkin skin;
        [SerializeField]
        protected vQuestListData questList;

        protected virtual void OnEnable()
        {
            questList = (vQuestListData)target;
            skin = Resources.Load("skin") as GUISkin;
        }

        [MenuItem("Invector/Quests/Create New QuestListData")]
        static void CreateNewListData()
        {
            vQuestListData listData = ScriptableObject.CreateInstance<vQuestListData>();
            AssetDatabase.CreateAsset(listData, "Assets/QuestListData.asset");
        }

        public override void OnInspectorGUI()
        {
            if (skin) GUI.skin = skin;

            serializedObject.Update();

            GUI.enabled = !Application.isPlaying;

            GUILayout.BeginVertical("Quest List", "window");
            GUILayout.Space(30);

            if (questList.itemsHidden && !questList.inEdition && GUILayout.Button("Edit Quests in List"))
            {
                vQuestListWindow.CreateWindow(questList);
            }
            else if (questList.inEdition)
            {
                if (vQuestListWindow.Instance != null)
                {
                    if (vQuestListWindow.Instance.questList == null)
                    {
                        vQuestListWindow.Instance.Close();
                    }
                    else
                        EditorGUILayout.HelpBox("The Quest List Window is open", MessageType.Info);
                }
                else
                {
                    questList.inEdition = false;
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button(questList.itemsHidden ? "Show quests in Hierarchy" : "Hide quests in Hierarchy"))
            {
                ShowAllQuests();
            }
            GUILayout.EndVertical();
            if (GUI.changed || serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(target);

            }
        }
        public virtual void ShowAllQuests()
        {
            if (questList.itemsHidden)
            {
                foreach (vQuest quest in questList.quests)
                {
                    quest.hideFlags = HideFlags.None;
                    EditorUtility.SetDirty(quest);
                }
                questList.itemsHidden = false;
            }
            else
            {
                foreach (vQuest quest in questList.quests)
                {
                    quest.hideFlags = HideFlags.HideInHierarchy;
                    EditorUtility.SetDirty(quest);
                }
                questList.itemsHidden = true;
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

        }
    }
}
