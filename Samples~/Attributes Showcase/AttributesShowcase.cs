using UnityEngine;
using UUP.EditorTools;

namespace UUP.EditorTools.Samples
{
    /// <summary>
    /// Add this component to any GameObject to try out the attributes of the Editor Tools package.
    /// </summary>
    [AddComponentMenu("Editor Tools/Samples/Attributes Showcase")]
    public class AttributesShowcase : MonoBehaviour
    {
        public enum Difficulty
        {
            Easy,
            Normal,
            Hard
        }

        // Note fields are only read by the Inspector, so the compiler reports them as unused (CS0414).
#pragma warning disable CS0414
        [SerializeField, Note]
        private string about = "This is a [Note] field. Double-click it to edit the text.";

        [Title("Spawning", "[Title] draws a header with an optional subtitle and separator line")]
        [SerializeField, Tooltip("Tick to reveal the field below.")]
        private bool useCustomPrefab;

        [SerializeField, ShowIf(nameof(useCustomPrefab))]
        private GameObject customPrefab;

        [SerializeField]
        private Difficulty difficulty = Difficulty.Normal;

        [SerializeField, ShowIf(nameof(difficulty), Difficulty.Hard), Tooltip("Only visible when Difficulty is Hard.")]
        private float hardModeDamageMultiplier = 2f;

        [Title("Runtime State")]
        [SerializeField, ReadOnlyInspector]
        private int clickCount;

        [SerializeField, Note(NoteIcon.Warning)]
        private string buttonsNote = "The buttons below are created with [InspectorButton].";
#pragma warning restore CS0414

        [InspectorButton]
        private void Increment()
        {
            clickCount++;
        }

        [InspectorButton("Add Amount")]
        private void Add(int amount = 5)
        {
            clickCount += amount;
        }

        [InspectorButton("Log Settings", ButtonMode.PlayModeOnly)]
        private string LogSettings()
        {
            // Return values are logged to the Console.
            return $"Difficulty: {difficulty}, custom prefab: {(useCustomPrefab && customPrefab != null ? customPrefab.name : "none")}, " +
                   $"multiplier: {hardModeDamageMultiplier}";
        }

        [InspectorButton]
        private void ResetCount()
        {
            clickCount = 0;
        }
    }
}
