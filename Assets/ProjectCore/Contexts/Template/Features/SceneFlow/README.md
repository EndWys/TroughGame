# Scene Flow

`SceneFlowFeature` is persistent Template infrastructure in `ProjectContext`.
It loads gameplay scenes, supplies immutable scene settings through DI, and
executes the scene lifecycle.

## Scene definition

Each gameplay scene has one concrete definition asset beside its `.unity` file.
The definition inherits `SceneDefinition<TScene, TSettings>` and declares the
scene marker and settings type.

```csharp
public sealed class DungeonSceneDefinition
    : SceneDefinition<DungeonScene, DungeonSceneSettings>
{
}
```

The asset is registered in `SceneCatalogConfig`. It stores its `SceneAsset` and
the synchronized enabled Build Settings index.

## Scene settings

`ISceneSettings` represents immutable settings supplied when entering a scene.
They are registered in the new `SceneContext` by their concrete type and remain
available until that scene is unloaded.

```csharp
public sealed class DungeonSceneSettings : ISceneSettings
{
    public DungeonSceneSettings(string dungeonId)
    {
        DungeonId = dungeonId;
    }

    public string DungeonId { get; }
}
```

Services in the loaded scene receive the same settings through constructor DI.

```csharp
public DungeonBootstrapService(DungeonSceneSettings settings)
{
}
```

## Loading a scene

Inject `ISceneFlowService` and use the typed API.

```csharp
Result result = await _sceneFlowService.LoadAsync<DungeonScene, DungeonSceneSettings>(
    new DungeonSceneSettings(dungeonId),
    cancellationToken);
```

Check `Result` before continuing. Scene names, paths, and build indexes are not
passed by consumers.

## Lifecycle

1. Optional `ISceneTransitionPresenter` implementations show transition UI.
2. The current `IGameSceneLifecycle.ExitAsync` completes.
3. The previous scene lifetime token is cancelled.
4. Unity loads the next scene and its settings are bound into its DI container.
5. The next `IGameSceneLifecycle.InitializeAsync` completes.
6. Transition UI is hidden.

`IGameSceneLifecycle` is implemented by the scene context initializer. Its
`ExitAsync` runs before Unity unloads the previous scene.
