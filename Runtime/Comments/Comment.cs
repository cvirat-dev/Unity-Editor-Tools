using UnityEngine;

namespace UUP.EditorTools
{
    /// <summary>
    /// Attaches a note to a GameObject, displayed as a message box in the Inspector.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use it to document scene setups for your team: why an object exists, what must not be changed, what is
    /// still to do, ... Double-click the comment to edit it, press <b>Ctrl/Cmd + Enter</b> or click <b>Done</b> to
    /// finish.
    /// </para>
    /// <para>
    /// Comments are removed from scenes when building a player, so they cost nothing at runtime. Comments on
    /// prefabs that are only instantiated at runtime are not removed, but they do nothing.
    /// </para>
    /// </remarks>
    [AddComponentMenu("Editor Tools/Comment")]
    public sealed class Comment : MonoBehaviour
    {
        internal const string TextFieldName = nameof(text);
        internal const string IconFieldName = nameof(icon);

        [SerializeField, TextArea(2, 10), Tooltip("The text of the comment.")]
        private string text = string.Empty;

        [SerializeField, Tooltip("The icon shown next to the text.")]
        private NoteIcon icon = NoteIcon.Info;

        /// <summary>The text of the comment.</summary>
        public string Text
        {
            get => text;
            set => text = value ?? string.Empty;
        }

        /// <summary>The icon shown next to the text.</summary>
        public NoteIcon Icon
        {
            get => icon;
            set => icon = value;
        }
    }
}
