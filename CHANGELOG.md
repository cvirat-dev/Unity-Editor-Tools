# Changelog

All notable changes to this package are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-10-31

### Added

- `[InspectorButton]` attribute with parameter fields, return value logging, coroutine support, Undo and
  `ButtonMode` (always / Edit Mode only / Play Mode only). Works on every MonoBehaviour and ScriptableObject without
  requiring a special base class.
- `InspectorButtonGUI.DrawButtons` to draw the buttons from custom editors.
- `[ShowIf]` attribute supporting serialized siblings (also in nested classes) and non-serialized fields, properties
  and methods, with optional compare values.
- `[ReadOnlyInspector]` attribute.
- `[Title]` decorator with subtitle, alignment and separator line.
- `[Note]` attribute to display string fields as editable message boxes.
- `Comment` component, stripped from scenes during player builds.
- `Readme` asset with a formatted Inspector view and an Edit toggle.
- Active state toggles in the Hierarchy window.
- Preferences page under **Preferences > Editor Tools**.
- "Attributes Showcase" sample.
