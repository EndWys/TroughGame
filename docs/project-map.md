# TroughGame: integration map

This is a discovery index, verified on 2026-10-12. Inspect current code before
designing a change; this index does not approve behavior or freeze feature order.
All paths below `Assets/ProjectCore/Contexts/` use the flat namespace
`ProjectCore.<Context>` regardless of feature folder depth.

| Owner | Entry / composition / relevant capabilities |
| --- | --- |
| Template | Reusable feature framework in `Scripts/Initialization/`; `BaseFeature`, `BaseMonoBehaviourFeature`, `BaseFeatureInstaller`, `FeatureInitializationFlow`, `FeatureLifecycleAdapter` |
| Project | `Scripts/Init/ProjectContextInstaller.cs`, `ProjectContextInitializer.cs`; persistent Logging, CommandLine, DebugTools, LocalConfig, AppTime, ObjectFactory, LoadingScreen, SceneLoadingScreenBridge, LocalSave, SceneFlow |
| Preloader | `Scripts/Init/PreloaderContextInstaller.cs`, `PreloaderContextInitializer.cs`; `Features/ApplicationInitialization/Scripts/Init/ApplicationEntryPoint.cs`; `Assets/Scenes/Scene_Preloader.unity` |
| GameCore | `Scripts/Init/GameCoreFeatureGroup.cs`; NetworkEntities, CameraTarget, MovementSimulation, Movement, Combat; Input is registered by the gameplay installer |
| TechnicalPrototype | `Scripts/Init/TechnicalPrototypeContextInstaller.cs`, `TechnicalPrototypeContextInitializer.cs`; Player, Enemy, LocalCamera; `Assets/Scenes/Scene_TechnicalPrototype.unity` |
| Prototype | Legacy experiments, excluded from the supported runtime scene flow; not the default integration target |
| Shared | `Assets/ProjectCore/Shared/`; .NET-only primitives, separate from Unity and other external APIs |

The persistent Zenject prefab is
`Assets/ProjectCore/Resources/ProjectContext.prefab`; preserve its location and
GUID. Persistent catalog instances are in `Contexts/Project/Assets/Catalogs`.
Feature-owned resources live in the owning feature's `Assets/` directory.

Template also owns reusable Popup, ScreenNavigation, DebugConsole,
DebugVisualization, Cheat, and other documented capabilities. Refer to
[the documentation index](README.md) and the relevant feature README before
adding a competing implementation. `ObjectFactoryFeature` installs the
context-local `IClassFactory`; runtime construction with injected dependencies
uses that context's factory.

Runtime UI work uses [unity-ui-toolkit](../.agents/skills/unity-ui-toolkit/SKILL.md)
and [the runtime UI standard](UI_TOOLKIT_STANDARD.md). Inspect Template
ScreenNavigation/Popup READMEs, typed definitions, scene catalogs and their
UIDocument hosts before adding a screen/popup. MVVM is the default; justified MVP
uses the same architecture approval. Feature-owned per-view ViewModels/Presenters
use `Scripts/Other/ViewModels` or `Scripts/Other/Presenters` respectively;
resource reuse searches actual owning-feature
and root common UI assets under `Assets/ProjectCore/Assets` according to
[the common resource layout](CONVENTIONS.md#common-ui-resource-layout).
Concrete-context UI resources stay private to that context; Template remains
reusable. This specialization reuses the current feature/bug host.

Startup path:

```text
ProjectContextInstaller registers persistent features
  -> PreloaderContextInstaller registers startup features / ApplicationEntryPoint
  -> ApplicationEntryPoint.Start
  -> ApplicationInitializationFlow
  -> IProjectContextInitializer -> FeatureInitializationFlow
  -> IPreloaderContextInitializer -> FeatureInitializationFlow
  -> ISceneFlowService
  -> gameplay IGameSceneLifecycle -> context-local FeatureInitializationFlow
```

Tests use existing `Template/Tests/Editor/ProjectCore.Tests.Editor.asmdef` and
`Template/Tests/Play/ProjectCore.Tests.Play.asmdef`. GameCore and
TechnicalPrototype Editor tests reference the Editor assembly through `.asmref`.
Place new fixtures in the owning context's `Tests/Editor` or `Tests/Play`;
do not introduce an assembly definition. Startup tests use Preloader.

For a new feature, inspect its actual contracts, callers, installer, serialized
references, cancellation owner, and tests. Use `rg` for code discovery and Unity
CLI live commands for scene/asset inspection and modification.
