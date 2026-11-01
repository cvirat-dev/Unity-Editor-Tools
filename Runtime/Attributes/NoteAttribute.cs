using System;
using UnityEngine;

namespace UUP.EditorTools
{
    /// <summary>
    /// Displays a <c>string</c> field as a read-only note box in the Inspector, with an optional icon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Double-click the note (or right-click &gt; Edit Note) to edit its text, then click <b>Done</b>.
    /// Empty notes start in edit mode.
    /// </para>
    /// <para>
    /// The text is regular serialized data, so it is saved with the object and included in builds. To annotate
    /// GameObjects without any runtime cost, use the <see cref="Comment"/> component instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField, Note(NoteIcon.Warning)]
    /// private string setupNote = "Assign the spawn points before entering Play Mode.";
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NoteAttribute : PropertyAttribute
    {
        /// <summary>
        /// Creates a note.
        /// </summary>
        /// <param name="icon">Icon shown next to the text.</param>
        public NoteAttribute(NoteIcon icon = NoteIcon.Info)
        {
            Icon = icon;
        }

        /// <summary>Icon shown next to the text.</summary>
        public NoteIcon Icon { get; }
    }
}
