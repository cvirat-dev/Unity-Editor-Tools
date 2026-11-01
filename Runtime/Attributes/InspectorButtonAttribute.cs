using System;

namespace UUP.EditorTools
{
    /// <summary>
    /// Controls when a button created with <see cref="InspectorButtonAttribute"/> can be clicked.
    /// </summary>
    public enum ButtonMode
    {
        /// <summary>The button can be clicked in Edit Mode and in Play Mode.</summary>
        Always,

        /// <summary>The button can only be clicked in Edit Mode.</summary>
        EditModeOnly,

        /// <summary>The button can only be clicked in Play Mode.</summary>
        PlayModeOnly
    }

    /// <summary>
    /// Draws a button at the bottom of the Inspector that invokes the decorated method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Works out of the box on every <c>MonoBehaviour</c> and <c>ScriptableObject</c> that does not have its own
    /// custom editor. If your class has a custom editor, call
    /// <c>UUP.EditorTools.Editor.InspectorButtonGUI.DrawButtons(targets)</c> at the end of its
    /// <c>OnInspectorGUI</c>.
    /// </para>
    /// <para>
    /// The method can be public or private, static or instance. Parameters of common Unity types (numbers, strings,
    /// bools, enums, vectors, colors, curves, object references, ...) are shown as editable fields above the button
    /// and use the parameter's default value initially. Return values are logged to the Console, and methods
    /// returning <c>IEnumerator</c> are started as coroutines in Play Mode.
    /// </para>
    /// <para>In Edit Mode, the call is recorded with Undo and the target is marked dirty.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [InspectorButton]
    /// private void Respawn() { ... }
    ///
    /// [InspectorButton("Deal Damage", ButtonMode.PlayModeOnly)]
    /// public void TakeDamage(int amount = 10) { ... }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InspectorButtonAttribute : Attribute
    {
        /// <summary>
        /// Creates an inspector button.
        /// </summary>
        /// <param name="label">Text shown on the button. If null or empty, the nicified method name is used.</param>
        /// <param name="mode">When the button can be clicked.</param>
        public InspectorButtonAttribute(string label = null, ButtonMode mode = ButtonMode.Always)
        {
            Label = label;
            Mode = mode;
        }

        /// <summary>
        /// Creates an inspector button labelled with the method name.
        /// </summary>
        /// <param name="mode">When the button can be clicked.</param>
        public InspectorButtonAttribute(ButtonMode mode) : this(null, mode)
        {
        }

        /// <summary>Text shown on the button. If null or empty, the nicified method name is used.</summary>
        public string Label { get; }

        /// <summary>When the button can be clicked.</summary>
        public ButtonMode Mode { get; }
    }
}
