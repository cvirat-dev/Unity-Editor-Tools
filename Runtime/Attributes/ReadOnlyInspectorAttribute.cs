using System;
using UnityEngine;

namespace UUP.EditorTools
{
    /// <summary>
    /// Displays the decorated field in the Inspector but prevents editing it.
    /// </summary>
    /// <remarks>
    /// Handy to expose runtime state (counters, references found at runtime, IDs, ...) for debugging
    /// without allowing accidental edits. Works with any serializable type, including nested classes and arrays.
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField, ReadOnlyInspector] private int currentHealth;
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyInspectorAttribute : PropertyAttribute
    {
    }
}
