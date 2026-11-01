using System;
using UnityEditor;
using UnityEngine;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Displays a <see cref="Readme"/> asset as formatted documentation, with an Edit toggle to change its content.
    /// </summary>
    [CustomEditor(typeof(Readme))]
    internal sealed class ReadmeEditor : UnityEditor.Editor
    {
        private const float MaxIconSize = 64f;
        private const float SectionSpacing = 12f;

        private static GUIStyle s_TitleStyle;
        private static GUIStyle s_HeadingStyle;
        private static GUIStyle s_BodyStyle;

        private bool _isEditing;

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            var readme = (Readme)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                _isEditing = GUILayout.Toggle(_isEditing, _isEditing ? "Done" : "Edit", EditorStyles.miniButton, GUILayout.Width(60f));
            }

            if (_isEditing)
            {
                DrawDefaultInspector();
                return;
            }

            if (string.IsNullOrEmpty(readme.Title) && readme.Sections.Count == 0)
            {
                EditorGUILayout.HelpBox("This Readme is empty. Click 'Edit' to add a title and sections.", MessageType.Info);
                return;
            }

            DrawHeader(readme);
            foreach (Readme.Section section in readme.Sections)
                DrawSection(section);
        }

        private static void DrawHeader(Readme readme)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (readme.Icon != null)
                {
                    float size = Mathf.Min(MaxIconSize, EditorGUIUtility.currentViewWidth / 4f);
                    GUILayout.Label(readme.Icon, GUILayout.Width(size), GUILayout.Height(size));
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(readme.Title, s_TitleStyle);
                    GUILayout.FlexibleSpace();
                }
            }

            GUILayout.Space(SectionSpacing);
        }

        private static void DrawSection(Readme.Section section)
        {
            if (!string.IsNullOrEmpty(section.heading))
                GUILayout.Label(section.heading, s_HeadingStyle);

            if (!string.IsNullOrEmpty(section.text))
                GUILayout.Label(section.text, s_BodyStyle);

            if (!string.IsNullOrEmpty(section.linkText) && !string.IsNullOrEmpty(section.url))
            {
                if (IsValidUrl(section.url))
                {
                    if (EditorGUILayout.LinkButton(section.linkText))
                        Application.OpenURL(section.url);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"Link '{section.linkText}' has an invalid or unsupported URL: {section.url}\n" +
                        "Only absolute http/https URLs are allowed.",
                        MessageType.Warning);
                }
            }

            GUILayout.Space(SectionSpacing);
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static void InitializeStyles()
        {
            if (s_BodyStyle != null)
                return;

            s_BodyStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, richText = true, fontSize = 13 };
            s_HeadingStyle = new GUIStyle(s_BodyStyle) { fontStyle = FontStyle.Bold, fontSize = 15 };
            s_TitleStyle = new GUIStyle(s_BodyStyle) { fontStyle = FontStyle.Bold, fontSize = 20 };
        }
    }
}
