# Project Conventions

This document defines the coding, naming, and physical organization rules for
TroughGame. Architecture ownership and dependency direction are defined in
[`ARCHITECTURE_RULES.md`](ARCHITECTURE_RULES.md).

## Source Layout

Project code is organized below `Assets/ProjectCore`:

```text
Assets/ProjectCore/
  ContentData/
  Contexts/<Context>/
    GraphicResources/
    Resources/
    Scripts/                 # context-shared scripts, no feature lifecycle
    Features/<Role>/<Feature>/
  Domain/
    Attributes/
      Editor/
  ThirdParty/
```

`Template` is project-agnostic reusable code. `Domain` is independent of
external APIs and may reference only .NET and Unity Engine/Editor APIs.

Zenject's `Assets/ProjectCore/Resources/ProjectContext.prefab` is the explicit
root-layout exception. It keeps both its framework-required location and name.

Context-shared scripts that do not require a feature or DI initialization may
live in `Contexts/<Context>/Scripts/<Category>` (for example extensions,
utilities, constants, or networking abstractions). They must still follow the
same naming and suffix rules.

## Feature Layout

```text
<Feature>/
  DataResources/             # feature-owned data assets and JSON
  GraphicResources/          # feature-owned visual assets
    Materials/
    Prefabs/
  Scripts/
    Abstract/
    DataHolders/
      Configs/ Data/ DTOs/ Payloads/
    Enums/
    Init/
    Managers/
      Controllers/ Coordinators/ Factories/ Flows/ Handlers/
      Mediators/ Providers/ Registries/ Repositories/ Services/
      Spawners/ Systems/
    Other/
      Adapters/ Builders/ Commands/ Converters/ Decorators/
      Mappers/ Processors/ StateMachines/ States/ Strategies/
    Static/
      Constants/ Errors/ Extensions/ Utilities/ Validation/
    Tests/
      Editor/<ProductionCategory>/
      Play/<ProductionCategory>/
    Views/
      Components/ Navigation/ Popups/ Screens/ Widgets/
```

Create only folders that are needed. `Scripts` and every category root are
closed: they contain no C# files directly, only their documented subfolders.
Every concrete subfolder contains only scripts with the corresponding suffix.
Additional organizational subfolders are allowed below a category root when
they preserve the suffix rule. One file contains one top-level type and the
file name exactly matches it.

Every feature that participates in the lifecycle has
`Scripts/Init/<Feature>Feature.cs`. A feature may be independent, may compose
modules, or may be reusable infrastructure/bridge as defined in the
architecture rules.

## Script Categories and Suffixes

### Abstract

Contracts and abstract foundations only. Interfaces use `I` plus a semantic
suffix (`IMovementService`); abstract bases use `Base` plus a semantic suffix
(`BaseNetworkStateMachine`). Concrete implementations do not belong here.

### Managers

Non-`MonoBehaviour` behavioral objects registered as DI `AsSingle` in their
owning container. Interfaces belong in `Abstract`.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Controllers | `Controller` | Coordinates one focused use case or input translation. |
| Coordinators | `Coordinator` | Orchestrates a multi-step workflow across services/systems. |
| Factories | `Factory` | Creates objects or aggregates and hides construction. |
| Flows | `Flow` | Executes one explicitly ordered, awaitable pipeline. |
| Handlers | `Handler` | Handles one command, request, callback, or message. |
| Mediators | `Mediator` | Mediates several contracts without exposing implementations. |
| Providers | `Provider` | Supplies a value, resource, capability, or strategy. |
| Registries | `Registry` | Maintains keyed runtime registrations and lookup. |
| Repositories | `Repository` | Abstracts stored/queryable collections or persistence. |
| Services | `Service` | Exposes a capability and owns its state/operations. |
| Spawners | `Spawner` | Creates, registers, and places runtime entities. |
| Systems | `System` | Applies ongoing runtime rules to entities or state. |

Managers must not use service locators, scene searches, or self-starting Unity
callbacks. A long-lived object that is not DI `AsSingle` belongs elsewhere.

### DataHolders

Passive data only. They may contain constructors, serialization attributes,
defaults, and trivial equality, but must not resolve dependencies, call
services, load assets, or execute workflows.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Configs | `Config` | Authored or loaded configuration values. |
| Data | `Data` | Internal feature state or value bundles. |
| DTOs | `DTO` | Serialization, network, backend, or persistence boundary shape. |
| Payloads | `Payload` | Parameters for commands, factories, navigation, or signals. |

Use the uppercase `DTO` abbreviation in type names, for example
`PlayerStateDTO`. Types with `Load`, `Save`, `Spawn`,
`Send`, or `Initialize` behavior are not data holders.

### Views

Every type below `Views` must inherit from `MonoBehaviour`. Views render state,
forward Unity/user events to injected contracts, and contain no business rules.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Components | `Component` | General scene/prefab behavior. |
| Navigation | `NavigationView` | Navigation controls and transitions. |
| Popups | `PopupView` | Popup presentation and serialized references. |
| Screens | `ScreenView` | Full-screen or major panel presentation. |
| Widgets | `WidgetView` | Reusable embedded presentation. |

### Other

Feature-owned non-`MonoBehaviour` behavior that is not a DI singleton, data,
view, or special category.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Adapters | `Adapter` | Translates one contract/API to another. |
| Builders | `Builder` | Incrementally constructs a complex value/object. |
| Commands | `Command` | Executable operation with explicit inputs. |
| Converters | `Converter` | Converts between representations. |
| Decorators | `Decorator` | Wraps a contract while preserving it. |
| Mappers | `Mapper` | Maps fields between model/data/DTO/view forms. |
| Processors | `Processor` | Performs one focused pipeline step. |
| StateMachines | `StateMachine` | Owns bounded transitions and active state. |
| States | `State` | One behavior state used by a state machine. |
| Strategies | `Strategy` | Replaceable algorithm selected by caller or DI. |

Do not create `Misc`, `Common`, `Helpers`, or `Utils` escape-hatch folders.

### Static

Every type in `Static` is a `static class` with no mutable global runtime
state, cached services, or service-locator access.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Constants | `Constants`, `Keys`, or `Defaults` | Stable identifiers, compile-time defaults, and immutable `static readonly` values. |
| Errors | `Errors` | Stable error definitions, codes, or factories. |
| Extensions | `Extensions` | Cohesive extension methods for one target concept. |
| Utilities | `Utility` | Stateless cohesive operations that are not extensions. |
| Validation | `Validation` | Stateless validation returning a result. |

Immutable `static readonly` values are allowed in `Constants`; mutable global
runtime state and cached services are not. If a static operation needs an
injected service, make it a Manager or an `Other` strategy instead.

### Tests

Test fixtures end with `Tests` and are stored directly under `Tests/Editor` or
`Tests/Play`; nested test-category folders are not used. Editor tests cover
feature services, DI bindings, and editor-safe logic. Play tests cover scenes,
prefabs, Unity lifecycle, frame progression, and runtime integration. Domain
primitives do not receive dedicated test fixtures. Test assemblies reference
the runtime assembly and are never production dependencies.

### Enums

`Enums` contains only enum declarations, one enum per file. Enum names may be
singular or plural; use the form that best communicates the project concept.
Do not append `Enum`. Optional semantic suffixes are `Type`, `State`, `Status`,
`Mode`, and `Reason`.

### Init

`Init` contains composition and explicitly invoked lifecycle types:

| Suffix | Meaning |
| --- | --- |
| `Feature` | Feature entry and lifecycle participants. |
| `FeatureGroup` | Explicitly ordered composition of features. |
| `Installer` | Zenject composition root; registers bindings only. |
| `Initializer` | Async initialization invoked and awaited by a flow. |
| `EntryPoint` | Root trigger for one complete application flow. |

No `Init` type starts an independent chain from `Awake`, `OnEnable`, or
`Start`. `ApplicationEntryPoint.Start()` is the sole Unity-to-application
startup boundary.

## Naming

- Types, methods, properties, events, namespaces: `PascalCase`.
- Interfaces: `I` prefix (`IPlayerService`).
- Abstract bases: `Base` prefix when useful (`BaseNetworkStateMachine`).
- Private instance and static fields: `_camelCase`.
- Constants: `PascalCase` (`DefaultTimeout`), not screaming snake case.
- Local variables and parameters: `camelCase`.
- Established abbreviations may preserve their conventional uppercase form,
  including `UI`, `HUD`, `ID`, `DTO`, `FOV`, and `FPS`. Apply the same form
  consistently across related fields, properties, methods, and types.
- Boolean members use `Is`, `Has`, `Can`, or `Should` prefixes.
- Async methods end with `Async`; cancellation tokens end with
  `CancellationToken` and are passed explicitly.
- Event names use past-tense notifications (`Initialized`, `SceneLoaded`).
- File name equals primary type name; one top-level type per file.
- Generic type files may append `T` to the primary type name when the generic
  arity cannot be represented in the file name, for example `ResultT.cs` for
  `Result<TValue>`.
- Feature entries use `<Feature>Feature`; context installers use
  `<Context>ContextInstaller`; context initializers use
  `<Context>ContextInitializer`.
- Avoid duplicated segments and vague names such as `Manager`, `Helper`, or
  `Utils` unless they are the documented role/suffix.
- Correct spelling in identifiers and paths; do not preserve legacy typos.

Namespaces are flat below the owning context: `ProjectCore.<Context>` or
`Domain`. Folder depth and feature role do not extend the namespace. Third-party
code keeps its original namespace.

## Member Order

Use this order inside classes, structs, and interfaces:

1. Constants and static fields.
2. Instance fields (serialized fields first, then private fields).
3. Constructors and Zenject injection methods.
4. Public properties and events.
5. Unity lifecycle methods (`Awake`, `OnEnable`, `Start`, `Update`, `OnDestroy`).
6. Public methods.
7. Protected methods.
8. Private methods.
9. Nested types.

Keep related overloads together. Do not use regions to hide unrelated members.

## Access Modifiers and API Surface

- Always write an explicit access modifier; no implicit class/member visibility.
- Prefer the narrowest visibility: `private` by default, then `protected`,
  `internal`, and `public` only for a required contract.
- Use `public` interfaces for DI contracts and public operations consumed by
  other features; keep implementations and helpers private/internal.
- Prefer `private` fields with public read-only properties when exposure is
  required. Avoid public mutable fields and public setters.
- Use `internal` for assembly-local implementation details; use
  `InternalsVisibleTo` only for the dedicated test assembly when necessary.
- Seal concrete classes unless inheritance is an intentional extension point.
- Prefer constructor injection for non-`MonoBehaviour` classes and DI methods
  or explicit injection for Unity components. Do not resolve from a container
  inside ordinary business code.

## C# and Unity Coding Rules

- Use `using` directives only when a type from the namespace is referenced;
  remove unused usings.
- Keep business rules in domain/services, Unity references in views/components,
  and external SDK access behind adapters/providers.
- Use UniTask for asynchronous runtime/initialization workflows; Domain must
  not depend on UniTask.
- Do not start application work from `Awake`, `OnEnable`, or arbitrary `Start`.
- Avoid `async void`; use `UniTask`, `UniTask<T>`, or `UniTaskVoid` only for a
  Unity event boundary where required.
- Pass cancellation tokens through async call chains and dispose linked token
  sources in the owner of the flow.
- Prefer guard clauses, immutable payloads, and explicit null/error handling.
- Keep methods focused; extract a named type when behavior requires a new
  responsibility rather than adding a generic helper.
- Do not commit credentials, tokens, passwords, keystores, or generated build
  artifacts.

## Unity Resource Naming

- Scenes: `Scene_<Context>[_<Detail>]`.
- Prefabs: `Prefab_<Feature>_<Purpose>`.
- Zenject's `ProjectContext.prefab` is the naming exception and keeps its
  framework-required name.
- Materials: `Material_<Purpose>`.
- ScriptableObject assets: `Config_<Context>_<Feature>_<Detail>`.
- `CreateAssetMenu.menuName`: `SO/<Context>/<Feature>/<Detail>`.
- Keep feature-owned assets inside `GraphicResources` or `DataResources`.
- Keep cross-feature project content in `ContentData`.
- Move Unity `.meta` files together with renamed assets.
- Use `FormerlySerializedAs` or `MovedFrom` when serialized fields/types are
  renamed.
