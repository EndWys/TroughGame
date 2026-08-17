# Scene Definition Window

Open `Tools > Scene Flow > Scene Definition Window`.

The window creates or registers a concrete `BaseSceneDefinition`, keeps it
beside its Unity scene, adds the scene to Build Settings, synchronizes its build
index, and adds the definition to `SceneCatalogConfig`.

## Prerequisites

- A `SceneCatalogConfig` asset.
- A concrete `SceneDefinition<TScene, TSettings>` C# type.
- An existing scene, or an existing folder below `Assets` for a new scene.

## Existing Scene

1. Select `Scene Catalog` and `Scene`.
2. Select an existing definition asset, or select `Definition Type` to create
   one.
3. Select **Create And Register Definition** or **Register Existing Definition**.

The definition is moved beside the selected scene when necessary. A definition
cannot be registered when another catalog entry already uses the same `TScene`.

## New Scene

1. Select `Scene Catalog`.
2. Enter an existing `Scene Directory` below `Assets` and a `Scene File Name`.
3. Select `Definition Type`.
4. Select **Create Scene And Definition**.

The window creates an empty scene, saves it, creates the definition asset beside
it, and registers both. Add the required `SceneContext` and context installer
before using the new scene at runtime.
