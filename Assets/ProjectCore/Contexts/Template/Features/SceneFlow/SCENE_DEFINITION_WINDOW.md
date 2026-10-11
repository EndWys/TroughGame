# Scene Definition Window

Open `Tools > Scene Flow > Scene Definition Window`.

The window creates or registers a concrete `BaseSceneDefinition`, keeps it
beside its Unity scene, adds the scene to Build Settings, synchronizes its build
index, and adds the definition to `SceneCatalogConfig`.

## Prerequisites

- A `SceneCatalogConfig` asset.
- A concrete `SceneDefinition<TScene, TSettings>` C# type.
- An existing scene, or an existing context-owned `Assets/Scenes` folder for
  a new scene. Scene locations and names follow
  [Unity Resource Naming](../../../../../../docs/CONVENTIONS.md#unity-resource-naming).

## Existing Scene

1. Select `Scene Catalog` and `Scene`.
2. Select an existing definition asset, or select `Definition Type` to create
   one.
3. Select **Create And Register Definition** or **Register Existing Definition**.

The definition is moved beside the selected scene when necessary. A definition
cannot be registered when another catalog entry already uses the same `TScene`.

## New Scene

1. Select `Scene Catalog`.
2. Choose an existing `Scene Directory` under
   `Assets/ProjectCore/Contexts/<Context>/Assets/Scenes` or
   `Assets/ProjectCore/Contexts/<Context>/Features/<Feature>/Assets/Scenes`.
   Nested scene folders are allowed. The window has no default context.
   Enter `Scene_<Context>[_<Detail>]` as the `Scene File Name`.
3. Select `Definition Type`.
4. Select **Create Scene And Definition**.

The window creates an empty scene, saves it, creates the definition asset beside
it, and registers both. A scene name that does not match the folder's context
is rejected. For a gameplay scene, add the composition required by the
[Scene Hierarchy Standard](../../../../../../docs/SCENE_HIERARCHY_STANDARD.md)
before using it at runtime.
