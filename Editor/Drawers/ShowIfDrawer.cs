using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UUP.EditorTools.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal sealed class ShowIfDrawer : PropertyDrawer
    {
        private const BindingFlags AnyMember =
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private static readonly HashSet<string> s_ReportedErrors = new HashSet<string>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return IsVisible(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
                EditorGUI.PropertyField(position, property, label, true);
        }

        private bool IsVisible(SerializedProperty property)
        {
            var showIf = (ShowIfAttribute)attribute;

            bool ok;
            bool visible;
            string error;
            try
            {
                ok = TryEvaluate(property, showIf.ConditionMember, showIf.CompareValue, out visible, out error);
            }
            catch (Exception e)
            {
                // User getters/methods and value conversions can throw; don't let that break the Inspector layout.
                Exception inner = e is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e;
                ok = false;
                visible = true;
                error = $"[ShowIf] Evaluating '{showIf.ConditionMember}' for '{property.propertyPath}' threw {inner.GetType().Name}: {inner.Message}";
            }

            if (ok)
                return visible;

            // Report each misconfiguration once instead of on every repaint, and keep the field visible.
            if (s_ReportedErrors.Add(error))
                Debug.LogWarning(error, property.serializedObject.targetObject);
            return true;
        }

        private static bool TryEvaluate(SerializedProperty property, string member, object expected, out bool result, out string error)
        {
            SerializedProperty sibling = FindSibling(property, member);
            if (sibling != null)
                return TryCompare(sibling, expected, out result, out error);

            Object target = property.serializedObject.targetObject;
            if (TryGetMemberValue(target, member, out object value))
            {
                result = AreEqual(value, expected);
                error = null;
                return true;
            }

            result = true;
            error = $"[ShowIf] No field, property or parameterless method named '{member}' was found on {target.GetType().Name} " +
                    $"(used by '{property.propertyPath}').";
            return false;
        }

        /// <summary>
        /// Finds the serialized property named <paramref name="member"/> that lives next to <paramref name="property"/>,
        /// so conditions also work inside nested serializable classes and on array elements.
        /// </summary>
        private static SerializedProperty FindSibling(SerializedProperty property, string member)
        {
            string path = property.propertyPath;

            // Drawers run on each element of a collection: "list.Array.data[2]" belongs to the field "list".
            int arrayIndex = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
            if (arrayIndex >= 0 && path.EndsWith("]", StringComparison.Ordinal))
                path = path.Substring(0, arrayIndex);

            int lastDot = path.LastIndexOf('.');
            string siblingPath = lastDot < 0 ? member : path.Substring(0, lastDot + 1) + member;
            return property.serializedObject.FindProperty(siblingPath);
        }

        private static bool TryCompare(SerializedProperty condition, object expected, out bool result, out string error)
        {
            error = null;
            switch (condition.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    result = condition.boolValue == (!(expected is bool flag) || flag);
                    return true;

                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Integer:
                    result = expected != null && condition.longValue == Convert.ToInt64(expected);
                    return true;

                case SerializedPropertyType.Float:
                    result = expected != null && Mathf.Approximately(condition.floatValue, Convert.ToSingle(expected));
                    return true;

                case SerializedPropertyType.String:
                    result = condition.stringValue == expected as string;
                    return true;

                case SerializedPropertyType.ObjectReference:
                    bool assigned = condition.objectReferenceValue != null;
                    result = expected is bool wantsAssigned ? assigned == wantsAssigned : condition.objectReferenceValue == expected as Object;
                    return true;

                default:
                    result = true;
                    error = $"[ShowIf] '{condition.propertyPath}' has the unsupported type {condition.propertyType}. " +
                            "Use a bool, enum, int, float, string or object reference.";
                    return false;
            }
        }

        private static bool TryGetMemberValue(object target, string member, out object value)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(member, AnyMember);
                if (field != null)
                {
                    value = field.GetValue(field.IsStatic ? null : target);
                    return true;
                }

                PropertyInfo propertyInfo = type.GetProperty(member, AnyMember);
                if (propertyInfo != null && propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
                {
                    MethodInfo getter = propertyInfo.GetGetMethod(true);
                    value = getter.Invoke(getter.IsStatic ? null : target, null);
                    return true;
                }

                MethodInfo method = type.GetMethod(member, AnyMember, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType != typeof(void))
                {
                    value = method.Invoke(method.IsStatic ? null : target, null);
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static bool AreEqual(object value, object expected)
        {
            switch (value)
            {
                case bool flag:
                    return flag == (!(expected is bool expectedFlag) || expectedFlag);
                case Object unityObject:
                    return expected is bool wantsAssigned ? (unityObject != null) == wantsAssigned : unityObject == expected as Object;
                case Enum _ when expected != null:
                    return Convert.ToInt64(value) == Convert.ToInt64(expected);
                case null:
                    return expected is bool wantsNotNull ? !wantsNotNull : expected == null;
                default:
                    return Equals(value, expected);
            }
        }
    }
}
