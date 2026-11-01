using UnityEditor;
using UnityEngine;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Draws a checkbox on the right of each GameObject in the Hierarchy window to toggle its active state.
    /// </summary>
    /// <remarks>
    /// Toggling a selected GameObject applies to the whole selection. Changes support Undo.
    /// Disable it in <b>Preferences &gt; Editor Tools</b>.
    /// </remarks>
    [InitializeOnLoad]
    internal static class HierarchyActiveToggle
    {
        private const float ToggleWidth = 16f;

        // Unity 6.4 replaced instance IDs with EntityId in the Hierarchy callback; 6.5 removed the old callback.
#if UNITY_6000_4_OR_NEWER
        static HierarchyActiveToggle()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(EntityId entityId, Rect selectionRect)
        {
            DrawToggle(EditorUtility.EntityIdToObject(entityId) as GameObject, selectionRect);
        }
#else
        static HierarchyActiveToggle()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            DrawToggle(EditorUtility.InstanceIDToObject(instanceID) as GameObject, selectionRect);
        }
#endif

        private static void DrawToggle(GameObject gameObject, Rect selectionRect)
        {
            if (gameObject == null || !EditorToolsSettings.HierarchyActiveToggle)
                return;

            var toggleRect = new Rect(selectionRect.xMax - ToggleWidth, selectionRect.y, ToggleWidth, selectionRect.height);

            EditorGUI.BeginChangeCheck();
            bool active = GUI.Toggle(toggleRect, gameObject.activeSelf, GUIContent.none);
            if (!EditorGUI.EndChangeCheck())
                return;

            GameObject[] targets = Selection.Contains(gameObject) ? Selection.gameObjects : new[] { gameObject };
            Undo.RecordObjects(targets, active ? "Activate GameObject" : "Deactivate GameObject");
            foreach (GameObject target in targets)
                target.SetActive(active);
        }
    }
}
