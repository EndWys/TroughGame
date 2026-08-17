# TroughGame Architecture Rules

TroughGame is a multiplayer friends-slope game about dungeons.

## Architecture

The project follows the context- and feature-based organization of BG Games
Platform, with the following TroughGame-specific rules:

- `Template` remains a context inside `Assets/ProjectCore/Contexts`.
- `Domain` is independent of external APIs. It may use only .NET, UnityEngine,
  and UnityEditor APIs.
- Project-owned namespaces are intentionally short: `Domain` or
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

    ContentData/
      InputActions/
      Settings/

    Contexts/
      Template/
        Features/
          Bridges/
          Implementations/
          Infrastructure/
          Modules/
        Plugins/
        Scripts/
        Tests/
          Editor/
          Play/

      Project/
        Features/
        Scripts/
          Init/

      Preloader/
        Features/
        GraphicResources/
          Scenes/
        Resources/
        Scripts/
          Abstract/
          Init/
          Navigation/

      GameCore/
        Features/
          Bridges/
          Implementations/
          Infrastructure/
          Modules/
        GraphicResources/
        Scripts/
          Init/

      Prototype/
        Features/
          Bridges/
          Implementations/
          Infrastructure/
          Modules/
        GraphicResources/
          Scenes/
        Resources/
        Scripts/
          Init/

    Domain/
      Attributes/
        Editor/
      Collections/
      Modifiers/
      Results/
    ThirdParty/
```

### ContentData

`ContentData` contains project-wide designer- or game-owned data that is not
owned by one feature or scene. Examples include input actions, rendering
settings, localization tables, level catalogs, and shared balance data.

Feature-owned configs and assets stay inside their feature.

### Contexts

Contexts define ownership and lifetime boundaries.

| Context | Responsibility | Namespace |
| --- | --- | --- |
| `Template` | Shared initialization, DI infrastructure, bootstrap code, reusable utilities, and common plugins | `ProjectCore.Template` |
| `Project` | Zenject `ProjectContext`, application-lifetime services, and state that survives every scene change | `ProjectCore.Project` |
| `Preloader` | The only scene entry point, transient startup initialization, and navigation to the first gameplay scene | `ProjectCore.Preloader` |
| `GameCore` | Gameplay features reusable by game scenes | `ProjectCore.GameCore` |
| `Prototype` | One concrete gameplay scene and only the behavior specific to that scene | `ProjectCore.Prototype` |

Add a new context when code has a distinct lifetime or composition root. Do
not create a context merely to group related classes.

`Prototype` is not a special application root. It represents the first
gameplay-scene context and may later be accompanied or replaced by contexts
such as `Dungeon`, `Lobby`, or `Tutorial`. Features needed by more than one
gameplay scene belong to `GameCore`; only scene composition and scene-specific
behavior belong to the gameplay context.

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
Contexts/GameCore/Scripts/Views/Components/Networking/
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
- code independent from project contexts and external APIs moves to `Domain`;
- behavioral objects requiring DI lifetime or initialization remain features.

Context-shared dependencies are one-way:

```text
Feature -> Context Scripts -> Template Scripts -> Domain
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
`Domain`.

### Domain

`Assets/ProjectCore/Domain` contains code that expresses project-independent
rules and abstractions without depending on infrastructure.

Allowed dependencies:

- .NET/BCL namespaces such as `System` and `System.Collections.Generic`;
- UnityEngine;
- UnityEditor, when editor code is isolated in an `Editor` folder or guarded by
  `UNITY_EDITOR`.

Forbidden dependencies include, but are not limited to:

- Photon Fusion;
- Zenject;
- UniTask;
- Newtonsoft.Json;
- SDKs, backend clients, platform APIs, and project contexts.

Fusion state machines, Zenject installers, network spawning, scene
initializers, and feature lifecycle code are infrastructure and must live in
the owning context rather than `Domain`.

`Domain` must not become a general-purpose utilities folder. Feature behavior
belongs to its feature even when the implementation itself is small.

Reusable attributes and their editor-only drawers belong to `Domain` when they
depend only on .NET and Unity APIs. Runtime attributes stay outside contexts;
editor drawers are isolated in a nested `Editor` folder.

Current shared Domain categories are:

- `Attributes` for reusable declarative metadata such as subclass selection
  and former-name mappings;
- `Collections` for small context-independent collection primitives;
- `Modifiers` for context-independent sequential value transformations;
- `Results` for success/failure values and errors. Results are standalone
  primitives rather than a `Patterns` category.

Domain primitives do not receive dedicated test fixtures. Feature and
integration tests verify behavior at the owning application boundary.

### ThirdParty

Game-specific SDKs, packages, and external assets live in
`Assets/ProjectCore/ThirdParty`. Common plugins owned by the project baseline
live in `Assets/ProjectCore/Contexts/Template/Plugins`.

Third-party source code keeps its original namespaces and naming conventions.

## Dependency Direction

The intended dependency flow is:

```text
Domain -> .NET and Unity APIs only
Template -> Domain
Project -> Template + Domain + application-lifetime integrations
GameCore -> Template + Domain + Project services
Preloader -> Template + Domain + Project services + startup integrations
Gameplay contexts -> Template + Domain + Project services + GameCore
```

Rules:

- `Domain` does not depend on a context or an external API.
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

Because TroughGame uses one default Unity runtime assembly, these rules must be
validated with static dependency checks in addition to compilation.

## Runtime Entry Flow

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
    await GDPR flow, third-party SDKs, and other startup-only work
    ask ISceneFlowService to open the configured first gameplay scene
  unload Scene_Preloader and destroy all Preloader-scoped objects

Scene_Prototype (initial gameplay destination)
  PrototypeContextInstaller
    bind only scene-owned features
    compose shared GameCore features for the scene
  persistent SceneFlowService
    resolve IGameSceneLifecycle from the scene container
    FeatureInitializationFlow(Prototype features)
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
- The initial destination may be `Prototype` during development, but it must be
  selected by Preloader navigation rather than hard-coded as the application
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

Every feature has one of four architectural roles.

### Feature Groups

`FeatureGroup` is an explicitly ordered composition of related features. It
does not introduce a DI scope, own business logic, or replace a feature role.
Its child features are installed and initialized in declaration order.

`BaseFeatureGroup` composes code-created features. A group that owns serialized
references to MonoBehaviour features inherits `BaseMonoBehaviourFeatureGroup`,
is registered through `AddFeatureFromComponent<TFeatureGroup>()`, and adds those
children through explicit serialized references. Component searches are not
used for group composition.

Use a group when a context needs to keep a coherent set of related features
together, such as GameCore modules or a scene's implementation features. A
group may contain another group. Context installers compose top-level groups
and standalone features only.

### Module Features

Modules are reusable capabilities that can be consumed and implemented by
different implementation features. A module defines focused contracts and the
shared behavior needed to use that capability, but it does not know which game
entity or implementation feature consumes it.

Examples:

- `Movement`;
- `Combat`;
- `Health`;
- `NetworkEntity`.

Rules:

- A module never depends on an implementation feature.
- A module exposes narrow interfaces and data contracts through DI.
- A module must not assume that `Player`, `Bot`, or another concrete entity is
  its only consumer.
- Reusable module code belongs to `Features/Modules/<Feature>` in the context
  that owns its lifetime.

### Implementation Features

Implementation features provide a complete concrete game concept. They may
compose several modules, use one module, or be fully independent when no
reusable module is required. `Player`, for example, can implement movement,
combat, health, input, and network-entity module contracts.

Rules:

- An implementation feature may depend on module contracts.
- It owns the adapters, components, controllers, and state that implement those
  contracts for its concrete concept.
- One implementation feature must not reference another implementation
  feature directly.
- Shared behavior discovered inside an implementation must be extracted into a
  module instead of being consumed through a direct implementation dependency.

### Infrastructure Features

Infrastructure features are complete, reusable technical systems shared by
multiple features or contexts. Injection into other features, generic APIs,
and payload-based communication do not make a feature a bridge when it owns a
complete system and its lifecycle.

Examples:

- `AppTime`;
- `LocalConfig`;
- `Logging`;
- `Popup`;
- `ScreenNavigation`;
- `SignalBus`;
- `Audio`;
- `Save`;
- `Pooling`;
- `Localization`.

Rules:

- Reusable cross-context infrastructure normally belongs to
  `Template/Features/Infrastructure/<Feature>`.
- Infrastructure must not depend on concrete gameplay implementation features.
- Other features consume infrastructure through DI and narrow public contracts.
- Code ownership and runtime lifetime are separate: Template infrastructure may
  be installed into Project, Preloader, or scene containers as required.
- A complete reusable system remains Infrastructure even when every consumer
  injects it directly.
- `AppTime` is owned by Template Infrastructure but installed once in the
  persistent Project context. Consumers inject `IAppTimeService`; static access
  is forbidden. Its public time values use UTC, network synchronization is
  asynchronous, and local UTC time remains the fallback.
- `LocalConfig` is owned by Template Infrastructure and installed separately
  in every DI context that owns a flat local config catalog. Lookup checks the
  current context first and then parent contexts; nested provider catalogs and
  mutable global provider registries are forbidden. Its MonoBehaviour feature
  privately owns the serialized catalog and passes it directly to its service;
  the catalog is not bound in DI. Usage and lifetime details are documented in
  [`features/LOCAL_CONFIG.md`](features/LOCAL_CONFIG.md).
- `Logging` is owned by Template Infrastructure and installed once in the
  persistent Project context. Consumers use the injected `IDebugLogger` or a
  context-scoped `IDebugLogger<TFeature>`; log handlers connect through
  `ILogService`. Usage and lifetime details are documented in
  [`features/LOGGING.md`](features/LOGGING.md).
- `CommandLine` is independent Template Infrastructure installed before Debug
  Tools in the persistent Project context. It provides an immutable parsed
  argument snapshot and the dedicated-server cancel request. It remains
  available in every build type and is not controlled by Debug Tools.
- `DebugTools` is the persistent Template Infrastructure composition for
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
  VisualElement Views only render supplied data and forward user input.

### Bridge Features

Bridge features are small adapters that connect two otherwise independent
systems through their public contracts. They do not provide either connected
system and do not own business logic.

Examples include `PopupSignalBusBridge`, `SaveCloudBridge`, and
`AnalyticsNavigationBridge`. `SignalBus` itself is Infrastructure; a feature
that adapts popup events to that bus is a Bridge.

Rules:

- A bridge depends only on the public contracts of the systems it connects.
- A bridge must not resolve or call a concrete implementation feature.
- Signals and messages describe neutral domain or integration events, not the
  internal implementation of their publisher.
- Bridge features must stay small and must not become repositories for game
  logic.
- Cross-implementation communication uses a bridge contract; it never uses a
  concrete class reference.

The allowed feature dependency direction is:

```text
Domain <- Infrastructure <- Modules <- Implementations
              ^               ^              ^
              +---- Bridges ---+--------------+

Implementation A -X-> Implementation B
```

Bridges are installed by a composition root and adapt public contracts without
creating a direct dependency between concrete implementations. Cyclic feature
dependencies are forbidden.

> Folder taxonomy, suffix semantics, naming, member order, access modifiers,
> resource naming, and coding style are maintained in
> [`CONVENTIONS.md`](CONVENTIONS.md). This document defines architecture,
> ownership, dependency direction, lifecycle, and DI rules.

## Feature Structure

Project-owned features use the following layout:

```text
Assets/ProjectCore/Contexts/<Context>/Features/<Kind>/<Feature>/
  DataResources/
  GraphicResources/
    Materials/
    Prefabs/
  Scripts/
    Abstract/
    DataHolders/
      Configs/
      Data/
      Descriptors/
      DTOs/
      Payloads/
    Enums/
    Init/
      <Feature>Feature.cs
    Managers/
      Controllers/
      Coordinators/
      Factories/
      Flows/
      Handlers/
      Mediators/
      Providers/
      Registries/
      Repositories/
      Serializers/
      Services/
      Storages/
      Spawners/
      Systems/
    Other/
      Adapters/
      Builders/
      Commands/
      Converters/
      Decorators/
      Mappers/
      Processors/
      StateMachines/
      States/
      Strategies/
    Static/
      Constants/
      Errors/
      Extensions/
      Utilities/
      Validation/
    Views/
      Components/
      Navigation/
      Popups/
      Screens/
      Widgets/
```

Only create folders that the feature actually needs.

`Scripts` is a closed taxonomy. Its direct children may only be `Abstract`,
`DataHolders`, `Enums`, `Init`, `Managers`, `Other`, `Static`, and `Views`. Do
not put C# files directly in `Scripts`.

`DataHolders`, `Managers`, `Other`, `Static`, and `Views` are category
roots and also do not contain C# files directly. Their scripts must be placed
in one of the documented suffix-specific subfolders. Additional organizational
subfolders are allowed when they preserve the suffix rule.

Use the following decision order for every script:

1. Interface or abstract base type -> `Abstract`.
2. Enum -> `Enums`.
3. Feature composition or explicitly invoked initialization -> `Init`.
4. Test fixture or test-only support type -> the owning context's
   `Tests/Editor` or `Tests/Play`.
5. Any `static class` -> the matching `Static` subfolder.
6. Passive data-only type -> the matching `DataHolders` subfolder.
7. `MonoBehaviour` or a descendant -> the matching `Views` subfolder.
8. Non-MonoBehaviour behavioral object bound `AsSingle` in the owning DI
   container -> the matching `Managers` subfolder.
9. Any remaining feature-owned behavior -> a suffix-specific subfolder under
   `Other`.

The nature of the type takes precedence over how it happens to be bound. A
`MonoBehaviour` bound from the scene as a single instance remains a `View`; a
config instance passed through DI remains a `DataHolder`.

Every feature must have:

- a folder named after the feature;
- an explicit `Modules`, `Implementations`, `Infrastructure`, or `Bridges` role;
- a clear owner context;
- `Scripts/Init/<Feature>Feature.cs` when it participates in the feature
  lifecycle;
- feature-owned code and resources inside the feature folder;
- names that remain unambiguous within the context's flat namespace.

Examples:

```text
Contexts/GameCore/Features/Modules/Movement
Contexts/GameCore/Features/Implementations/Player
Contexts/Template/Features/Infrastructure/Popup
Contexts/Template/Features/Infrastructure/SignalBus
Contexts/Template/Features/Bridges/PopupSignalBusBridge
```

The role folders affect ownership and dependency direction but do not extend
the namespace.

### Strict Folder And Suffix Rule

Every concrete subfolder contains only scripts with its assigned suffix. A
suffix describes a type's responsibility and must not be selected merely to
make a file fit an existing folder.

Rules:

- One file contains one top-level type.
- The file name matches the top-level type exactly.
- Prefixes such as `I` and `Base` do not replace the role suffix:
  `IMovementService` and `BaseMovementSystem` are valid abstract names.
- Do not mix suffixes inside a folder. For example, `Managers/Services` cannot
  contain a `MovementFactory`.
- Do not create `Misc`, `Common`, `Helpers`, or `Utils` as escape-hatch folders.
- A new suffix and subfolder must be documented here before it is introduced.
- A type that changes from passive data to behavior must be renamed and moved
  to the folder matching its new responsibility.
- A feature-owned production `static class` is always placed under `Static`,
  never under `Managers`, `Other`, `DataHolders`, or `Views`. Context-owned
  shared static classes follow the direct context `Scripts/<Category>` rule.

### Managers

`Managers` contains behavioral, non-MonoBehaviour objects whose lifetime is
managed by DI with `AsSingle` in the owning container. `AsSingle` means one
instance per owning Project or Scene container, not necessarily one global
instance for the whole application.

| Folder | Required suffix | Role and usage |
| --- | --- | --- |
| `Controllers` | `Controller` | Coordinates one non-visual use case or translates input into calls to domain/module contracts. Visual MonoBehaviour controllers belong to `Views/Components`. |
| `Coordinators` | `Coordinator` | Orchestrates a multi-step workflow involving several services or systems. Application and scene flow coordinators belong here. |
| `Factories` | `Factory` | Creates injected objects or aggregates and hides construction details. The factory is a singleton even when the objects it creates are transient. |
| `Flows` | `Flow` | Executes one explicitly ordered, awaitable lifecycle or use-case pipeline. A Flow is invoked by its owner and never starts itself from a Unity callback. |
| `Handlers` | `Handler` | Handles one command, request, callback, or message contract. A handler should have one clear input responsibility. |
| `Mediators` | `Mediator` | Mediates several contracts without exposing their concrete implementations. Cross-feature mediation must still follow the Bridge rules. |
| `Providers` | `Provider` | Supplies a value, resource, environment capability, or strategy-selected implementation without owning the consumer workflow. |
| `Registries` | `Registry` | Maintains identity-to-instance or key-to-value registration and lookup for runtime objects. |
| `Repositories` | `Repository` | Provides an abstraction over stored or queryable collections and persistence boundaries. |
| `Serializers` | `Serializer` | Converts a selected data format to and from its runtime representation. |
| `Services` | `Service` | Exposes an application or feature capability to multiple consumers and owns the capability's state or operations. |
| `Storages` | `Storage` | Stores and retrieves raw data through a selected persistence mechanism. |
| `Spawners` | `Spawner` | Coordinates creation, network spawning, registration, and initial placement of runtime entities. |
| `Systems` | `System` | Executes ongoing runtime rules over entities, components, or feature state. Use it for runtime behavior rather than storage or external API access. |

Managers must not:

- inherit from `MonoBehaviour`;
- locate dependencies through scene searches or service locators;
- initialize themselves from constructors, `Start`, or background tasks;
- directly depend on another implementation feature.

Interfaces for Managers live in `Abstract`, not beside their implementations.
For example:

```text
Scripts/Abstract/IMovementService.cs
Scripts/Managers/Services/MovementService.cs
```

### DataHolders

`DataHolders` contains passive data representations. These types may define
constructors, serialization attributes, simple default values, and trivial
value equality, but they do not perform workflows, call services, load assets,
resolve dependencies, or mutate unrelated objects.

| Folder | Required suffix | Role and usage |
| --- | --- | --- |
| `Configs` | `Config` | Authored or loaded configuration values. A config may be a plain class or `ScriptableObject`, but it contains no runtime service behavior. |
| `Data` | `Data` | Internal feature state or a value bundle that does not represent a transport contract. Use the most specific name, such as `PlayerInputData`. |
| `Descriptors` | `Descriptor` | Immutable description of how another object is identified or processed. |
| `DTOs` | `DTO` | A transport representation used at a serialization, network, backend, or persistence boundary. Its shape follows the external contract. |
| `Payloads` | `Payload` | Immutable or short-lived parameters for a command, factory, spawn request, navigation request, or signal. |

Use the uppercase `DTO` suffix: `PlayerStateDTO`. The folder remains `DTOs`
because it names the category.

Data holders must not be used as disguised services. If a type has methods such
as `Load`, `Save`, `Spawn`, `Send`, `Update`, or `Initialize`, it normally does
not belong in `DataHolders`.

### Views

`Views` contains presentation and scene-facing scripts. Every type in this
folder must inherit from `MonoBehaviour`, directly or through a project base
class. Plain C# presenters, services, and controllers are not Views.

| Folder | Required suffix | Role and usage |
| --- | --- | --- |
| `Components` | `Component` | General scene or prefab behavior attached to a GameObject when no more specific View category applies. |
| `Navigation` | `NavigationView` | MonoBehaviour presentation for navigation controls, transitions, or scene/screen navigation state. Navigation services remain Managers. |
| `Popups` | `PopupView` | Popup presentation and serialized popup references. Popup control logic remains an injected Manager. |
| `Screens` | `ScreenView` | Full-screen or major panel presentation owned by a screen flow. |
| `Widgets` | `WidgetView` | Reusable, smaller UI presentation embedded in a screen or popup. |

View rules:

- Views receive runtime dependencies through DI.
- Serialized fields reference scene objects, prefabs, assets, and visual
  configuration; they are not service-locator substitutes.
- A View forwards user or Unity events to an injected contract and renders
  state. It does not own business rules.
- Unity callbacks may update local presentation but must not start feature or
  application initialization.

### Other

`Other` contains feature-owned behavior that is neither a DI `AsSingle`
Manager, passive data, a View, nor one of the special top-level categories.
Every type still lives in a suffix-specific subfolder.

| Folder | Required suffix | Role and usage |
| --- | --- | --- |
| `Adapters` | `Adapter` | Translates one API or contract into another, especially at external SDK boundaries. |
| `Builders` | `Builder` | Incrementally constructs a complex value or object description. |
| `Commands` | `Command` | Represents an executable operation with explicit inputs. Passive command parameters are Payloads instead. |
| `Converters` | `Converter` | Converts between two representations without owning persistence or workflow. |
| `Decorators` | `Decorator` | Wraps one contract to add behavior while preserving that contract. |
| `Mappers` | `Mapper` | Maps fields between domain, data, DTO, or View representations. |
| `Processors` | `Processor` | Applies one focused processing step, commonly as part of a state or data pipeline. |
| `StateMachines` | `StateMachine` | Owns transitions and the active state for a bounded behavior. |
| `States` | `State` | Implements one behavioral state used by a state machine. Passive stored state remains `Data`. |
| `Strategies` | `Strategy` | Encapsulates a replaceable algorithm selected by the caller or DI composition. |

If an `Other` object becomes a long-lived DI `AsSingle`, move it to the most
appropriate Managers role rather than keeping it under `Other`.

### Abstract

`Abstract` contains contracts and abstract foundations:

- interfaces use the `I` prefix and retain their semantic role suffix, such as
  `IMovementService`, `INetworkEntityFactory`, or `IPlayerDataAccessor`;
- abstract base classes use the `Base` prefix and retain their semantic suffix,
  such as `BaseMovementState` or `BaseNetworkEntityFactory`;
- concrete implementations are forbidden in this folder.

Manager contract interfaces reuse the Manager suffix definitions above. The
following additional contract suffixes are allowed:

| Suffix | Role and usage |
| --- | --- |
| `Accessor` | Exposes read-only access to feature or entity data. |
| `Mutator` | Exposes controlled mutation of data without exposing its storage implementation. Prefer it over the vague legacy suffix `Changer`. |
| `Contract` | Groups a cohesive capability only when no more specific semantic suffix exists. Use sparingly. |

An interface belongs to the feature that owns the contract, not automatically
to the feature that first implements it.

### Static

`Static` contains every production `static class` owned by the feature. Static
classes are separated by responsibility and may not be placed elsewhere merely
because another category has a similar name.

| Folder | Required suffix | Role and usage |
| --- | --- | --- |
| `Constants` | `Constants`, `Keys`, or `Defaults` | Compile-time constants, stable identifiers, documented defaults, and immutable `static readonly` values for one cohesive concept. |
| `Errors` | `Errors` | Static catalog of stable error definitions, codes, or factory methods. Runtime error state is not static. |
| `Extensions` | `Extensions` | Cohesive extension methods for one target concept. Avoid unrelated extension collections. |
| `Utilities` | `Utility` | Stateless reusable operations that cannot be expressed as extensions and do not require injected dependencies. |
| `Validation` | `Validation` | Stateless validation functions that return results without executing the validated workflow. |

Static rules:

- Every type under `Static` is declared `static`.
- Mutable global state, cached services, runtime registries, and service-locator
  access are forbidden.
- A static operation that needs an injected service must become a Manager or an
  `Other` strategy instead.
- Constants must not load configuration or assets.
- `Utility` is permitted only for a cohesive, specifically named responsibility;
  generic names such as `GameUtility` or `CommonUtility` are forbidden.

### Tests

Tests are owned by their context rather than stored inside production Feature
folders. They are separated only by execution environment:

```text
Contexts/<Context>/Tests/
  Editor/
    <Subject>Tests.cs
  Play/
    <Subject>Tests.cs
```

| Folder | Required suffix | Role and usage |
| --- | --- | --- |
| `Editor` | `Tests` | Fast Edit Mode tests for plain C# behavior, DI bindings, data conversion, validation, and editor-safe logic that does not require a running scene. |
| `Play` | `Tests` | Play Mode tests requiring Unity lifecycle, scenes, prefabs, MonoBehaviours, frame progression, or runtime integration. |

Test rules:

- Tests are placed directly in `Contexts/<Context>/Tests/Editor` or
  `Contexts/<Context>/Tests/Play`; do not create `Scripts/Tests` inside a
  Feature or nested production-category folders.
- Domain types do not have dedicated test fixtures. Test feature behavior and
  integration contracts instead of duplicating tests for `Domain` primitives.
- Test fixture files and fixture types end with `Tests`, not `Test`.
- Tests use the same flat context namespace as their production feature. The
  required `Tests` type suffix and the `Tests` path distinguish test code; test
  folders do not extend the namespace.
- Production code must not reference a test namespace or test-only type.
- Editor tests must not be used to hide logic that belongs in a Play test.
- Play tests must enter scenes through the supported flow when testing
  application initialization.
- Production code is compiled into `ProjectCore.Runtime`. Editor and Play tests
  use separate test assemblies and reference the runtime assembly explicitly.
- Test assemblies are excluded from normal player builds and must not be used
  as production dependencies.

### Enums

`Enums` contains only enum declarations, one enum per file. Enum names are
singular or plural and use a semantic suffix when it clarifies the value category:

| Suffix | Use |
| --- | --- |
| `Type` | Selects one category or implementation kind. |
| `State` | Represents a state-machine or lifecycle state. |
| `Status` | Represents the outcome or current condition of an operation. |
| `Mode` | Selects an operating mode that changes behavior. |
| `Reason` | Explains why an event, transition, or failure occurred. |

Do not append `Enum`. Prefer `MovementState` over `MovementStates` or
`MovementStateEnum`.

### Init

`Init` contains composition and lifecycle types that are invoked by the unified
initialization flow.

| Required suffix | Role and usage |
| --- | --- |
| `Feature` | Feature entry that declares its bindings and lifecycle participants. |
| `FeatureGroup` | Explicitly ordered composition of several features. It contains no feature behavior itself. |
| `Installer` | Zenject composition root that registers bindings only. |
| `Initializer` | Async initialization implementation explicitly invoked and awaited by the owning flow. |
| `EntryPoint` | Root trigger for a complete flow. `ApplicationEntryPoint` in Preloader is the only application entry point. |

No `Init` type may start an independent initialization chain from `Awake` or
`Start`. A scene `Initializer` is called by the persistent game-flow
coordinator after its installer has completed.

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
- Implementation features satisfy module contracts through DI bindings.
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
// Assets/ProjectCore/Domain/**
namespace Domain;

// Assets/ProjectCore/Contexts/Template/**
namespace ProjectCore.Template;

// Assets/ProjectCore/Contexts/Project/**
namespace ProjectCore.Project;

// Assets/ProjectCore/Contexts/Preloader/**
namespace ProjectCore.Preloader;

// Assets/ProjectCore/Contexts/GameCore/**
namespace ProjectCore.GameCore;

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

### Code

- Types, methods, properties, events, and namespaces use `PascalCase`.
- Interfaces start with `I`.
- Abstract base types start with `Base` when the prefix clarifies their role.
- Private instance fields use `_camelCase`.
- A C# file name must match its primary type name.
- Feature entry points use `<Feature>Feature`.
- Context composition roots use `<Context>ContextInstaller`; explicitly invoked
  scene initialization contracts use `<Context>ContextInitializer`.
- The persistent composition root uses `ProjectContextInstaller`. It binds the
  Project initialization contracts but does not start a second bootstrap flow.
- `ApplicationEntryPoint` is the only application bootstrap type.
- Avoid duplicated segments such as `Prototype.Prototype`.
- Feature-owned types should start with the feature or domain term when a short
  name would be ambiguous in the flat context namespace. Examples:
  `CombatDamageableSystem`, `MovementStateMachine`, `NetworkEntitySpawner`,
  and `PlayerSpawnController`.
- Names must describe responsibility rather than implementation details such
  as `Manager`, `Helper`, or `Utils`, unless the type genuinely represents a
  broad utility API.

Spelling mistakes are not accepted in paths or identifiers. In particular,
legacy names must be corrected as follows:

```text
Proyotype -> Prototype
Enteties  -> Entities
Paterns   -> Patterns
```

### Folders

Use the canonical folder names below consistently:

```text
Abstract
Components
Configs
Constants
Controllers
Data
DataResources
DTOs
Enums
Features
GraphicResources
Init
Materials
Mediators
Other
Prefabs
Resources
Scenes
Scripts
Serializers
Services
Storages
StateMachines
Systems
UserData
Views
```

### Unity Assets

- Scenes use `Scene_<Context>[_<Detail>]`.
- Prefabs use `Prefab_<Feature>_<Purpose>`.
- Materials use `Material_<Purpose>`.
- ScriptableObject asset files start with `Config_`.
- `CreateAssetMenu.fileName` follows
  `Config_<Context>_<Feature>_<Detail>`.
- `CreateAssetMenu.menuName` follows
  `SO/<Context>/<Feature>/<Detail>`.

Example:

```csharp
[CreateAssetMenu(
    fileName = "Config_GameCore_Movement_Locomotion",
    menuName = "SO/GameCore/Movement/Locomotion")]
public sealed class LocomotionMovementConfig : ScriptableObject
{
}
```

When renaming Unity assets, move their `.meta` files with them. When renaming
serialized fields or managed-reference types, use the applicable migration
attributes such as `FormerlySerializedAs` or `MovedFrom`.

## Coding Rules

- Keep business rules in services or domain objects.
- Keep Unity references and presentation changes in components, systems,
  controllers, and views.
- Services must not depend directly on views.
- Expose shared services through interfaces when multiple consumers require a
  stable contract.
- Use UniTask for asynchronous context and feature workflows; do not use it in
  `Domain`.
- Register scene-lifetime features in the owning context installer.
- Register application-lifetime features through `ProjectContextInstaller` or
  a feature group owned by the Project context.
- Register startup-only features through `PreloaderContextInstaller`; never
  resolve Preloader-scoped instances from a later gameplay scene.
- Keep project-wide content in `ContentData`.
- Keep feature-owned data and presentation assets inside the feature.
- Do not commit passwords, tokens, credentials, or keystores.

## Architecture Validation

Run the project-owned validator from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -Mode Report
```

`Report` mode prints all diagnostics but returns a successful exit code. Use
`-SummaryOnly` for counts and `-OutputPath` for a JSON report:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -Mode Report `
  -SummaryOnly `
  -OutputPath Logs\ArchitectureValidation.json
```

After legacy violations are removed, CI and local verification use strict mode:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -Mode Strict `
  -SummaryOnly
```

`Strict` returns a non-zero exit code when an Error diagnostic exists. Warnings
identify heuristic checks that require review and do not fail strict mode.

The validator checks:

- path ownership and legacy roots;
- context and Domain namespaces;
- external dependencies in Domain;
- feature kind and `Scripts` taxonomy;
- folder/suffix correspondence;
- static, Manager, View, enum, abstract, Init, and test type rules;
- known spelling and legacy suffix errors;
- file/type correspondence and one top-level type per file;
- suspicious `Awake`, `OnEnable`, `Start`, and `async void` initialization.

The validator intentionally reports possible lifecycle and DI issues as
Warnings when static source inspection cannot prove them. Compilation, tests,
and manual architecture review remain required.

Detailed rule families and the positive regression fixture are documented in
[`Tools/Architecture/README.md`](../Tools/Architecture/README.md).
