# Local Config

`LocalConfig` provides strongly typed access to configuration shipped as
`ScriptableObject` assets. It is reusable Template Infrastructure and is
installed separately in every DI context that owns local configuration.

## Responsibilities

| Type | Responsibility |
| --- | --- |
| `ILocalConfig` | Marker contract implemented by local config types. |
| `BaseLocalConfig` | Base type for every concrete local config asset. |
| `LocalConfigCatalogConfig` | Flat authored list of configs owned by one DI context. |
| `LocalConfigFeature` | Owns the serialized catalog reference and initializes the context service. |
| `LocalConfigService` | Indexes the local catalog and delegates missing lookups to a parent context. |
| `ILocalConfigService` | Read-only API injected into consumers. |
| `LocalConfigCatalogValidation` | Rejects null entries and duplicate concrete types. |

The catalog is private feature data. It is not bound into Zenject and cannot be
resolved by arbitrary consumers.

## Lookup model

Each context has its own `LocalConfigService` and one flat catalog:

```text
Gameplay LocalConfigService
  -> gameplay catalog
  -> parent ILocalConfigService
       -> Project catalog
       -> not found
```

Rules:

- lookup uses the exact concrete config type;
- the current context is checked before any parent context;
- a child config overrides a parent config of the same type;
- duplicate types inside one catalog are invalid.

Related values may be grouped inside a concrete aggregate config. For example,
`EnemiesConfig` can own a serialized dictionary keyed by `EnemyTypes` and
expose `GetConfig(EnemyTypes enemyType)`.

## Creating a config

Place the class in the owning Feature under `Scripts/DataHolders/Configs` and
the asset under that Feature's `DataResources`.

```csharp
using UnityEngine;

namespace ProjectCore.GameCore
{
    [CreateAssetMenu(
        fileName = "Config_GameCore_Enemies_Balance",
        menuName = "SO/GameCore/Enemies/Balance")]
    public sealed class EnemiesConfig : BaseLocalConfig
    {
        [field: SerializeField] public float DefaultHealth { get; private set; }
        [field: SerializeField] public float DefaultSpeed { get; private set; }
    }
}
```

Concrete configs must be `sealed`. Runtime systems must treat authored config
assets as read-only and keep mutable runtime state elsewhere.

## Creating a catalog

Create a `LocalConfigCatalogConfig` through:

```text
Create -> SO -> Template -> LocalConfig -> Catalog
```

Rename the asset according to its owner:

```text
Config_<Context>_LocalConfig_Catalog
```

Add every config required by that context to its `Configs` list. Do not add
null entries or repeat the same concrete type. Reusing a type in a child
catalog is allowed because that represents an explicit context override.

The persistent Project catalog currently lives at:

```text
Assets/ProjectCore/Contexts/Template/Features/Infrastructure/LocalConfig/DataResources/Config_Project_LocalConfig_Catalog.asset
```

## Installing in a context

1. Add `LocalConfigFeature` to the same GameObject as the context installer.
2. Assign the context's catalog to `_localConfigCatalog` in the Inspector.
3. Register it in `AddFeatures()` before every feature that reads configs.

```csharp
protected override void AddFeatures()
{
    AddFeatureFromComponent<LocalConfigFeature>();
    AddFeature<GameplayFeature>();
}
```

During the unified initialization flow, `LocalConfigFeature` passes its catalog
directly to `LocalConfigService.Initialize()`. The catalog does not need a
separate binding in the context installer.

Installing the feature only in Project provides persistent configs. Install it
again in a scene context only when that scene needs local configs or overrides.
The nearest parent context containing `ILocalConfigService` is used as fallback.

## Reading configs

Inject only the public read-only contract:

```csharp
public sealed class EnemyService
{
    private readonly ILocalConfigService _localConfigService;

    public EnemyService(ILocalConfigService localConfigService)
    {
        _localConfigService = localConfigService;
    }

    public void Initialize()
    {
        EnemiesConfig config =
            _localConfigService.GetRequiredConfig<EnemiesConfig>();
    }
}
```

Use `GetRequiredConfig<TConfig>()` when absence is a setup error. It throws an
`InvalidOperationException` if the type is missing in the complete context
chain.

Use `TryGetConfig<TConfig>()` only for genuinely optional configuration:

```csharp
if (_localConfigService.TryGetConfig(out DebugConfig config))
{
    // Apply optional debug settings.
}
```

Features that consume configs must be initialized after `LocalConfigFeature`.
Do not resolve services or read configs during `InstallBindings()`.

## Lifetime and resources

`LocalConfigService` retains its own catalog for the complete lifetime of its
DI context:

- Project configs live until application shutdown;
- scene configs lose their context-owned references when the scene container
  is destroyed;
- unused scene assets only become eligible for unloading; immediate unloading
  is not guaranteed by Unity;
- a persistent object that caches a scene config keeps that config and its
  direct references alive.

Keep persistent configs lightweight. Prefer IDs or a dedicated lazy resource
loading abstraction for large prefabs, textures, audio, or animation assets.
Local Config does not load resources and does not own load handles.

## Change checklist

When adding a local config:

1. Create a sealed class derived from `BaseLocalConfig`.
2. Place the script and asset in the owning Feature.
3. Add the asset to the appropriate context catalog.
4. Inject `ILocalConfigService` into the consuming service or system.
5. Read it with `GetRequiredConfig<TConfig>()` or `TryGetConfig<TConfig>()`.
6. Confirm that `LocalConfigFeature` initializes before the consuming Feature.
7. Run the strict architecture validator and relevant tests.
