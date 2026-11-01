using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// User preferences of the package, shown in <b>Edit &gt; Preferences &gt; Editor Tools</b>
    /// (<b>Unity &gt; Settings &gt; Editor Tools</b> on macOS).
    /// </summary>
    public static class EditorToolsSettings
    {
        private const string KeyPrefix = "UUP.EditorTools.";
        private const string HierarchyToggleKey = KeyPrefix + "HierarchyActiveToggle";
        private const string LogStrippedCommentsKey = KeyPrefix + "LogStrippedComments";

        /// <summary>Whether a checkbox to toggle the active state of GameObjects is drawn in the Hierarchy window.</summary>
        public static bool HierarchyActiveToggle
        {
            get => EditorPrefs.GetBool(HierarchyToggleKey, true);
            set
            {
                EditorPrefs.SetBool(HierarchyToggleKey, value);
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        /// <summary>Whether the Comment components removed during a build are listed in the Console.</summary>
        public static bool LogStrippedComments
        {
            get => EditorPrefs.GetBool(LogStrippedCommentsKey, true);
            set => EditorPrefs.SetBool(LogStrippedCommentsKey, value);
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Preferences/Editor Tools", SettingsScope.User)
            {
                label = "Editor Tools",
                guiHandler = _ => DrawGUI(),
                keywords = new HashSet<string> { "Hierarchy", "Active", "Toggle", "Comment", "Build" }
            };
        }

        private static void DrawGUI()
        {
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 260f;

            EditorGUILayout.LabelField("Hierarchy", EditorStyles.boldLabel);
            HierarchyActiveToggle = EditorGUILayout.Toggle(
                new GUIContent("Show active toggles", "Draws a checkbox on the right of each GameObject in the Hierarchy window to activate or deactivate it."),
                HierarchyActiveToggle);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Comment component", EditorStyles.boldLabel);
            LogStrippedComments = EditorGUILayout.Toggle(
                new GUIContent("Log comments removed from builds", "Lists the Comment components removed from the build in the Console."),
                LogStrippedComments);

            EditorGUIUtility.labelWidth = previousLabelWidth;
        }
    }
}
