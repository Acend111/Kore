using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Invector;

namespace EviLA.AddOns.RPGPack
{
    public class vQuestEnumsWindow : EditorWindow
    {
        public vQuestEnumsList[] datas;
        public List<string> _questTypeNames = new List<string>();
        public List<string> _questAttributeNames = new List<string>();
        public List<string> _questStateNames = new List<string>();
        public List<string> _questTargetTypeNames = new List<string>();
        public List<string> _questTriggerTypeNames = new List<string>();

        public GUISkin skin;
        public Vector2 scrollView;
        public static vQuestEnumsWindow instance;
        [MenuItem("Invector/Quests/QuestEnums/Open QuestEnums Editor")]
        public static void CreateWindow()
        {
            if (instance == null)
            {
                var window = vQuestEnumsWindow.GetWindow<vQuestEnumsWindow>("Quest Enums", true);
                instance = window;
                window.skin = Resources.Load("skin") as GUISkin;
                #region Get all vQuestType values of current Enum
                try
                {
                    window._questTypeNames = Enum.GetNames(typeof(vQuestType)).vToList();

                }
                catch
                {

                }
                #endregion

                #region Get all vQuestStates values of current Enum
                try
                {
                    window._questStateNames = Enum.GetNames(typeof(vQuestState)).vToList();

                }
                catch
                {

                }
                #endregion

                #region Get all vItemAttributes values of current Enum
                try
                {
                    window._questAttributeNames = Enum.GetNames(typeof(vQuestAttributes)).vToList();
                }
                catch
                {

                }
                #endregion


                #region Get all vQuestTargetTypes values of current Enum
                try
                {
                    window._questTargetTypeNames = Enum.GetNames(typeof(vQuestTargetType)).vToList();
                }
                catch
                {

                }
                #endregion

                window.datas = Resources.LoadAll<vQuestEnumsList>("");
                window.minSize = new Vector2(460, 600);

            }

        }

        protected virtual void OnGUI()
        {
            if (skin) GUI.skin = skin;
            this.minSize = new Vector2(460, 600);
            GUILayout.BeginVertical("box");
            GUILayout.Box("vQuestEnums");
            GUILayout.BeginHorizontal();
            #region Quest Type current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Quest Types", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _questTypeNames.Count; i++)
            {
                GUILayout.Label(_questTypeNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest State current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Quest States", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _questStateNames.Count; i++)
            {
                GUILayout.Label(_questStateNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest Attribute current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Quest Attributes", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _questAttributeNames.Count; i++)
            {
                GUILayout.Label(_questAttributeNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion


            #region Quest Target Types current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Quest Targets", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _questTargetTypeNames.Count; i++)
            {
                GUILayout.Label(_questTargetTypeNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest Trigger Types current
            GUILayout.BeginVertical("box", GUILayout.Width(215));
            GUILayout.Box("Quest Trigger Types", GUILayout.ExpandWidth(true));

            for (int i = 0; i < _questTriggerTypeNames.Count; i++)
            {
                GUILayout.Label(_questTriggerTypeNames[i], EditorStyles.miniBoldLabel);
            }

            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            GUILayout.Box("Edit Enums", GUILayout.ExpandHeight(false));
            scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.MinHeight(100), GUILayout.MaxHeight(600));
            for (int i = 0; i < datas.Length; i++)
            {
                DrawQuestEnumListData(datas[i]);
            }
            GUILayout.EndScrollView();
            EditorGUILayout.HelpBox("If the field has is gray, the vQuestEnums contains same value", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Refresh QuestEnums", "Save and refesh changes to vQuestEnums")))
            {
                vQuestEnumsBuilder.RefreshQuestEnums();
            }
            GUILayout.EndVertical();
        }

        protected void DrawQuestEnumListData(vQuestEnumsList data)
        {
            SerializedObject _data = new SerializedObject(data);
            _data.Update();
            GUILayout.BeginVertical("box");
            GUILayout.Box(data.name, GUILayout.ExpandWidth(true));
            EditorGUILayout.ObjectField(data, typeof(vQuestEnumsList), false);
            GUILayout.BeginHorizontal();

            #region Quest Types
            var questTypeEnumValueList = _data.FindProperty("questTypeEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Quest Types", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(questTypeEnumValueList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();

            var labelWidht = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 30f;
            var color = GUI.color;
            for (int i = 0; i < questTypeEnumValueList.arraySize; i++)
            {
                if (_questTypeNames.Contains(questTypeEnumValueList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(questTypeEnumValueList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest States
            var questStateEnumValueList = _data.FindProperty("questStateEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Quest States", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(questStateEnumValueList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();

            GUI.color = color;
            for (int i = 0; i < questStateEnumValueList.arraySize; i++)
            {
                if (_questStateNames.Contains(questStateEnumValueList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(questStateEnumValueList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest Attributes
            GUI.color = color;
            var questAttributesEnumValuesList = _data.FindProperty("questAttributesEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Quest Attributes", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(questAttributesEnumValuesList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();


            for (int i = 0; i < questAttributesEnumValuesList.arraySize; i++)
            {
                if (_questAttributeNames.Contains(questAttributesEnumValuesList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(questAttributesEnumValuesList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest Target Types
            GUI.color = color;
            var questTargetTypesEnumValuesList = _data.FindProperty("questTargetTypeEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Quest Target Types", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(questTargetTypesEnumValuesList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();


            for (int i = 0; i < questTargetTypesEnumValuesList.arraySize; i++)
            {
                if (_questTargetTypeNames.Contains(questTargetTypesEnumValuesList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(questTargetTypesEnumValuesList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            #region Quest Trigger Types
            GUI.color = color;
            var questTriggerTypesEnumValuesList = _data.FindProperty("questTriggerTypeEnumValues");
            GUILayout.BeginVertical("box", GUILayout.Width(200));
            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
            GUILayout.Label("Quest Trigger Types", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(questTriggerTypesEnumValuesList.FindPropertyRelative("Array.size"), GUIContent.none);
            GUILayout.EndHorizontal();


            for (int i = 0; i < questTriggerTypesEnumValuesList.arraySize; i++)
            {
                if (_questTriggerTypeNames.Contains(questTriggerTypesEnumValuesList.GetArrayElementAtIndex(i).stringValue))
                    GUI.color = Color.gray;
                else GUI.color = color;
                EditorGUILayout.PropertyField(questTriggerTypesEnumValuesList.GetArrayElementAtIndex(i), new GUIContent(i.ToString()));
            }

            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            _data.ApplyModifiedProperties();
            if (_data.ApplyModifiedProperties())
                EditorUtility.SetDirty(data);
            EditorGUIUtility.labelWidth = labelWidht;
            GUI.color = color;
        }
    }
}


