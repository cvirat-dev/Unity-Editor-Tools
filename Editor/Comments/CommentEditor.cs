using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Inspector of the <see cref="Comment"/> component with a read view (message box) and an edit view.
    /// </summary>
    [CustomEditor(typeof(Comment)), CanEditMultipleObjects]
    internal sealed class CommentEditor : UnityEditor.Editor
    {
        private const string EmptyText = "Empty comment. Double-click to edit.";

        private SerializedProperty _textProperty;
        private SerializedProperty _iconProperty;
        private VisualElement _content;

        // The hash code of a UnityEngine.Object identifies it for the whole editor session.
        private string StateKey => $"UUP.EditorTools.Comment.{target.GetEntityId()}.IsEditing";

        private bool IsEditing
        {
            get => SessionState.GetBool(StateKey, false);
            set => SessionState.SetBool(StateKey, value);
        }

        public override VisualElement CreateInspectorGUI()
        {
            _textProperty = serializedObject.FindProperty(Comment.TextFieldName);
            _iconProperty = serializedObject.FindProperty(Comment.IconFieldName);

            var root = new VisualElement();
            _content = new VisualElement();
            root.Add(_content);

            if (IsEditing || (!_textProperty.hasMultipleDifferentValues && string.IsNullOrEmpty(_textProperty.stringValue)))
                ShowEditView();
            else
                ShowReadView();

            return root;
        }

        private void ShowReadView()
        {
            IsEditing = false;
            _content.Clear();

            var box = new HelpBox(DisplayText(), ToMessageType(_iconProperty))
            {
                tooltip = "Double-click to edit."
            };
            box.style.marginLeft = 0f;
            box.style.marginRight = 0f;

            box.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0 && evt.clickCount == 2)
                {
                    ShowEditView();
                    evt.StopPropagation();
                }
            });
            box.AddManipulator(new ContextualMenuManipulator(evt =>
                evt.menu.AppendAction("Edit Comment", _ => ShowEditView())));

            // Keep the box in sync with changes made elsewhere (undo, other inspectors, scripts).
            box.TrackPropertyValue(_textProperty, _ => box.text = DisplayText());
            box.TrackPropertyValue(_iconProperty, property => box.messageType = ToMessageType(property));

            _content.Add(box);
        }

        private void ShowEditView()
        {
            IsEditing = true;
            _content.Clear();

            var textField = new TextField
            {
                multiline = true,
                bindingPath = Comment.TextFieldName,
                tooltip = "Press Ctrl/Cmd + Enter to finish editing."
            };
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.style.minHeight = 60f;
            textField.RegisterCallback<KeyDownEvent>(evt =>
            {
                bool submit = evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter;
                if (submit && (evt.ctrlKey || evt.commandKey))
                {
                    ShowReadView();
                    evt.StopPropagation();
                }
            }, TrickleDown.TrickleDown);

            var iconField = new EnumField("Icon") { bindingPath = Comment.IconFieldName };

            var doneButton = new Button(ShowReadView) { text = "Done" };
            doneButton.style.alignSelf = Align.FlexEnd;
            doneButton.style.width = 80f;

            _content.Add(textField);
            _content.Add(iconField);
            _content.Add(doneButton);
            _content.Bind(serializedObject);

            textField.schedule.Execute(() => textField.Focus());
        }

        private string DisplayText()
        {
            if (_textProperty.hasMultipleDifferentValues)
                return "(Multiple values)";
            return string.IsNullOrEmpty(_textProperty.stringValue) ? EmptyText : _textProperty.stringValue;
        }

        private static HelpBoxMessageType ToMessageType(SerializedProperty iconProperty)
        {
            switch ((NoteIcon)iconProperty.enumValueIndex)
            {
                case NoteIcon.Info: return HelpBoxMessageType.Info;
                case NoteIcon.Warning: return HelpBoxMessageType.Warning;
                case NoteIcon.Error: return HelpBoxMessageType.Error;
                default: return HelpBoxMessageType.None;
            }
        }
    }
}
