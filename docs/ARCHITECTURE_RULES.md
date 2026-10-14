# TroughGame Architecture Rules

TroughGame is a multiplayer friends-slope game about dungeons.

## Architecture

The project follows the context- and feature-based organization of BG Games
Platform, with the following TroughGame-specific rules:

- `Template` remains a context inside `Assets/ProjectCore/Contexts`.
- `Shared` is independent of external APIs. It may use only .NET APIs.
- Project-owned namespaces are intentionally short: `Shared` or
  `ProjectCore.<Context>`.
- Runtime and test assembly definitions separate production code from
  Editor/Play tests. Architectural boundaries are additionally enforced through
  folder ownership, namespaces, dependency checks, and code review.

The structure below is the supported project structure. Legacy project-owned
paths and namespaces are not part of the supported architecture.

```text
Assets/
  ProjectCore/
    Resources/
      ProjectContext.prefab

    Assets/
      InputActions/
      Rendering/

    Contexts/
      Template/
        Features/
          <Feature>/
        Plugins/
        Scripts/
        Tests/
          Editor/
          Play/

      Project/
        Assets/
          Catalogs/
        Scripts/
          Init/

      Preloader/
        Features/
        Assets/
          Scenes/
        Scripts/
          Abstract/
          Init/
          Navigation/

      GameCore/
        Features/
          <Feature>/
        Assets/
        Scripts/
          Init/

      TechnicalPrototype/
        Features/
          <Feature>/
        Assets/
          Scenes/
        Scripts/
          Init/
          Navigation/

      Prototype/                 # legacy experiments; not part of runtime flow
        Features/
          <Feature>/
        Assets/
          Scenes/
        Scripts/
          Init/

    Shared/
      Attributes/
      Collections/
      Modifiers/
      Patterns/
      Results/
    ThirdParty/
```

### Assets

`Assets` contains project-wide designer- or game-owned data that is not
owned by one feature or scene. Examples include input actions, rendering
settings, localization tables, level catalogs, and shared balance data.

Feature-owned configs and assets stay inside `<Feature>/Assets`. Resources without
a feature owner use `Contexts/<Context>/Assets`; this does not require a new Feature.
The owning context describes the resource instance, rather than the context defining
its reusable C# type. Project's application catalogs are therefore in
`Contexts/Project/Assets/Catalogs`, while catalog types remain in Template.
The global `InputActions_Project_Default` supplies Input System project defaults;
`InputActions_TechnicalPrototype` supplies the gameplay feature's serialized input.
Both sets keep their distinct action IDs and assignments.

### Contexts

Contexts define ownership and lifetime boundaries.

| Context | Responsibility | Namespace |
| --- | --- | --- |
| `Template` | Shared initialization, DI infrastructure, bootstrap code, reusable utilities, and common plugins | `ProjectCore.Template` |
| `Project` | Zenject `ProjectContext`, application-lifetime services, and state that survives every scene change | `ProjectCore.Project` |
| `Preloader` | The only scene entry point, transient startup initialization, and navigation to the first gameplay scene | `ProjectCore.Preloader` |
| `GameCore` | Gameplay features reusable by game scenes | `ProjectCore.GameCore` |
| `TechnicalPrototype` | The active technical-prototype gameplay scene and only behavior specific to that scene | `ProjectCore.TechnicalPrototype` |
| `Prototype` | Legacy experiments retained for reference and excluded from the runtime scene flow | `ProjectCore.Prototype` |

Add a new context when code has a distinct lifetime or composition root. Do
not create a context merely to group related classes.

`TechnicalPrototype` is not a special application root. It represents the
current technical-prototype gameplay-scene context and may later be accompanied
or replaced by contexts such as `Dungeon`, `Lobby`, or `Tutorial`. Features
needed by more than one gameplay scene belong to `GameCore`; only scene
composition and scene-specific behavior belong to the gameplay context.

`Prototype` is retained only as legacy experimental source. New runtime code,
scene definitions, Build Settings, catalogs, and Preloader configuration must
not depend on it.

`Project` and `Preloader` have deliberately different lifetimes:

- `Project` is the standard Zenject `ProjectContext`. Its prefab is created
  before the first `SceneContext`, remains in `DontDestroyOnLoad`, and is
  destroyed only when the application process ends.
- `Preloader` is a normal scene context. It exists only while the startup scene
  is loaded and is destroyed after navigation to the first gameplay scene.

### Context-Owned Shared Scripts

Not every shared script requires a feature. Code may live directly under
`Assets/ProjectCore/Contexts/<Context>/Scripts/<Category>` when all of the
following are true:

- it clearly belongs to one context;
- it is shared by the context composition or by more than one feature in that
  context;
- it does not represent an independently connectable gameplay or technical
  capability;
- it has no independent feature lifecycle;
- it does not require DI registration, resolution, or initialization;
- it does not own context-level runtime state;
- it does not coordinate feature-owned services, systems, managers, factories,
  or views;
- it does not depend on internal types of a concrete feature;
- it does not own feature resources such as configs, prefabs, scenes, or
  feature-specific data assets;
- wrapping it in a feature would add structure without adding ownership or
  lifecycle meaning.

Typical context-owned scripts are shared base classes, interfaces, enums,
extensions, utilities, validation helpers, constants, passive data, and generic
MonoBehaviour components. The absence of DI alone is not sufficient: a
standalone capability with its own responsibility, state, resources, or
lifecycle is still a feature.

Common examples are:

```text
Contexts/GameCore/Scripts/Constants/
Contexts/GameCore/Scripts/Errors/
Contexts/GameCore/Scripts/Extensions/
Contexts/GameCore/Scripts/Utilities/
Contexts/GameCore/Scripts/Validation/
Contexts/GameCore/Scripts/Abstract/Networking/
Contexts/GameCore/Scripts/Enums/Networking/
Contexts/GameCore/Scripts/DataHolders/Data/
Contexts/GameCore/Scripts/Other/Strategies/
Contexts/GameCore/Scripts/Components/Networking/
```

Context-owned folders still follow the folder/suffix rule documented below.
For example, `Extensions` contains only `*Extensions`, and `Utilities` contains
only `*Utility`.

Use the narrowest valid owner:

- code used by one feature stays in that feature;
- code shared by several features in one context may move to context `Scripts`;
- code shared across contexts may move to `Template/Scripts`, but only when its
  responsibility is genuinely context-independent;
- networking foundations that describe the game runtime remain in
  `GameCore/Scripts`, even when several gameplay contexts consume them;
- code independent from project contexts and external APIs moves to `Shared`;
- behavioral objects requiring DI lifetime or initialization remain features.

Context-shared dependencies are one-way:

```text
Feature -> Context Scripts -> Template Scripts -> Shared
```

Context `Scripts` must not depend on concrete features. If a shared type needs
a feature-internal contract, it remains owned by that feature or the contract
is extracted to a lower-level owner first.

A script must remain feature-owned when any of the following is true:

- it implements a complete capability or business rule;
- it participates in feature installation, initialization, shutdown, or DI;
- it owns runtime state or coordinates other behavioral objects;
- it depends on concrete feature internals;
- it owns feature-specific resources or configuration.

Context `Scripts` must not become a substitute for `Features`, `Template`, or
`Shared`.

### Shared

`Assets/ProjectCore/Shared` contains code that expresses project-independent
rules and abstractions without depending on infrastructure.

Allowed dependencies:

- .NET/BCL namespaces such as `System` and `System.Collections.Generic`.

Forbidden dependencies include, but are not limited to:

- UnityEngine and UnityEditor;
- Photon Fusion;
- Zenject;
- UniTask;
- Newtonsoft.Json;
- SDKs, backend clients, platform APIs, and project contexts.

Fusion state machines, Zenject installers, network spawning, scene
initializers, and feature lifecycle code are infrastructure and must live in
the owning context rather than `Shared`.

`Shared` must not become a general-purpose utilities folder. Feature behavior
belongs to its feature even when the implementation itself is small.

Pure C# attributes belong to `Shared/Attributes`. Unity property attributes
belong to `Template/Scripts/DataHolders/Attributes`, and their drawers belong
to `Template/Scripts/Other/Editor`. Reusable MonoBehaviour state foundations
belong to `Template/Scripts/Abstract`; mediator handler components belong to
`Template/Scripts/Components/Handlers`, with their Unity-dependent contracts in
`Template/Scripts/Abstract/Mediators`. Shared contracts must not depend on
these Template implementations.

Current Shared categories are:

- `Attributes` for pure C# declarative metadata such as former-name mappings;
- `Collections` for small context-independent collection primitives;
- `Modifiers` for context-independent sequential value transformations;
- `Patterns` for pure C# composites, mediators, and state-machine foundations;
- `Results` for success/failure values and errors. Results are standalone
  primitives rather than a `Patterns` category.

Shared primitives do not receive dedicated test fixtures. Feature and
integration tests verify behavior at the owning application boundary.

### ThirdParty

Game-specific SDKs, packages, and external assets live in
`Assets/ProjectCore/ThirdParty`. Common plugins owned by the project baseline
live in `Assets/ProjectCore/Contexts/Template/Plugins`.

Third-party source code keeps its original namespaces and naming conventions.

## Dependency Direction

The intended dependency flow is:

```text
Shared -> .NET APIs only
Template -> Shared
Project -> Template + Shared + application-lifetime integrations
GameCore -> Template + Shared + Project services
Preloader -> Template + Shared + Project services + startup integrations
Gameplay contexts -> Template + Shared + Project services + GameCore
```

Rules:

- `Shared` does not depend on a context or an external API.
- `Template` does not depend on `Project`, `Preloader`, `GameCore`, or a
  gameplay context.
- `Project` does not depend on `Preloader` or a gameplay context.
- `GameCore` does not depend on `Preloader` or a gameplay context.
- A gameplay context may compose and use `GameCore` features.
- `Preloader` may consume persistent Project services and may know registered
  navigation destinations, but no persistent or gameplay context may depend on
  `Preloader`.
- Cross-context references must follow the dependency direction above.
- Cyclic context dependencies are forbidden.

TroughGame's project-owned runtime code shares the existing `ProjectCore.Runtime`
assembly, defined by `Assets/ProjectCore/ProjectCore.Runtime.asmdef`. Contexts and
features are not separated by assembly boundaries, so compilation alone does not
enforce their dependency direction. These rules must be validated separately
with static dependency checks and code review.

## Runtime Entry Flow

The authored hierarchy for gameplay scenes is standardized separately in
[`SCENE_HIERARCHY_STANDARD.md`](SCENE_HIERARCHY_STANDARD.md). It defines the
required scene containers and their ownership without changing the lifecycle
and DI rules in this document.

`Scene_Preloader` is the only supported application entry point and must be the
first enabled scene in Build Settings. Gameplay scenes are navigation
destinations and must not be used to initialize the complete application.

```text
Application startup
  Zenject ProjectContext
    ProjectContextInstaller.InstallBindings()
    bind Template infrastructure
    bind application-lifetime features, services, and state
    remain in DontDestroyOnLoad

Scene_Preloader
  Preloader SceneContext
  PreloaderContextInstaller.InstallBindings()
    bind transient startup features
    bind the single ApplicationEntryPoint
  ApplicationEntryPoint.Start()
    enter the application flow once, after Zenject injection is complete
  ApplicationInitializationFlow
    ProjectContextInitializer
      FeatureInitializationFlow(Project features)
    PreloaderContextInitializer
      FeatureInitializationFlow(Preloader features)
    ask ISceneFlowService to open the configured first gameplay scene
  unload Scene_Preloader and destroy all Preloader-scoped objects

Scene_TechnicalPrototype (initial gameplay destination)
  TechnicalPrototypeContextInstaller
    bind only scene-owned features
    compose shared GameCore features for the scene
  persistent SceneFlowService
    resolve IGameSceneLifecycle from the scene container
    FeatureInitializationFlow(TechnicalPrototype features)
    expose the scene as ready only after initialization completes
```

Startup rules:

- `Assets/ProjectCore/Resources/ProjectContext.prefab`,
  `ProjectContextInstaller`, and all services that must
  survive scene changes belong to the `Project` context.
- The Project context is initialized once and remains alive until application
  shutdown.
- `PreloaderContextInstaller` and `ApplicationEntryPoint` belong to the
  `Preloader` scene context and are destroyed when that scene is unloaded.
- Preloader features are for one-shot work such as consent flows, SDK startup,
  warm-up, or initial downloads. A service that must remain usable afterward
  must be bound in `Project`, even if Preloader triggers its initialization.
- A gameplay scene assumes that both Project initialization and required
  Preloader startup work have completed.
- Opening a gameplay scene directly is allowed only through a dedicated editor
  development bootstrap that reproduces or redirects through Preloader setup.
- The initial destination is `TechnicalPrototype` during development, and it
  must be selected by Preloader navigation rather than used as the application
  entry scene in Build Settings.
- Installers only register bindings. They do not start asynchronous work or
  initialize features.
- `ApplicationEntryPoint.Start()` is the only project-owned Unity lifecycle
  callback allowed to trigger application startup. `Start` is used so Zenject
  has completed scene injection before the asynchronous pipeline begins.
- `ApplicationEntryPoint` only crosses the Unity-to-application boundary. It
  delegates ordering to `ApplicationInitializationFlow` and attaches root
  cancellation and error handling to the returned async operation.
- Features, services, components, and scene initializers must not create
  independent initialization chains from `Awake`, `OnEnable`, or `Start`.
- Scene transitions go through one persistent flow coordinator. That
  coordinator loads the scene, obtains its initialization contract through DI,
  awaits scene initialization, and only then exposes the scene as ready.
- Unity lifecycle methods may perform strictly local component setup, but they
  must not resolve dependencies, initialize application systems, or determine
  startup order.

## Feature Architecture

All features live directly under `Contexts/<Context>/Features/<Feature>`.
Features are not classified as Modules, Implementations, Infrastructure, or
Bridges. Ownership comes from the context and the feature's responsibility;
dependency direction comes from public contracts, not role folders.

### Feature Dependencies

- Each feature owns one coherent capability, its code, and its resources.
- Features consume other capabilities through narrow public contracts and DI.
- Reusable capabilities must not depend on the concrete consumers that use them.
- Consumers must not access another feature's internal implementation types.
- Extract shared behavior into an appropriate owning feature rather than
  coupling consumers to one another's internals.
- Adapter features may connect independent systems through their public
  contracts. Names such as `SceneLoadingScreenBridge` describe responsibility
  and remain valid without a separate Bridges category.
- Adapters must stay focused and must not accumulate unrelated game logic.
- Feature dependency cycles are forbidden. Cross-context dependencies follow
  the context dependency direction defined above.
- Reusable cross-context capabilities belong to `Template/Features/<Feature>`.
  Code ownership and runtime lifetime are separate: a Template feature may be
  installed into Project, Preloader, or scene containers as required.
- Feature groups and installers compose concrete features; runtime consumers
  depend on the contracts bound by those features.

### Feature Groups

`FeatureGroup` is an explicitly ordered composition of related features. It
does not introduce a DI scope, own business logic, or change feature ownership.
Its child features are installed and initialized in declaration order.

`BaseFeatureGroup` composes code-created features. A group that owns serialized
references to MonoBehaviour features inherits `BaseMonoBehaviourFeatureGroup`,
is registered through `AddFeatureFromComponent<TFeatureGroup>()`, and adds those
children through explicit serialized references. Component searches are not
used for group composition.

Use a group when a context needs to keep a coherent set of related features
together, such as shared GameCore features or scene-owned features. A
group may contain another group. Context installers compose top-level groups
and standalone features only.

### Shared Feature Lifetimes

- `NetworkEntities` is owned by GameCore and installed in the gameplay context.
  Its entity IDs use a uint with 6 type bits and 26 index bits. The spawning
  authority issues monotonically increasing indexes without reuse within that
  context. Entity lookup remains a registry responsibility; protocol codes,
  validation, and world-lifetime limits are documented in
  [`features/NETWORK_ENTITIES.md`](features/NETWORK_ENTITIES.md).
- `AppTime` is owned by a Template feature but installed once in the
  persistent Project context. Consumers inject `IAppTimeService`; static access
  is forbidden. Its public time values use UTC, network synchronization is
  asynchronous, and local UTC time remains the fallback.
- `LocalConfig` is owned by a Template feature and installed separately
  in every DI context that owns a flat local config catalog. Lookup checks the
  current context first and then parent contexts; nested provider catalogs and
  mutable global provider registries are forbidden. Its MonoBehaviour feature
  privately owns the serialized catalog and passes it directly to its service;
  the catalog is not bound in DI. Usage and lifetime details are documented in
  [`features/LOCAL_CONFIG.md`](features/LOCAL_CONFIG.md).
- `Logging` is owned by a Template feature and installed once in the
  persistent Project context. Consumers use the injected `IDebugLogger` or a
  context-scoped `IDebugLogger<TFeature>`; log handlers connect through
  `ILogService`. Usage and lifetime details are documented in
  [`features/LOGGING.md`](features/LOGGING.md).
- `CommandLine` is an independent Template feature installed before Debug
  Tools in the persistent Project context. It provides an immutable parsed
  argument snapshot and the dedicated-server cancel request. It remains
  available in every build type and is not controlled by Debug Tools.
- `DebugTools` is the persistent Template feature composition for
  Cheats, Debug Visualization, and Runtime Console. The systems remain
  independent features and communicate through public contracts. A single
  `IDebugToolsService` controls build availability and runtime state; disabling
  a build target prevents debug UI, cheat reflection, and debug visualization
  runtime objects from starting. Debug Tools may consume `ICommandLineService`
  for optional state overrides, but Command Line never depends on Debug Tools.
  Debug Visualization exposes a stateless static drawing facade for call-site
  convenience; its runner, registered targets, reflection caches, and cleanup
  belong to a context-owned DI system and registry. Static visualization
  collections or Unity-object ownership are forbidden.
  Runtime Console follows System-Controller-View separation: its System owns
  logs, history, and command execution independently of the window; its
  Controller owns presentation state and UI flow; MonoBehaviour and
  VisualElement Components only render supplied data and forward user input.

> Folder taxonomy, suffix semantics, naming, member order, access modifiers,
> resource naming, and coding style are maintained in
> [`CONVENTIONS.md`](CONVENTIONS.md). This document defines architecture,
> ownership, dependency direction, lifecycle, and DI rules.

## Feature Structure

The canonical folder layout, placement decision order, category tables, and
suffix meanings are maintained in
[`CONVENTIONS.md`, Feature Layout](CONVENTIONS.md#feature-layout) and
[Script Categories and Suffixes](CONVENTIONS.md#script-categories-and-suffixes).
This section adds ownership, DI, and lifecycle constraints.

Every feature has a clear owner context, keeps its code and resources inside
that feature, and uses `Scripts/Init/<Feature>Feature.cs` when it participates
in the unified lifecycle. Feature folders express ownership, not namespaces.
Mandatory Clean Architecture layer folders are not part of the project standard.

### Strict Folder And Suffix Rule

Use the [canonical placement rules](CONVENTIONS.md#script-placement) and
[role tables](CONVENTIONS.md#script-categories-and-suffixes). Document a new
category in CONVENTIONS before introducing it; define any new ownership,
dependency, or lifecycle constraints here. A contract role alone needs no folder.

### Managers

Managers are non-MonoBehaviour behavioral objects managed by DI in their
owning container. `AsSingle` means one instance per owning Project or Scene
container, rather than one application-global instance. See the
[Manager roles](CONVENTIONS.md#managers), including
[System and Service](CONVENTIONS.md#system-and-service).

The existing open-generic `IDebugLogger<>` binding to
`FeatureDebugLoggerService<>` is an explicit `AsCached` case: one instance per
closed generic type per context. `BaseFeatureInstaller` owns this registration;
it does not change the default lifetime for other Managers. See
[Logging](features/LOGGING.md#feature-logging).

Managers must not locate dependencies through scene searches or service
locators, start initialization from constructors or detached background tasks,
or depend on another feature's internal implementation. Their initialization
is invoked by the owning feature after bindings are installed.

### DataHolders

Use the [DataHolders rules](CONVENTIONS.md#dataholders) for passive data,
configuration, settings, and transport shapes. Passing a config through DI
does not make it a Manager or give it ownership of a workflow.

### Components

Use the [Components categories](CONVENTIONS.md#components) for Unity-facing
types. Components receive runtime dependencies through DI; serialized fields
reference scene objects, prefabs, assets, and visual configuration.
Presentation Views render state and forward events to injected contracts.
Unity-bound states own bounded processing and delegate reusable operations.
Local Unity callbacks must not start feature or application initialization.

### Other

Use the [Other roles](CONVENTIONS.md#other) for remaining feature-owned
behavior. If an object becomes a long-lived DI-managed dependency, move it
to its actual Managers role. Adapter dependencies still follow the public
contract and context-direction rules.

### Abstract

The [contract vocabulary and abstract-base naming](CONVENTIONS.md#abstract)
are maintained in CONVENTIONS. A contract belongs to the feature that owns
the capability, regardless of where its first implementation lives.
Runtime consumers use those public contracts; feature groups and installers
compose implementations.

### Static

Follow the [Static rules](CONVENTIONS.md#static), including the bounded Debug
Visualization facade exception. Runtime state and backend lifecycle remain
owned by DI objects, rather than by their static facade.

### Tests

Test placement and fixture naming are defined in
[CONVENTIONS](CONVENTIONS.md#tests). Tests share their context's flat namespace;
their path and assembly distinguish them from production code.
Production uses the existing `ProjectCore.Runtime` assembly; Editor and Play
tests use separate assemblies and explicitly reference it. Preserve these
assembly definitions and references; do not add new `.asmdef` files.
Test assemblies are excluded from normal player builds and must not be
production dependencies. Tests of startup enter scenes through the supported
Preloader flow.

### Enums

Enum naming is defined in [CONVENTIONS](CONVENTIONS.md#enums).

### Init

Composition type roles are defined in [CONVENTIONS](CONVENTIONS.md#init).
An Initializer is explicitly invoked and awaited by its owning flow.
The lifecycle and initialization order are defined below.

## Initialization Flow

The project has one initialization flow and one application entry point.
Registration and initialization are separate operations:

1. Zenject creates `ProjectContext` and executes installers that only register
   bindings.
2. Unity loads `Scene_Preloader`; its installer registers Preloader bindings
   and the single `ApplicationEntryPoint`.
3. Unity invokes the single `ApplicationEntryPoint.Start()` after Zenject has
   injected its dependencies.
4. The entry point starts and root-observes `ApplicationInitializationFlow`.
5. `IProjectContextInitializer` awaits
   `FeatureInitializationFlow` for application-lifetime features.
6. `IPreloaderContextInitializer` awaits the same reusable flow for transient
   startup features.
7. `ApplicationInitializationFlow` asks the persistent `ISceneFlowService` to
   navigate to the configured initial scene definition.
8. `SceneFlowService` loads the scene, supplies its typed payload through DI,
   resolves its `IGameSceneLifecycle`, and awaits its local
   `FeatureInitializationFlow`.
9. The scene is marked ready only after that pipeline succeeds.

Initialization responsibilities are deliberately separated:

- `ApplicationEntryPoint` is the single Unity lifecycle trigger.
- `ApplicationInitializationFlow` owns the Project -> Preloader -> first-scene
  startup order.
- `FeatureInitializationFlow` is a reusable DI service that sequentially
  initializes the explicitly ordered features of one owning context. It never
  starts itself from a Unity callback.
- `SceneFlowService` owns all later scene transitions. It invokes
  `ExitAsync` before Unity unloads the previous scene, then invokes the next
  scene lifecycle initializer explicitly.
- Context-specific contracts (`IProjectContextInitializer`,
  `IPreloaderContextInitializer`, and `IGameSceneLifecycle`) prevent ambiguous
  resolution of a generic initializer across parent and child Zenject
  containers.
- Installers register features and flows only; an installer must not also act
  as the asynchronous feature initialization flow.

### Scene Flow

`SceneFlowFeature` is Template infrastructure installed in `ProjectContext`.
It is the only project-owned path for loading gameplay scenes.

- Consumers use `ISceneFlowService.LoadAsync<TScene, TSettings>`; they never pass
  scene names, paths, or build indexes.
- `SceneCatalogConfig` owns the registered `SceneDefinition` assets. A
  definition maps a typed scene to its validated build index and payload type.
- Immutable scene settings are bound directly into the new `SceneContext` by
  their concrete `TSettings` type.
- A scene implements `IGameSceneLifecycle`. `ExitAsync` is awaited while its
  container remains alive and before the next single-scene load begins.
- Scene lifetime cancellation occurs after `ExitAsync` and before the next
  scene is loaded.
- Scene flow does not depend on popup or screen-navigation features. Persistent
  transition UI integrates only through `ISceneTransitionPresenter`.
- `LoadingScreenFeature` is Template infrastructure installed in the persistent
  Project context. It owns one scene-independent loading screen without
  navigation history, hierarchy, or user-controlled completion.
- `SceneLoadingScreenBridgeFeature` adapts `ILoadingScreenSystem` to
  `ISceneTransitionPresenter`; neither Scene Flow nor Loading Screen depends on
  the concrete implementation of the other system.
- A scene transition shows its presenters before the current scene exits and
  hides them only after the next scene finishes `IGameSceneLifecycle`
  initialization. Presenter cleanup remains in `finally`, including failed or
  cancelled transitions.

Feature initialization rules:

- Every asynchronously initialized feature exposes one common async
  initialization contract.
- `BaseFeature` is the default implementation of that contract. A feature that
  requires serialized scene, prefab, or asset references may instead inherit
  `BaseMonoBehaviourFeature`, live on the same GameObject as its context
  installer, and be added through `AddFeatureFromComponent<TFeature>()`.
- Serialized feature groups inherit `BaseMonoBehaviourFeatureGroup`, reference
  every MonoBehaviour child explicitly, and are themselves added through
  `AddFeatureFromComponent<TFeatureGroup>()`.
- Both feature bases expose protected `BindAsSingle`, `BindInterfacesAsSingle`,
  `Resolve`, and `ResolveAs` DI helpers. Their implementation is centralized in
  `FeatureLifecycleAdapter` and the bases only delegate to it. Use
  `BindInterfacesAsSingle<T>` when lifecycle contracts such as `IDisposable`
  must be registered without exposing the concrete implementation as self.
  Feature implementations must not access `DiContainer` directly.
  Use `ResolveAs` only during feature initialization to invoke
  implementation-only startup work; do not add `InitializeAsync` to a public
  service or system interface solely for feature lifecycle orchestration.
- Initialization order is declared explicitly by the owning context or feature
  group. It is not discovered through Unity callback timing or reflection.
- Every feature base receives its owning context container exactly once through
  `InstallBindings(DiContainer)`. After every feature has registered bindings,
  `InitializeAsync(CancellationToken)` may resolve and initialize its DI-managed
  services; resolving them while bindings are still being installed is
  forbidden. MonoBehaviour features follow the same flow and must not use
  Unity callbacks as additional entry points.
- `FeatureInitializationFlow` is registered as one context-local singleton and
  is conditionally injectable only into `BaseContextInitializer` descendants.
  Other services and Feature must not invoke it directly.
- Each initialization method completes only when the feature is ready for its
  consumers.
- Initialization failures propagate to the root flow, which logs and handles
  them in one place. Features must not silently detach initialization tasks.
- Scene-owned initialization receives a cancellation token tied to scene
  unloading; application-owned initialization receives an application-lifetime
  token.
- Re-entering an already initialized application feature must be either an
  explicit no-op or an error according to its contract; accidental double
  initialization is forbidden.
- Disposal and scene shutdown are coordinated by the same flow owner rather
  than by an unrelated startup chain.

Forbidden initialization patterns:

```text
FeatureA.Awake() -> FeatureB.Init() -> FeatureC.Start()
Start() -> async void initialization
OnEnable() -> Container.Resolve<T>() -> Init()
fire-and-forget startup tasks without root error handling
script execution order used as a dependency mechanism
```

`Awake`, `OnEnable`, and `Start` may still be used for local MonoBehaviour
concerns that do not affect dependency creation, application readiness, feature
initialization, or initialization order.

## Dependency Injection

Project-owned runtime dependencies are supplied through Zenject DI.

Rules:

- Plain C# services and systems use constructor injection.
- MonoBehaviours use Zenject injection for runtime services and serialized
  fields only for scene or asset references.
- Installers are composition roots: they bind interfaces, implementations,
  factories, configuration, and lifecycle contracts.
- Consumers depend on the narrowest appropriate interface rather than a
  concrete implementation.
- Features satisfy public capability contracts through DI bindings.
- Bridge services such as the signal bus are injected; features do not locate
  the bus or one another globally.
- Object creation that requires injected dependencies goes through an injected
  factory.
- `IClassFactory` is the shared Template infrastructure factory for runtime
  object creation that must use Zenject injection. It is installed separately
  into every context that needs runtime creation, so each resolved factory owns
  the current context container through constructor injection.
- `LocalSaveFeature` is shared Template infrastructure registered in the
  persistent Project context. Consumers inject `ILocalSaveService`; they do not
  access PlayerPrefs, file paths, serializers, or storages directly.
- Local saves are client-owned convenience data, never an authority or
  anti-cheat boundary. Server-owned game state must not depend on local-save
  integrity or secrecy.
- Optional dependencies must be represented explicitly by an optional contract
  or null-object implementation, not by runtime searches.

Do not use the following as substitutes for DI:

```text
static mutable singletons
service locator APIs
FindObjectOfType / FindAnyObjectByType for services
GameObject.Find for dependencies
Container.Resolve outside a composition-root or DI infrastructure adapter
new ConcreteService(...) inside a consumer
direct implementation-feature references
```

External SDK callbacks enter the application through an adapter owned by the
appropriate Project, Preloader, or gameplay feature. The adapter forwards data
to injected contracts or a bridge feature instead of exposing SDK globals to
consumers.

## Namespace Convention

Folder depth below a context does not extend its namespace.

```csharp
// Assets/ProjectCore/Shared/**
namespace Shared;

// Assets/ProjectCore/Contexts/Template/**
namespace ProjectCore.Template;

// Assets/ProjectCore/Contexts/Project/**
namespace ProjectCore.Project;

// Assets/ProjectCore/Contexts/Preloader/**
namespace ProjectCore.Preloader;

// Assets/ProjectCore/Contexts/GameCore/**
namespace ProjectCore.GameCore;

// Assets/ProjectCore/Contexts/TechnicalPrototype/**
namespace ProjectCore.TechnicalPrototype;

// Assets/ProjectCore/Contexts/Prototype/**
namespace ProjectCore.Prototype;
```

Do not use legacy forms such as:

```text
GameCore.Movement
GameCore.Combat.Init
Prototype.Prototype
Prototype.Player.Init
PhotonZenjectBridge
namespace Project
ProjectCore.Contexts.<Context>.<Feature>
```

External packages are excluded from this convention.

## Naming Convention

`CONVENTIONS.md` is the source of truth for naming and coding style.

### Code

Use its [Naming](CONVENTIONS.md#naming),
[Type Name Composition](CONVENTIONS.md#type-name-composition),
[Injection Method Names](CONVENTIONS.md#injection-method-names),
[Empty, Default, and NoOp](CONVENTIONS.md#empty-default-and-noop), and
[Nested Implementation Types](CONVENTIONS.md#nested-implementation-types).
Injecting dependencies does not start initialization; it remains part of the
explicitly awaited feature/context flow.

### Folders

Use the canonical [Source Layout](CONVENTIONS.md#source-layout),
[Feature Layout](CONVENTIONS.md#feature-layout), and
[category tables](CONVENTIONS.md#script-categories-and-suffixes).

### Unity Assets

Resource names, semantic folders, owner names, CreateAssetMenu, and rename
compatibility are defined in
[Unity Resource Naming](CONVENTIONS.md#unity-resource-naming).
An instance's owning context may differ from the context that defines its type.

## Coding Rules

Coding style, member order, and API visibility are maintained in
[CONVENTIONS](CONVENTIONS.md#c-and-unity-coding-rules). Ownership and runtime
behavior continue to follow this document's dependency, DI, and lifecycle
rules. In particular, Services must not depend directly on Views; integration
uses the appropriate public presentation contract.

## Runtime UI Toolkit

Use [UI_TOOLKIT_STANDARD.md](UI_TOOLKIT_STANDARD.md) for runtime UI presentation:
MVVM by default and MVP only as an explained, accepted architecture choice.
It owns ViewModel/Presenter boundaries, synchronization and presentation lifetime.
Domain services do not depend on Views, ViewModels or Presenters. Construct per-view presentation objects through
the existing DI/factory flow; do not bind them as global scene/application
singletons. Reuse Template ScreenNavigation/Popup definitions, catalogs and hosts.
Existing UI migration requires approved scope. Folder/suffix ownership remains
with CONVENTIONS.

Context-owned UI resources are consumed within their owning context. Cross-context
UI reuse uses root-owned common resources under `Assets/ProjectCore/Assets` or
project-independent Template resources. Do not reference another concrete
context's private UI assets directly; promote genuinely common resources through
an approved ownership change. Root common UI must not depend on concrete-context
UI assets. Resource instances may still use reusable Template types.

## Architecture Validation

Use Strict validation for local verification and CI. It returns a nonzero
exit code for Error diagnostics; heuristic Warnings require review.
The current baseline has zero errors and warnings.

Commands, diagnostic families, regression fixtures, and analysis limits are
maintained in [the validator README](../Tools/Architecture/README.md).
The [Enforcement section](CONVENTIONS.md#enforcement) distinguishes automated
structure checks from semantic naming review. A clean report does not replace
compilation, Unity tests, serialized-reference checks, or architecture review.
