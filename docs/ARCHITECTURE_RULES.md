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

Project-owned content lives under `Assets/ProjectCore`. Supported roots are:

| Root | Owner / contents |
| --- | --- |
| `Assets` | Project-wide resource instances |
| `Contexts/<Context>` | `Features/<Feature>`, context `Scripts`, `Assets`, and `Tests/Editor` or `Tests/Play` as needed |
| `Shared` | Context-independent .NET-only primitives |
| `ThirdParty` | Game-specific external dependencies |
| `Resources/ProjectContext.prefab` | Zenject's fixed-location exception; do not move or rename |

Common plugins remain in `Contexts/Template/Plugins`. Legacy roots/namespaces are
unsupported. Detailed feature/category/resource placement belongs to
[CONVENTIONS](CONVENTIONS.md#source-layout); create only the folders actually needed.

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

Code may live in `Contexts/<Context>/Scripts/<Category>` when it belongs to that
context, is shared by its composition/features, and has none of the following:

- an independently connectable capability, business rule or feature lifecycle;
- DI registration/resolution/initialization, context-level runtime state, or
  coordination of feature-owned services, systems, managers, factories or views;
- dependencies on concrete feature internals;
- feature-specific resources/configuration.

Typical candidates are shared abstractions, enums, extensions, utilities, constants,
validation helpers, passive data and generic components. Absence of DI alone is not
sufficient: a capability with its own responsibility/state/resources/lifecycle stays
feature-owned. Use the [category/suffix rules](CONVENTIONS.md#script-categories-and-suffixes)
for these context scripts as well.

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

Context `Scripts` must not become a substitute for `Features`, `Template` or `Shared`.

### Shared

`Assets/ProjectCore/Shared` contains code that expresses project-independent
rules and abstractions without depending on infrastructure.

Only .NET/BCL dependencies are allowed. Unity, Fusion, Zenject, UniTask,
Newtonsoft.Json, other SDKs/platform APIs and project contexts are forbidden.
Their runtime/lifecycle infrastructure belongs in the owning context.

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
first enabled scene in Build Settings. Gameplay scenes are navigation destinations.
The configured first destination is currently `TechnicalPrototype`, selected by
Preloader navigation rather than used as the application entry scene.

- `ProjectContextInstaller` and persistent services belong to Project;
  `PreloaderContextInstaller` and `ApplicationEntryPoint` belong to Preloader.
  Their lifetimes follow the [context rules](#contexts).
- Preloader owns one-shot work such as consent, SDK startup, warm-up or downloads.
  A service needed afterward is registered in Project even if Preloader initializes it.
- Gameplay assumes Project and required Preloader startup have completed. Direct
  gameplay-scene opening requires an Editor development bootstrap that reproduces
  or redirects through Preloader setup.
- `ApplicationEntryPoint.Start()` is the sole Unity callback triggering application
  startup, after Zenject injection. It delegates ordering to
  `ApplicationInitializationFlow` and attaches root cancellation/error handling.

The ordered startup, installer responsibilities, scene readiness and permitted
local callbacks are defined once in [Initialization Flow](#initialization-flow).

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

## Feature Structure

Features own their code/resources and express ownership rather than namespaces.
The closed categories, placement order and suffix meanings are defined in
[CONVENTIONS](CONVENTIONS.md#feature-layout). Mandatory Clean Architecture layer
folders and intermediate feature role folders are not part of the standard.
Contracts belong to the feature owning the capability; runtime consumers use its
public contracts, while installers/groups compose concrete implementations.

### Strict Folder And Suffix Rule

Document a new category in CONVENTIONS before introducing it; add any new ownership,
dependency or lifecycle constraints here. A contract role alone needs no folder.

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

### Components

Components receive runtime services through DI and scene/asset/visual references
through serialized fields. Category-specific behavior and placement are defined in
[CONVENTIONS](CONVENTIONS.md#components).

The [Other](CONVENTIONS.md#other), [DataHolders](CONVENTIONS.md#dataholders),
[Abstract](CONVENTIONS.md#abstract) and [Static](CONVENTIONS.md#static) rules own their
respective type semantics; feature dependencies still follow the public contracts
and context direction above.

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
- Serialized groups follow the explicit-reference composition in [Feature Groups](#feature-groups).
- Both feature bases expose protected `BindAsSingle`, `BindInterfacesAsSingle`,
  `Resolve`, and `ResolveAs` DI helpers. Their implementation is centralized in
  `FeatureLifecycleAdapter` and the bases only delegate to it. Use
  `BindInterfacesAsSingle<T>` when lifecycle contracts such as `IDisposable`
  must be registered without exposing the concrete implementation as self.
  Feature implementations must not access `DiContainer` directly.
  Use `ResolveAs` only during feature initialization to invoke
  implementation-only startup work; do not add `InitializeAsync` to a public
  service or system interface solely for feature lifecycle orchestration.
- Initialization order is declared by the owning context or feature group, not
  Unity callback timing, script execution order or reflection.
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
  them in one place. No `async void` or detached initialization tasks.
- Scene-owned initialization receives a cancellation token tied to scene
  unloading; application-owned initialization receives an application-lifetime
  token.
- Re-entering an already initialized application feature must be either an
  explicit no-op or an error according to its contract; accidental double
  initialization is forbidden.
- Disposal and scene shutdown are coordinated by the same flow owner rather
  than by an unrelated startup chain.

`Awake`, `OnEnable` and `Start` may perform local component setup only; they
must not create/resolve dependencies, start application/feature initialization or
determine readiness/order. The sole application startup exception is
`ApplicationEntryPoint.Start()` as defined above.

## Dependency Injection

Project-owned runtime dependencies are supplied through Zenject DI. Injection
supplies dependencies; initialization remains part of the explicitly awaited flow.

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

Project code uses only `Shared` or `ProjectCore.<Context>`, for example
`ProjectCore.GameCore`. Folder depth and feature names do not extend namespaces;
legacy forms such as `GameCore.Movement` and `ProjectCore.Contexts.<Context>.<Feature>`
are invalid. External packages retain their original namespaces.

## Naming Convention

[CONVENTIONS](CONVENTIONS.md) owns naming, category/suffix tables, folder/resource
layout, member order, access modifiers and coding style. Use the relevant section
instead of maintaining copies in this architecture document.

## Coding Rules

Services must not depend directly on Views; integration uses public contracts or
adapters. Other C#/Unity coding rules are defined in
[CONVENTIONS](CONVENTIONS.md#c-and-unity-coding-rules).

## Runtime UI Toolkit

[UI_TOOLKIT_STANDARD](UI_TOOLKIT_STANDARD.md) owns presentation architecture,
MVVM/default and accepted MVP choices, binding, per-view lifetime and resource
reuse/promotion. These UI rules supplement the dependency and ownership rules here.

## Architecture Validation

Use Strict validation for local verification and CI. It returns a nonzero
exit code for Error diagnostics; heuristic Warnings require review.
The current baseline has zero errors and warnings.

Commands, diagnostic families, regression fixtures, and analysis limits are
maintained in [the validator README](../Tools/Architecture/README.md).
The [Enforcement section](CONVENTIONS.md#enforcement) distinguishes automated
structure checks from semantic naming review. A clean report does not replace
compilation, Unity tests, serialized-reference checks, or architecture review.
