using UnityEditor;
using UnityEngine;
using Invector;

namespace EviLA.AddOns.RPGPack.Experience
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeleeBaseDamageTrend))]
    public class MeleeBaseDamageEditor : Editor
    {
        private string[] hideProperties = { "isBool", "isNumeric", "type" };
        private string[] disabledProperties = { };

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