# Editor Tools for Unity

Lightweight Inspector and Hierarchy helpers that remove everyday friction in the Unity Editor.
No dependencies, no base classes to inherit from: add the package and start using the attributes.

| Feature | What it does |
| --- | --- |
| [`[InspectorButton]`](#inspectorbutton) | Adds a button to the Inspector that calls a method, with editable parameters |
| [`[ShowIf]`](#showif) | Shows a field only while another field has a given value |
| [`[ReadOnlyInspector]`](#readonlyinspector) | Displays a field without allowing edits |
| [`[Title]`](#title) | A header with subtitle and separator line |
| [`[Note]`](#note) | Turns a string field into an editable note box |
| [Comment component](#comment-component) | Documents GameObjects in the Inspector; stripped from builds |
| [Readme asset](#readme-asset) | Formatted documentation assets for your project folders |
| [Hierarchy active toggles](#hierarchy-active-toggles) | Activate or deactivate GameObjects directly from the Hierarchy |

## Requirements

- Unity 6 (6000.0) or newer

## Installation

In Unity, open **Window > Package Manager**, click **+ > Add package from git URL...** and enter:

```text
https://github.com/cvirat-dev/unity-editor-tools.git
```

To install a specific version, append a tag, for example `#v0.1.0`.
You can also add the dependency to `Packages/manifest.json` directly:

```json
"com.uup.editor-tools": "https://github.com/cvirat-dev/unity-editor-tools.git"
```

All runtime types live in the `UUP.EditorTools` namespace:

```csharp
using UUP.EditorTools;
```

A sample component that uses every attribute can be imported from the Package Manager (**Samples > Attributes Showcase**).

---

## `[InspectorButton]`

Draws a button at the bottom of the Inspector that invokes the decorated method.

```csharp
public class Enemy : MonoBehaviour
{
    [InspectorButton]
    private void Respawn() { /* ... */ }

    // Custom label, only clickable in Play Mode, with an editable parameter.
    [InspectorButton("Deal Damage", ButtonMode.PlayModeOnly)]
    public void TakeDamage(int amount = 10) { /* ... */ }
}
```

- Works on any `MonoBehaviour` or `ScriptableObject`, public or private, static or instance methods.
- **Parameters** of common types are shown as fields above the button: `int`, `long`, `float`, `double`, `bool`,
  `string`, enums, `Color`, `Vector2/3/4`, `Vector2Int/3Int`, `Quaternion` (edited as Euler angles), `Rect`,
  `Bounds` (and their `Int` variants), `AnimationCurve`, `Gradient`, and any `UnityEngine.Object` reference.
  Default parameter values are used as initial values.
- **Return values** are logged to the Console. Methods returning `IEnumerator` are started as coroutines in Play Mode.
- **Multi-object editing**: the method is invoked on every selected object.
- **Undo**: in Edit Mode the call is recorded with Undo and the object is marked dirty.
- `ButtonMode` restricts the button to `EditModeOnly` or `PlayModeOnly`.

### Classes that have their own custom editor

The buttons are drawn by a *fallback* editor, which Unity only uses when no other custom editor exists for the class.
This means the package never overrides your editors or those of other packages. If your class has a custom editor,
draw the buttons yourself:

```csharp
using UUP.EditorTools.Editor;

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        InspectorButtonGUI.DrawButtons(targets);
    }
}
```

## `[ShowIf]`

Shows a field only while another member has a given value.

```csharp
[SerializeField] private bool useCustomColor;
[SerializeField, ShowIf(nameof(useCustomColor))] private Color customColor;

[SerializeField] private Mode mode;
[SerializeField, ShowIf(nameof(mode), Mode.Advanced)] private float advancedSetting;

[SerializeField] private GameObject prefab;
[SerializeField, ShowIf(nameof(prefab))] private int poolSize; // shown while prefab is assigned
```

The condition can be:

- a **serialized field** declared next to the decorated field (`bool`, enum, `int`, `float`, `string` or object
  reference). This works inside nested serializable classes too;
- a **field, property or parameterless method** of the inspected object, even if it is not serialized
  (for example `private bool IsAdvanced => mode == Mode.Advanced;`).

Limitations (inherent to Unity property drawers):

- On arrays and lists, the attribute applies to each element, not to the collection. Wrap the collection in a
  serializable class to hide it as a whole.
- Only one property drawer runs per field, so `[ShowIf]` cannot be combined with `[ReadOnlyInspector]` or other
  drawer attributes. Decorators such as `[Header]` and `[Title]` are fine, but they stay visible.

## `[ReadOnlyInspector]`

Displays a field in the Inspector without allowing edits. Useful to expose runtime state while debugging.

```csharp
[SerializeField, ReadOnlyInspector] private int currentHealth;
```

## `[Title]`

A richer `[Header]`: title, optional subtitle, alignment and separator line.

```csharp
[Title("Movement", "Speeds are in meters per second")]
[SerializeField] private float walkSpeed = 4f;

[Title("Debug", alignment: TextAnchor.MiddleCenter, drawLine: false)]
[SerializeField] private bool drawGizmos;
```

## `[Note]`

Displays a `string` field as a message box with an optional icon (`None`, `Info`, `Warning`, `Error`).
Double-click the note, or right-click it and choose **Edit Note**, to change the text, then click **Done**.

```csharp
[SerializeField, Note(NoteIcon.Warning)]
private string setupNote = "Assign the spawn points before entering Play Mode.";
```

The text is regular serialized data and is included in builds. To document GameObjects at zero runtime cost, use the
Comment component instead.

Because a private note field is never read by your code, the compiler reports warning CS0414. Surround note fields
with `#pragma warning disable CS0414` / `#pragma warning restore CS0414` to silence it.

## Comment component

**Add Component > Editor Tools > Comment** attaches a note to a GameObject, shown as a message box in the Inspector.

- Double-click the comment (or right-click > **Edit Comment**) to edit it. Press **Ctrl/Cmd + Enter** or click
  **Done** to finish.
- Pick an icon to highlight information, warnings or errors.
- **Comments are removed from scenes when building a player**, so they cost nothing at runtime. The removed comments
  are listed in the Console (can be disabled in the preferences). Your scene files are not modified.

**Comment component** inspired by chrisyarbrough/comment-component (Unlicense)

## Readme asset

**Assets > Create > Editor Tools > Readme** creates an asset that renders formatted documentation in the Inspector:
an icon, a title and sections with a heading, rich text and an optional link. Click **Edit** in the top-right corner of
the Inspector to change its content. Drop one into a folder to explain its content to your team.

## Hierarchy active toggles

A checkbox is drawn on the right of each GameObject in the Hierarchy window to activate or deactivate it without
selecting it. Toggling a selected GameObject applies to the whole selection, and changes support Undo.

## Preferences

**Edit > Preferences > Editor Tools** (**Unity > Settings > Editor Tools** on macOS):

- **Show active toggles**: enables the Hierarchy checkboxes.
- **Log comments removed from builds**: lists stripped Comment components in the Console after a build.

## Development

The package ships with edit-mode tests, but they are dormant by default: the test assembly is guarded by the
`UNITY_INCLUDE_TESTS` define constraint, so it never compiles in projects that simply consume the package.

To run the tests, open the package in a Unity project (or add it to one via **Add package from disk...**) and opt the
package into test discovery by adding it to `testables` in the project's `Packages/manifest.json`:

```json
"testables": [
  "com.uup.editor-tools"
]
```

After Unity recompiles, the tests appear in **Window > Test Runner > EditMode**. See the Unity manual for details:
[Adding tests to a package](https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/cus-pkg-development/cus-tests).

## License

[MIT](LICENSE)
