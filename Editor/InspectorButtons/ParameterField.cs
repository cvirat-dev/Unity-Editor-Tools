using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UUP.EditorTools.Editor
{
    /// <summary>
    /// Draws editable IMGUI fields for method parameters of common Unity types.
    /// </summary>
    internal static class ParameterField
    {
        /// <summary>Returns true if a field can be drawn for <paramref name="type"/>.</summary>
        public static bool IsSupported(Type type)
        {
            return type == typeof(int) || type == typeof(long) ||
                   type == typeof(float) || type == typeof(double) ||
                   type == typeof(bool) || type == typeof(string) ||
                   type.IsEnum ||
                   type == typeof(Color) ||
                   type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
                   type == typeof(Vector2Int) || type == typeof(Vector3Int) ||
                   type == typeof(Quaternion) ||
                   type == typeof(Rect) || type == typeof(RectInt) ||
                   type == typeof(Bounds) || type == typeof(BoundsInt) ||
                   type == typeof(AnimationCurve) || type == typeof(Gradient) ||
                   typeof(Object).IsAssignableFrom(type);
        }

        /// <summary>
        /// Returns the parameter's declared default value if it has one, otherwise the default value of its type.
        /// </summary>
        public static object GetInitialValue(ParameterInfo parameter)
        {
            Type type = parameter.ParameterType;

            if (parameter.HasDefaultValue && parameter.DefaultValue != null)
            {
                // Enum defaults are stored as their underlying integer value.
                return type.IsEnum ? Enum.ToObject(type, parameter.DefaultValue) : parameter.DefaultValue;
            }

            if (type == typeof(string))
                return string.Empty;
            if (type == typeof(Quaternion))
                return Quaternion.identity;
            if (type == typeof(AnimationCurve))
                return AnimationCurve.Linear(0f, 0f, 1f, 1f);
            if (type == typeof(Gradient))
                return new Gradient();
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        /// <summary>Draws a field for <paramref name="value"/> and returns the edited value.</summary>
        public static object Draw(string label, Type type, object value)
        {
            if (type == typeof(int)) return EditorGUILayout.IntField(label, (int)value);
            if (type == typeof(long)) return EditorGUILayout.LongField(label, (long)value);
            if (type == typeof(float)) return EditorGUILayout.FloatField(label, (float)value);
            if (type == typeof(double)) return EditorGUILayout.DoubleField(label, (double)value);
            if (type == typeof(bool)) return EditorGUILayout.Toggle(label, (bool)value);
            if (type == typeof(string)) return EditorGUILayout.TextField(label, (string)value);

            if (type.IsEnum)
            {
                return type.IsDefined(typeof(FlagsAttribute), false)
                    ? EditorGUILayout.EnumFlagsField(label, (Enum)value)
                    : EditorGUILayout.EnumPopup(label, (Enum)value);
            }

            if (type == typeof(Color)) return EditorGUILayout.ColorField(label, (Color)value);
            if (type == typeof(Vector2)) return EditorGUILayout.Vector2Field(label, (Vector2)value);
            if (type == typeof(Vector3)) return EditorGUILayout.Vector3Field(label, (Vector3)value);
            if (type == typeof(Vector4)) return EditorGUILayout.Vector4Field(label, (Vector4)value);
            if (type == typeof(Vector2Int)) return EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
            if (type == typeof(Vector3Int)) return EditorGUILayout.Vector3IntField(label, (Vector3Int)value);

            // Quaternions are edited as Euler angles, like the Transform inspector does.
            if (type == typeof(Quaternion))
                return Quaternion.Euler(EditorGUILayout.Vector3Field(label, ((Quaternion)value).eulerAngles));

            if (type == typeof(Rect)) return EditorGUILayout.RectField(label, (Rect)value);
            if (type == typeof(RectInt)) return EditorGUILayout.RectIntField(label, (RectInt)value);
            if (type == typeof(Bounds)) return EditorGUILayout.BoundsField(label, (Bounds)value);
            if (type == typeof(BoundsInt)) return EditorGUILayout.BoundsIntField(label, (BoundsInt)value);
            if (type == typeof(AnimationCurve)) return EditorGUILayout.CurveField(label, (AnimationCurve)value ?? new AnimationCurve());
            if (type == typeof(Gradient)) return EditorGUILayout.GradientField(label, (Gradient)value ?? new Gradient());

            if (typeof(Object).IsAssignableFrom(type))
                return EditorGUILayout.ObjectField(label, (Object)value, type, true);

            EditorGUILayout.LabelField(label, $"Unsupported type {type.Name}");
            return value;
        }
    }
}
