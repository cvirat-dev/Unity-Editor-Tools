using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Inspector that draws Unity's default inspector followed by the <see cref="InspectorButtonAttribute"/> buttons.
    /// </summary>
    /// <remarks>
    /// The concrete editors below are registered as <i>fallback</i> editors: they are only used for classes that
    /// have no other custom editor, so they never replace your own editors or those of other packages.
    /// </remarks>
    public abstract class InspectorButtonEditorBase : UnityEditor.Editor
    {
        /// <inheritdoc />
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            if (target != null && InspectorButtonGUI.HasButtons(target.GetType()))
                root.Add(new IMGUIContainer(() => InspectorButtonGUI.DrawButtons(targets)));

            // Property drawers that depend on other fields (such as [ShowIf]) are drawn with IMGUI.
            // Re-layout them whenever any value of the object changes so they update immediately.
            root.TrackSerializedObjectValue(serializedObject, _ => RefreshImguiContainers(root));

            return root;
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            InspectorButtonGUI.DrawButtons(targets);
        }

        private static void RefreshImguiContainers(VisualElement root)
        {
            root.Query<IMGUIContainer>().ForEach(container => container.MarkDirtyLayout());
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true), CanEditMultipleObjects]
    internal sealed class MonoBehaviourButtonEditor : InspectorButtonEditorBase
    {
    }

    [CustomEditor(typeof(ScriptableObject), true, isFallback = true), CanEditMultipleObjects]
    internal sealed class ScriptableObjectButtonEditor : InspectorButtonEditorBase
    {
    }
}
