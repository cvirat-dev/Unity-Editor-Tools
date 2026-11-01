using System;
using UnityEngine;

namespace UUP.EditorTools
{
    /// <summary>
    /// Draws a title, an optional subtitle and an optional separator line above the decorated field.
    /// </summary>
    /// <remarks>
    /// A richer alternative to Unity's <c>[Header]</c>. Like <c>[Header]</c>, it is a decorator: it can be combined
    /// with any other attribute and is drawn once above arrays and lists.
    /// </remarks>
    /// <example>
    /// <code>
    /// [Title("Movement", "Values are in meters per second")]
    /// [SerializeField] private float walkSpeed = 4f;
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TitleAttribute : PropertyAttribute
    {
        /// <summary>
        /// Creates a title decorator.
        /// </summary>
        /// <param name="title">The title text.</param>
        /// <param name="subtitle">Optional smaller text shown below the title.</param>
        /// <param name="alignment">Horizontal alignment of the texts (use the Middle* values).</param>
        /// <param name="drawLine">Whether a separator line is drawn below the texts.</param>
        /// <param name="bold">Whether the title is drawn in bold.</param>
        public TitleAttribute(
            string title,
            string subtitle = null,
            TextAnchor alignment = TextAnchor.MiddleLeft,
            bool drawLine = true,
            bool bold = true)
        {
            Title = title;
            Subtitle = subtitle;
            Alignment = alignment;
            DrawLine = drawLine;
            Bold = bold;
        }

        /// <summary>The title text.</summary>
        public string Title { get; }

        /// <summary>Optional smaller text shown below the title.</summary>
        public string Subtitle { get; }

        /// <summary>Alignment of the texts.</summary>
        public TextAnchor Alignment { get; }

        /// <summary>Whether a separator line is drawn below the texts.</summary>
        public bool DrawLine { get; }

        /// <summary>Whether the title is drawn in bold.</summary>
        public bool Bold { get; }
    }
}
