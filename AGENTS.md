# Repository Guidelines

## Project Structure & Module Organization

This is a Unity 6000.6.7f1 2D boxing/card prototype using URP, Splines, TextMesh Pro, and DOTween.

- `Assets/Scripts/`: C# gameplay and presentation. `Card` holds runtime state, `CardData` defines ScriptableObject data, and `CardView`/`HandView` handle display and layout.
- `Assets/Scenes/MainScene.unity`: main development scene and enabled build scene.
- `Assets/Prefab/`: reusable card prefabs; `Assets/Data/Card Data/`: card definitions.
- `Assets/V1.02/` and `Assets/BoldPixels/`: artwork and fonts; `Assets/Plugins/`: bundled dependencies.
- `Assets/Settings/`, `Packages/`, and `ProjectSettings/`: rendering, dependencies, and project configuration.

## Build, Test, and Development Commands

Use the editor version recorded in `ProjectSettings/ProjectVersion.txt`. Set `UNITY_EDITOR` to its executable path, then run commands from the repository root:

```bash
git lfs pull
"$UNITY_EDITOR" -projectPath "$PWD"
```

These retrieve LFS-managed assets and open the project. Open `MainScene` and enter Play mode; press Space to add cards and hover over them to check previews.

Create player builds through **File > Build Profiles**, verifying that `MainScene` is included. No scripted build entry point is currently provided.

## Coding Style & Naming Conventions

Follow existing C# style: four-space indentation, braces on separate lines, PascalCase types/methods/properties, and camelCase fields/locals. Match filenames to their primary class. Use `[SerializeField] private` for Inspector references and keep card data separate from view behavior. No repository-wide formatter or linter configuration is present; avoid unrelated formatting changes.

## Testing Guidelines

Unity Test Framework 1.8.0 is installed; no automated test suite or coverage threshold is configured. `TestSystem.cs` is a manual Play-mode driver.

Add future tests under `Assets/Tests/EditMode/` or `Assets/Tests/PlayMode/` with appropriate test assembly definitions. Use names such as `CardTests.cs` and descriptive behavior-based methods. Run through **Window > General > Test Runner**, or:

```bash
"$UNITY_EDITOR" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testResults /tmp/boxing-tests.xml -logFile /tmp/boxing-tests.log
```

Use `PlayMode` for scene-dependent tests. Manually check card creation, spacing, animations, and hover restoration after UI changes.

## Commit & Pull Request Guidelines

History uses short, descriptive subjects such as `Card hover system`; continue that pattern. Keep commits focused. PRs should describe behavior changes, validation, related issues when applicable, and screenshots or clips for visual changes.

Commit asset `.meta` files together with their assets; preserve GUIDs when moving files through Unity. Follow `.gitattributes` for Git LFS. Exclude generated `Library/`, `Temp/`, `Logs/`, and IDE project files.
