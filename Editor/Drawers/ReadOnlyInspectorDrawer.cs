using UnityEditor;
using UnityEngine;

namespace UUP.EditorTools.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    internal sealed class ReadOnlyInspectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
