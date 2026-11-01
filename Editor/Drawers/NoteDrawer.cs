using UnityEditor;
using UnityEngine;

namespace UUP.EditorTools.Editor
{
    [CustomPropertyDrawer(typeof(NoteAttribute))]
    internal sealed class NoteDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 70f;
        private const float ButtonHeight = 20f;
        private const float Spacing = 2f;
        private const float IconMinHeight = 38f;
        private const float IconWidth = 36f;
        private const string Placeholder = "Empty note. Double-click to edit.";

        private NoteIcon Icon => ((NoteAttribute)attribute).Icon;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return EditorGUI.GetPropertyHeight(property, label, true);

            // The available width is not known here; approximate it from the inspector width.
            float width = Mathf.Max(EditorGUIUtility.currentViewWidth - 40f, 50f);

            if (IsEditing(property))
            {
                float textHeight = EditorStyles.textArea.CalcHeight(new GUIContent(property.stringValue), width);
                return Mathf.Max(textHeight, EditorGUIUtility.singleLineHeight * 3f) + Spacing + ButtonHeight;
            }

            string text = DisplayText(property);
            bool hasIcon = Icon != NoteIcon.None;
            float boxHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(text), hasIcon ? width - IconWidth : width);
            return Mathf.Max(boxHeight, hasIcon ? IconMinHeight : EditorGUIUtility.singleLineHeight + 4f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            if (IsEditing(property))
                DrawEditMode(position, property);
            else
                DrawReadMode(position, property);

            EditorGUI.EndProperty();
        }

        private static void DrawEditMode(Rect position, SerializedProperty property)
        {
            var textRect = new Rect(position.x, position.y, position.width, position.height - ButtonHeight - Spacing);

            EditorGUI.BeginChangeCheck();
            string text = EditorGUI.TextArea(textRect, property.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = text;

            var buttonRect = new Rect(position.xMax - ButtonWidth, textRect.yMax + Spacing, ButtonWidth, ButtonHeight);
            if (GUI.Button(buttonRect, "Done"))
            {
                SetEditing(property, false);
                GUI.FocusControl(null);
            }
        }

        private void DrawReadMode(Rect position, SerializedProperty property)
        {
            EditorGUI.HelpBox(position, DisplayText(property), ToMessageType(Icon));

            Event current = Event.current;
            if (current.type != EventType.MouseDown || !position.Contains(current.mousePosition))
                return;

            if (current.button == 0 && current.clickCount == 2)
            {
                SetEditing(property, true);
                current.Use();
            }
            else if (current.button == 1)
            {
                var menu = new GenericMenu();
                SerializedProperty copy = property.Copy();
                menu.AddItem(new GUIContent("Edit Note"), false, () => SetEditing(copy, true));
                menu.ShowAsContext();
                current.Use();
            }
        }

        private static string DisplayText(SerializedProperty property)
        {
            if (property.hasMultipleDifferentValues)
                return "(Multiple values)";
            return string.IsNullOrEmpty(property.stringValue) ? Placeholder : property.stringValue;
        }

        private static MessageType ToMessageType(NoteIcon icon)
        {
            switch (icon)
            {
                case NoteIcon.Info: return MessageType.Info;
                case NoteIcon.Warning: return MessageType.Warning;
                case NoteIcon.Error: return MessageType.Error;
                default: return MessageType.None;
            }
        }

        // Editing state is kept per object and field for the editor session.
        // The hash code of a UnityEngine.Object identifies it for the whole session.
        private static string StateKey(SerializedProperty property)
        {
            return $"UUP.EditorTools.Note.{property.serializedObject.targetObject.GetEntityId()}.{property.propertyPath}";
        }

        private static bool IsEditing(SerializedProperty property)
        {
            return SessionState.GetBool(StateKey(property), string.IsNullOrEmpty(property.stringValue));
        }

        private static void SetEditing(SerializedProperty property, bool editing)
        {
            SessionState.SetBool(StateKey(property), editing);
        }
    }
}
