using UnityEditor;
using Invector;

namespace EviLA.AddOns.RPGPack.Experience
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeleeWeaponStatComponent))]
    public class MeleeWeaponStatComponentEditor : Editor
    {
        private string[] hideProperties = { "type", "isBool", "isNumeric", "destroyAfterUse" };

        public override void OnInspectorGUI()
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
                if (!hideProperties.vToList().Contains(prop.name))
                    EditorGUILayout.PropertyField(prop);

            so.ApplyModifiedProperties();
        }
    }
}