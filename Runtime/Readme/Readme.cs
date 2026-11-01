using System;
using System.Collections.Generic;
using UnityEngine;

namespace UUP.EditorTools
{
    /// <summary>
    /// An asset that displays formatted documentation in the Inspector: a title, an icon and sections with text and links.
    /// </summary>
    /// <remarks>
    /// Create one with <b>Assets &gt; Create &gt; Editor Tools &gt; Readme</b> and place it in a folder to explain its
    /// content to your team. Click <b>Edit</b> in the top-right corner of the Inspector to change it.
    /// Section texts support Unity rich text tags such as <c>&lt;b&gt;</c>, <c>&lt;i&gt;</c> and <c>&lt;color&gt;</c>.
    /// </remarks>
    [CreateAssetMenu(menuName = "Editor Tools/Readme", fileName = "Readme", order = 0)]
    public sealed class Readme : ScriptableObject
    {
        [SerializeField, Tooltip("Optional icon shown next to the title.")]
        private Texture2D icon;

        [SerializeField, Tooltip("Title of the Readme.")]
        private string title = string.Empty;

        [SerializeField, Tooltip("Content of the Readme, displayed from top to bottom.")]
        private List<Section> sections = new List<Section>();

        /// <summary>Optional icon shown next to the title.</summary>
        public Texture2D Icon => icon;

        /// <summary>Title of the Readme.</summary>
        public string Title => title;

        /// <summary>Content of the Readme, displayed from top to bottom.</summary>
        public IReadOnlyList<Section> Sections => sections;

        /// <summary>
        /// A block of content: an optional heading, text and link.
        /// </summary>
        [Serializable]
        public sealed class Section
        {
            [Tooltip("Optional heading shown in bold above the text.")]
            public string heading;

            [TextArea(3, 12), Tooltip("Body text. Supports rich text tags.")]
            public string text;

            [Tooltip("Optional text of a link shown below the body text.")]
            public string linkText;

            [Tooltip("URL opened when the link is clicked. Only absolute http/https URLs are allowed.")]
            public string url;
        }
    }
}
