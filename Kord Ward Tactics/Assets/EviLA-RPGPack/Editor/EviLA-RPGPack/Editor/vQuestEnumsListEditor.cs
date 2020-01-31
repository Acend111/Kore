using UnityEditor;
using UnityEngine;
using Invector;

namespace EviLA.AddOns.RPGPack.DynamicEnum
{

    [CustomEditor(typeof(vQuestEnumsList))]
    public class vQuestEnumsListEditor : Editor
    {
        public GUISkin skin;
        protected virtual void OnEnable()
        {
            skin = Resources.Load("skin") as GUISkin;
        }
        public override void OnInspectorGUI()
        {
            if (skin) GUI.skin = skin;
            var assetPath = AssetDatabase.GetAssetPath(target);
            GUILayout.BeginVertical("vQuestEnums List", "window");
            GUILayout.Space(30);
            if (assetPath.Contains("Resources"))
            {
                GUILayout.BeginVertical("box");
                base.OnInspectorGUI();
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Open QuestEnums Editor"))
                {
                    vQuestEnumsWindow.CreateWindow();
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Refresh QuestEnums"))
                {
                    vQuestEnumsBuilder.RefreshQuestEnums();
                }

                EditorGUILayout.HelpBox("-This list will be merged with other lists and create the enums.\n- The Enum Generator will ignore equal values.\n- If our change causes errors, check which enum value is missing and adds to the list and press the refresh button.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Please put this list in Resources folder", MessageType.Warning);
            }
            GUILayout.EndVertical();
        }


        [MenuItem("Invector/Quests/QuestEnums/Create New vQuestEnumsList")]
        internal static void CreateDefaultItemEnum()
        {
            vScriptableObjectUtility.CreateAsset<vQuestEnumsList>();
        }

    }
}