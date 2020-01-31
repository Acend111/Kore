using UnityEngine;
using UnityEditor;
using EviLA.AddOns.RPGPack.Helpers;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack.Persistence
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(vCanSaveYou))]
    public class vCanSaveYouEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                if (prop.name.Equals("guid"))
                    if (prop.stringValue.Equals(""))
                    {
                        var guid = System.Guid.NewGuid();
                        prop.stringValue = guid.ToString();
                    }
                EditorGUILayout.PropertyField(prop);
            }

            so.ApplyModifiedProperties();
        }
    }

}