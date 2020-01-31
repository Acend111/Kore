using UnityEditor;
using UnityEngine;
using Invector;

namespace EviLA.AddOns.RPGPack.Experience
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AnimatorStatTrend))]
    public class AnimatorStatTrendEditor : Editor
    {
        private string[] hideProperties = { "trend" };
        private string[] disabledProperties = { "type" };

        public override void OnInspectorGUI()
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                if (disabledProperties.vToList().Contains(prop.name))
                    GUI.enabled = false;
                else
                    GUI.enabled = true;

                if (!hideProperties.vToList().Contains(prop.name))
                    EditorGUILayout.PropertyField(prop);
            }

            so.ApplyModifiedProperties();
        }
    }
}

