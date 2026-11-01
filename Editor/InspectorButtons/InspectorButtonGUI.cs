using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Draws the buttons declared with <see cref="InspectorButtonAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Buttons are drawn automatically for every MonoBehaviour and ScriptableObject without a custom editor.
    /// If your class has its own custom editor, call <see cref="DrawButtons"/> at the end of its <c>OnInspectorGUI</c>:
    /// <code>
    /// public override void OnInspectorGUI()
    /// {
    ///     base.OnInspectorGUI();
    ///     InspectorButtonGUI.DrawButtons(targets);
    /// }
    /// </code>
    /// </remarks>
    public static class InspectorButtonGUI
    {
        private const BindingFlags DeclaredMethods =
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private static readonly Dictionary<Type, ButtonMethod[]> s_Cache = new Dictionary<Type, ButtonMethod[]>();

        /// <summary>
        /// Returns true if <paramref name="type"/> (or one of its base classes) declares at least one inspector button.
        /// </summary>
        public static bool HasButtons(Type type)
        {
            return type != null && GetButtonMethods(type).Length > 0;
        }

        /// <summary>
        /// Draws one button per method marked with <see cref="InspectorButtonAttribute"/> using IMGUI.
        /// Clicking a button invokes the method on every object in <paramref name="targets"/>,
        /// or once for static methods.
        /// </summary>
        /// <param name="targets">The inspected objects, usually <c>Editor.targets</c>.</param>
        public static void DrawButtons(Object[] targets)
        {
            if (targets == null || targets.Length == 0 || targets[0] == null)
                return;

            ButtonMethod[] buttons = GetButtonMethods(targets[0].GetType());
            if (buttons.Length == 0)
                return;

            EditorGUILayout.Space();
            foreach (ButtonMethod button in buttons)
                DrawButton(button, targets);
        }

        private static void DrawButton(ButtonMethod button, Object[] targets)
        {
            bool hasParameters = button.Parameters.Length > 0;
            if (hasParameters)
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bool parametersValid = DrawParameters(button);
            bool modeAllowed = IsModeAllowed(button.Attribute.Mode, out string modeTooltip);

            using (new EditorGUI.DisabledScope(!parametersValid || !modeAllowed))
            {
                var content = new GUIContent(button.Label, modeAllowed ? button.Tooltip : modeTooltip);
                if (GUILayout.Button(content))
                {
                    // Invoke outside of the GUI pass: methods that change the hierarchy, open dialogs or
                    // select objects would otherwise break the current IMGUI layout.
                    object[] arguments = (object[])button.Values.Clone();
                    Object[] capturedTargets = (Object[])targets.Clone();
                    EditorApplication.delayCall += () => Invoke(button, capturedTargets, arguments);
                }
            }

            if (hasParameters)
                EditorGUILayout.EndVertical();
        }

        private static bool DrawParameters(ButtonMethod button)
        {
            if (button.UnsupportedReason != null)
            {
                EditorGUILayout.HelpBox(button.UnsupportedReason, MessageType.Error);
                return false;
            }

            for (int i = 0; i < button.Parameters.Length; i++)
            {
                ParameterInfo parameter = button.Parameters[i];
                string label = ObjectNames.NicifyVariableName(parameter.Name);
                button.Values[i] = ParameterField.Draw(label, parameter.ParameterType, button.Values[i]);
            }

            return true;
        }

        private static bool IsModeAllowed(ButtonMode mode, out string tooltip)
        {
            switch (mode)
            {
                case ButtonMode.EditModeOnly:
                    tooltip = "Only available in Edit Mode.";
                    return !EditorApplication.isPlaying;
                case ButtonMode.PlayModeOnly:
                    tooltip = "Only available in Play Mode.";
                    return EditorApplication.isPlaying;
                default:
                    tooltip = null;
                    return true;
            }
        }

        private static void Invoke(ButtonMethod button, Object[] targets, object[] arguments)
        {
            if (button.Method.IsStatic)
            {
                InvokeOn(button, null, arguments, targets[0]);
                return;
            }

            Object[] liveTargets = targets.Where(t => t != null).ToArray();
            bool recordUndo = !EditorApplication.isPlaying;

            if (recordUndo)
                Undo.RecordObjects(liveTargets, button.Label);

            foreach (Object target in liveTargets)
            {
                InvokeOn(button, target, arguments, target);

                if (recordUndo)
                {
                    EditorUtility.SetDirty(target);
                    if (PrefabUtility.IsPartOfPrefabInstance(target))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }
        }

        private static void InvokeOn(ButtonMethod button, object instance, object[] arguments, Object context)
        {
            object result;
            try
            {
                result = button.Method.Invoke(instance, arguments);
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception, context);
                return;
            }

            if (result is IEnumerator routine)
            {
                if (EditorApplication.isPlaying && instance is MonoBehaviour behaviour && behaviour.isActiveAndEnabled)
                    behaviour.StartCoroutine(routine);
                else
                    Debug.LogWarning($"'{button.Label}' returns a coroutine, which can only be started in Play Mode on an active MonoBehaviour.", context);
                return;
            }

            if (button.Method.ReturnType != typeof(void))
                Debug.Log($"{button.Label} returned: {FormatResult(result)}", context);
        }

        private static string FormatResult(object result)
        {
            switch (result)
            {
                case null:
                    return "null";
                case string text:
                    return $"\"{text}\"";
                case Object unityObject:
                    return unityObject == null ? "null (destroyed)" : $"{unityObject.name} ({unityObject.GetType().Name})";
                case ICollection collection:
                    return $"{result.GetType().Name} with {collection.Count} element(s)";
                default:
                    return result.ToString();
            }
        }

        private static ButtonMethod[] GetButtonMethods(Type type)
        {
            if (s_Cache.TryGetValue(type, out ButtonMethod[] cached))
                return cached;

            var found = new List<(int depth, MethodInfo method, InspectorButtonAttribute attribute)>();
            var seenBaseDefinitions = new HashSet<MethodInfo>();
            int depth = 0;

            // Walk from the most derived type up: private methods of base classes are not returned by
            // Type.GetMethods, and overriding methods must win over the methods they override.
            for (Type current = type; current != null && !IsUnityBaseType(current); current = current.BaseType)
            {
                foreach (MethodInfo method in current.GetMethods(DeclaredMethods))
                {
                    var attribute = (InspectorButtonAttribute)Attribute.GetCustomAttribute(method, typeof(InspectorButtonAttribute), true);
                    if (attribute == null || method.ContainsGenericParameters)
                        continue;

                    if (seenBaseDefinitions.Add(method.GetBaseDefinition()))
                        found.Add((depth, method, attribute));
                }

                depth++;
            }

            // Show base class buttons first, then in declaration order.
            ButtonMethod[] result = found
                .OrderByDescending(entry => entry.depth)
                .ThenBy(entry => entry.method.MetadataToken)
                .Select(entry => new ButtonMethod(entry.method, entry.attribute))
                .ToArray();

            s_Cache[type] = result;
            return result;
        }

        private static bool IsUnityBaseType(Type type)
        {
            return type == typeof(MonoBehaviour) || type == typeof(ScriptableObject) ||
                   type == typeof(Behaviour) || type == typeof(Component) || type == typeof(Object);
        }

        /// <summary>
        /// A method decorated with <see cref="InspectorButtonAttribute"/> and the values of its parameter fields.
        /// Parameter values are shared by all objects of the same type and kept for the editor session.
        /// </summary>
        private sealed class ButtonMethod
        {
            public ButtonMethod(MethodInfo method, InspectorButtonAttribute attribute)
            {
                Method = method;
                Attribute = attribute;
                Parameters = method.GetParameters();
                Values = new object[Parameters.Length];
                Label = string.IsNullOrEmpty(attribute.Label) ? ObjectNames.NicifyVariableName(method.Name) : attribute.Label;
                Tooltip = $"Invokes {method.DeclaringType?.Name}.{method.Name}()";

                for (int i = 0; i < Parameters.Length; i++)
                {
                    ParameterInfo parameter = Parameters[i];
                    if (parameter.IsOut || parameter.ParameterType.IsByRef)
                    {
                        UnsupportedReason = $"Parameter '{parameter.Name}' is passed by reference, which is not supported.";
                        break;
                    }

                    if (!ParameterField.IsSupported(parameter.ParameterType))
                    {
                        UnsupportedReason = $"Parameter '{parameter.Name}' has the unsupported type {parameter.ParameterType.Name}.";
                        break;
                    }

                    Values[i] = ParameterField.GetInitialValue(parameter);
                }
            }

            public MethodInfo Method { get; }
            public InspectorButtonAttribute Attribute { get; }
            public ParameterInfo[] Parameters { get; }
            public object[] Values { get; }
            public string Label { get; }
            public string Tooltip { get; }
            public string UnsupportedReason { get; }
        }
    }
}
