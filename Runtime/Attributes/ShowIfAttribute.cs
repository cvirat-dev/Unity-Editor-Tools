using System;
using UnityEngine;

namespace UUP.EditorTools
{
    /// <summary>
    /// Shows the decorated field in the Inspector only while another member has a given value.
    /// </summary>
    /// <remarks>
    /// <para>The condition member can be:</para>
    /// <list type="bullet">
    /// <item>a serialized field declared next to the decorated field (bool, enum, int, float, string or object
    /// reference). This also works inside nested serializable classes and structs.</item>
    /// <item>a field, property or parameterless method of the inspected object itself, even if it is not serialized.</item>
    /// </list>
    /// <para>
    /// When no compare value is given, the field is shown while the condition is <c>true</c>
    /// (for object references: while the reference is assigned).
    /// </para>
    /// <para>
    /// Limitations: Unity applies property drawers to each element of an array or list rather than to the collection
    /// itself, and only one property drawer runs per field, so <c>[ShowIf]</c> cannot be combined with another drawer
    /// attribute such as <see cref="ReadOnlyInspectorAttribute"/>. Decorators like <c>[Header]</c> or
    /// <see cref="TitleAttribute"/> are still drawn while the field is hidden.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [SerializeField] private bool useCustomColor;
    /// [SerializeField, ShowIf(nameof(useCustomColor))] private Color customColor;
    ///
    /// [SerializeField] private Mode mode;
    /// [SerializeField, ShowIf(nameof(mode), Mode.Advanced)] private float advancedSetting;
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        /// <summary>
        /// Shows the field while <paramref name="conditionMember"/> is <c>true</c> (or assigned, for object references).
        /// </summary>
        /// <param name="conditionMember">Name of the member to evaluate. Use <c>nameof(...)</c> to stay refactoring-safe.</param>
        public ShowIfAttribute(string conditionMember) : this(conditionMember, true)
        {
        }

        /// <summary>
        /// Shows the field while <paramref name="conditionMember"/> equals <paramref name="compareValue"/>.
        /// </summary>
        /// <param name="conditionMember">Name of the member to evaluate. Use <c>nameof(...)</c> to stay refactoring-safe.</param>
        /// <param name="compareValue">The value the member must have for the field to be shown (bool, enum, number or string).</param>
        public ShowIfAttribute(string conditionMember, object compareValue)
        {
            ConditionMember = conditionMember;
            CompareValue = compareValue;
        }

        /// <summary>Name of the member that controls the visibility.</summary>
        public string ConditionMember { get; }

        /// <summary>The value the condition member must have for the field to be shown.</summary>
        public object CompareValue { get; }
    }
}
