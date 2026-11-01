using UnityEditor;
using UnityEngine;

namespace UUP.EditorTools.Editor
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    internal sealed class TitleDecoratorDrawer : DecoratorDrawer
    {
        private const float TopSpacing = 8f;
        private const float BottomSpacing = 4f;
        private const float LineThickness = 1f;
        private const float LineSpacing = 2f;

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;

        private TitleAttribute Title => (TitleAttribute)attribute;

        private bool HasSubtitle => !string.IsNullOrEmpty(Title.Subtitle);

        private static Color LineColor => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.2f)
            : new Color(0f, 0f, 0f, 0.3f);

        public override float GetHeight()
        {
            float height = TopSpacing + EditorGUIUtility.singleLineHeight + BottomSpacing;
            if (HasSubtitle)
                height += EditorGUIUtility.singleLineHeight;
            if (Title.DrawLine)
                height += LineSpacing + LineThickness;
            return height;
        }

        public override void OnGUI(Rect position)
        {
            EnsureStyles();

            var rect = new Rect(position.x, position.y + TopSpacing, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, Title.Title, _titleStyle);

            if (HasSubtitle)
            {
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, Title.Subtitle, _subtitleStyle);
            }

            if (Title.DrawLine)
            {
                var lineRect = new Rect(position.x, rect.yMax + LineSpacing, position.width, LineThickness);
                EditorGUI.DrawRect(lineRect, LineColor);
            }
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
                return;

            _titleStyle = new GUIStyle(Title.Bold ? EditorStyles.boldLabel : EditorStyles.label)
            {
                alignment = Title.Alignment
            };
            _subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = Title.Alignment
            };
        }
    }
}
