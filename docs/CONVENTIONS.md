# Project Conventions

This document defines the coding, naming, and physical organization rules for
TroughGame. Architecture ownership and dependency direction are defined in
[`ARCHITECTURE_RULES.md`](ARCHITECTURE_RULES.md).

## Source Layout

Project code is organized below `Assets/ProjectCore`. The context and root layout
is maintained in
[`ARCHITECTURE_RULES.md`, Architecture](ARCHITECTURE_RULES.md#architecture).
This document defines the physical layout inside those owners and the
type/resource naming conventions.

`Template` is project-agnostic reusable code. `Shared` is independent of
external APIs and may reference only .NET APIs. Shared Unity property
attributes, drawers, state foundations, and mediator handlers belong to
`Template/Scripts/DataHolders/Attributes`, `Other/Editor`, `Abstract`, and
`Components/Handlers` respectively.

Zenject's `Assets/ProjectCore/Resources/ProjectContext.prefab` is the explicit
root-layout exception. It keeps both its framework-required location and name.

Context-shared scripts that do not require a feature or DI initialization may
live in `Contexts/<Context>/Scripts/<Category>` (for example extensions,
utilities, constants, or networking abstractions). They must still follow the
same naming and suffix rules.

`Scripts/Navigation` is allowed for context-owned scene route types and their
definitions; it contains only `*Scene` and `*SceneDefinition` scripts.

The shared Template initialization framework uses the closed structure
`Scripts/Initialization/Abstract`, `Init`, `Managers/Flows`, and
`Other/Adapters`. `Other/Adapters` contains only non-DI `*Adapter` objects that
adapt reusable feature-framework behavior to its external contracts.

## Feature Layout

```text
<Feature>/
  Assets/                   # non-C# resources owned by the feature
    Configs/ Catalogs/ Definitions/
    Prefabs/ Materials/ Sprites/ Fonts/ Animations/ InputActions/ Scenes/ References/
    UI/
      UXML/ USS/ PanelSettings/
  Scripts/
    Abstract/
    DataHolders/
      Attributes/ Configs/ Data/ Definitions/ Descriptors/ DTOs/ Models/ Payloads/ Settings/
    Enums/
    Init/
    Managers/
      Accumulators/ Controllers/ Coordinators/ Factories/ Flows/ Handlers/
      Mediators/ Providers/ Registries/ Repositories/ Serializers/ Services/ Storages/
      Spawners/ Systems/
    Other/
      Adapters/ Builders/ Commands/ Converters/ Decorators/ Editor/
      Mappers/ Presenters/ Processors/ StateMachines/ States/ Strategies/ ViewModels/
    Static/
      Constants/ Errors/ Extensions/ Utilities/ Validation/
    Components/
      <Name>Component.cs
      Navigation/ Popups/ Screens/ StateMachines/ States/ Widgets/
```

Create only folders that are needed. `Scripts` and every category root are
closed, except for `Abstract`, `Enums`, `Init` and ordinary `*Component` files
directly in `Scripts/Components`. Other files use documented subfolders.
Every concrete subfolder contains only scripts with the corresponding suffix.
Additional organizational subfolders are allowed below a category root when
they preserve the suffix rule. One file contains one top-level type and the
file name exactly matches it.

Every feature that participates in the lifecycle has
`Scripts/Init/<Feature>Feature.cs`. All features live directly under their
context's `Features` folder. Features may provide reusable capabilities,
compose other capabilities, or adapt public contracts; these responsibilities
do not introduce feature types or intermediate role folders.

Feature entries inherit `BaseFeature` by default. `BaseMonoBehaviourFeature` is
allowed only when the feature itself must own serialized scene, prefab, or
asset references. Such a component lives on the same GameObject as the context
installer and is added with `AddFeatureFromComponent<TFeature>()`; it follows
the same explicit initialization flow and does not use `Awake` or `Start`.
Both bases delegate their lifecycle state and DI operations to the shared
`FeatureLifecycleAdapter`; new feature helpers must be implemented there
instead of duplicated between the two bases.

## Script Categories and Suffixes

### Abstract

Contracts and abstract foundations only. Interfaces use `I` plus a meaningful
capability or subject name (`IMovementService`, `IScene`). Abstract bases use
`Base` plus a semantic suffix (`BaseNetworkStateMachine`). Concrete
implementations do not belong here.

An interface describes the capability available to its consumer; the concrete
type describes its implementation role or mechanism. Their suffixes need not
match: `ILocalInputReader` is implemented by `LocalInputProvider`,
`ISceneTransitionPresenter` by `SceneLoadingScreenAdapter`, and `IDebugLogger`
by `DebugLoggerService`. One implementation may satisfy several contracts.

Manager role names may also name interfaces. Additional established contract
roles are defined below; they do not introduce folders or concrete type
categories. All feature contracts remain in the owning feature's `Abstract`.

| Role | Responsibility and boundary | Existing example |
| --- | --- | --- |
| `Accessor` | Reads state without exposing mutation or its storage implementation. | `IEnemyTargetAccessor` |
| `Mutator` | Provides controlled changes to state; may also expose reading. Use instead of the legacy `Changer`. | `IEnemyTargetMutator` |
| `Reader` | Obtains input or source values; may capture a snapshot before reading. Does not provide arbitrary writes to that source. | `ILocalInputReader` |
| `Logger` | Accepts diagnostic messages at defined severity levels; does not expose log storage or UI management. | `IDebugLogger` |
| `Probe` | Performs a bounded observation/query and reports its result; does not own the workflow that acts on it. | `IMovementObstacleProbe` |
| `Presenter` | Shows or hides a particular presentation for a caller; does not own the caller's application flow. | `ISceneTransitionPresenter` |
| `Contributor` | Supplies behavior or registrations to composition owned by another object; does not become that composition's coordinator. | `IEnemyMovementAbilityContributor` |
| `Backend` | Implements the operations and lifecycle behind a public facade; consumers normally use the narrower public contract. | `IDebugVisualizationBackend` (internal) |
| `Drawable` | Supplies drawing commands through the provided drawing context; does not own rendering infrastructure. | `IDebugDrawable` |
| `Collection` | Exposes a cohesive typed group and its queries; use an explicit mutation contract if registration or removal is needed. | `IEnemyAbilityCollection` |
| `Contract` | Groups a cohesive capability only when neither a subject name nor a more precise role describes it. Use sparingly. | Fallback for a new capability, not a required suffix. |

Subject and marker interfaces may omit a role suffix when the subject itself
is clear: `IScene`, `ICameraTarget`, `ILocalCamera`, and `IEnemyAbility`.
Do not append `Service` or `Contract` solely to match a suffix table.

`Mutator` may inherit `Accessor` when its consumers also need the read API,
as in `IEnemyTargetMutator : IEnemyTargetAccessor` and
`IStateDataMutator<TStatesType> : IStateDataAccessor<TStatesType>`. Inheritance
is a capability decision, not a requirement for every mutator. An interface
belongs to the feature that owns the contract, regardless of where it is
first implemented.

The `Base` prefix also applies to top-level abstract classes in other context
categories, including Components and DataHolders. The four generic definition
families are the explicit exception: `ScreenDefinition<TScreen, TSettings>`,
`SceneDefinition<TScene, TSettings>`,
`PopupDefinition<TPopup, TPayload, TResponse>`, and
`LoadingScreenDefinition<TScreen, TSettings>`. They retain their names and
inherit the corresponding non-generic `Base*Definition` in their owning
Template features. Shared primitives follow their existing independent naming
conventions; this context-category rule does not rename `HandlerPayload`.

### Script Placement

Choose the folder by the type's responsibility, in this order:

1. Test fixture or test-only support type: the owning context's Tests/Editor
   or Tests/Play.
2. Interface or abstract contract foundation: Abstract. An abstract base that
   already has a concrete category, such as a MonoBehaviour state or a
   ScriptableObject definition, may remain in that category; it still follows
   the Base naming rule and its documented exceptions.
3. Enum: Enums.
4. Composition or explicitly invoked initialization: Init.
5. Static class: the matching Static subfolder.
6. Passive data, including authored configs: the matching DataHolders subfolder.
7. MonoBehaviour or VisualElement: the matching Components location.
8. Behavioral object that is neither MonoBehaviour nor VisualElement and has
   a DI-managed lifetime: the matching Managers subfolder.
9. Remaining feature-owned behavior: the matching Other subfolder.

The type's nature takes precedence over its binding: a scene component bound
as one instance remains a Component, and a config passed through DI remains a
DataHolder. Rename and move a type when its responsibility changes. Do not
introduce a suffix solely to fit an existing folder.

### Managers

Non-`MonoBehaviour` behavioral objects registered as DI `AsSingle` in their
owning container. Interfaces belong in `Abstract`. The existing open-generic
feature logger is the documented AsCached exception; its registration and
lifetime are defined in
[`ARCHITECTURE_RULES.md`, Managers](ARCHITECTURE_RULES.md#managers).

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Accumulators | `Accumulator` | Collects transient values or transitions until an explicit read/consume boundary. |
| Controllers | `Controller` | Coordinates one focused use case or input translation. |
| Coordinators | `Coordinator` | Orchestrates a multi-step workflow across services/systems. |
| Factories | `Factory` | Creates objects or aggregates and hides construction. |
| Flows | `Flow` | Executes one explicitly ordered, awaitable pipeline. |
| Handlers | `Handler` | Handles one command, request, callback, or message. |
| Mediators | `Mediator` | Mediates several contracts without exposing implementations. |
| Providers | `Provider` | Supplies a value, resource, capability, or strategy. |
| Registries | `Registry` | Maintains keyed runtime registrations and lookup. |
| Repositories | `Repository` | Abstracts stored/queryable collections or persistence. |
| Serializers | `Serializer` | Serializes and deserializes a selected data format. |
| Services | `Service` | Provides a specific capability through operations and queries; may own state and resources needed for that capability. |
| Storages | `Storage` | Stores and retrieves raw data through a selected persistence mechanism. |
| Spawners | `Spawner` | Creates, registers, and places runtime entities. |
| Systems | `System` | Owns the operation of a runtime subsystem: its active objects, state, interaction rules, transitions, or lifecycle. |

Managers must not use service locators, scene searches, or self-starting Unity
callbacks. A long-lived object that is not DI `AsSingle` belongs elsewhere.

#### System and Service

Choose the suffix by the responsibility exposed to consumers:

- Use `System` for the owner of a runtime subsystem. For example,
  `ScreenNavigationSystem` maintains screen history and manages screen
  transitions and lifecycle; `PopupSystem` manages active windows, completion,
  cancellation, and input blocking. A subsystem may have one active object,
  as in `LoadingScreenSystem`, and may delegate work to components, as in
  `DebugVisualizationSystem`.
- Use `Service` for a specific capability invoked through operations or
  queries. For example, `EnemySteeringService` calculates steering,
  `EnemyTargetingService` selects targets, and `LocalSaveService` provides
  save/load operations through storage and serializer contracts. The service
  may keep state, subscriptions, buffers, or resources for that capability.

These roles can overlap. When both descriptions fit, choose the name that
best explains the primary API and responsibility to consumers. Preserve an
established, clear name unless it misrepresents that responsibility;
`SceneFlowService` remains valid for its scene-transition capability even
though it also owns transition state and scene lifecycle. This convention
does not require every feature to have a System or a Service, or require both
roles to coexist in separate classes.

The following traits do not determine the suffix:

- State, collections, `Initialize`, `Dispose`, events, or asynchronous methods
  are valid for both roles.
- Per-frame or network-tick invocation does not require `System`.
  `MovementSimulationService.Simulate` operates on a supplied body even when
  called from `NetworkTick`. A System may operate through explicit commands
  and events without its own tick method.
- Unity API access does not require `System`. `LevelCollisionService` uses
  Physics2D through its Strategy and Probe contracts. Only `Shared` has the
  .NET-only dependency restriction; Services still follow the architecture's
  dependency rules and must not depend directly on Views.
- Class size and the feature's informal description as a "system" do not
  determine the role.

`System` does not imply Unity ECS or a new scheduler; `Service` does not imply
stateless or pure C# behavior. Both follow the existing Managers DI and
explicit initialization rules. Their suffixes do not establish dependency
direction or a mandatory System-to-Service hierarchy; ownership and public
contracts determine those dependencies.

A class that only forwards generic component queries or caller-supplied
actions does not acquire subsystem rules from a `System` name. Describe the
capability it actually provides, or remove a redundant wrapper after checking
its consumers. Role selection is a code-review decision; the architecture
validator checks folder/suffix correspondence, not these behavioral semantics.

### DataHolders

Passive data only. They may contain constructors, serialization attributes,
defaults, and trivial equality, but must not resolve dependencies, call
services, load assets, or execute workflows.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Attributes | `Attribute` | Declarative metadata owned by one feature. |
| Configs | `Config` | Authored or loaded configuration values. |
| Data | `Data` | Internal feature state or value bundles. |
| Definitions | `Definition` | Authored registration of a typed project resource or route. |
| Descriptors | `Descriptor` | Immutable description of how another object is identified or processed. |
| DTOs | `DTO` | Serialization, network, backend, or persistence boundary shape. |
| Models | `Model` | Long-lived runtime state representation owned by a feature. |
| Payloads | `Payload` | Parameters for commands, factories, navigation, or signals. |
| Settings | `Settings` | Immutable settings supplied for an object or context lifetime. |

Use the uppercase `DTO` abbreviation in type names, for example
`PlayerStateDTO`. Types with `Load`, `Save`, `Spawn`,
`Send`, or `Initialize` behavior are not data holders.

The final suffix identifies the type's role; the preceding words describe its
contents. `DebugToolsSettingsConfig` is an authored `Config` containing
settings, while `DebugConsoleSettings` is the settings value it supplies.
`LocalConfigCatalogConfig` is a `Config` containing a catalog of configuration
references. `SettingsConfig` and `CatalogConfig` therefore retain the final
`Config` and live in `DataHolders/Configs`; they do not introduce new suffixes
or folders. Resource instances follow Unity Resource Naming below.

### Components

`Components` contains Unity-facing C# types. Every component type must inherit from
`MonoBehaviour` or `UnityEngine.UIElements.VisualElement`. Presentation Components
render state and forward Unity/user events to injected contracts. Unity-bound
state machines and states own only their bounded state-processing role and
delegate reusable operations to processors, services, or systems. UXML, USS,
textures, fonts, and other non-C# presentation assets belong in the feature's
`Assets`.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Root of Components | `Component` | `MonoBehaviour` scene or prefab behavior; no nested Components folder. |
| Navigation | `NavigationView` | Navigation controls and transitions. |
| Popups | `PopupView` | Popup presentation and serialized references. |
| Screens | `ScreenView` | Full-screen or major panel presentation; may inherit `MonoBehaviour` or `VisualElement`. |
| StateMachines | `StateMachine` | `MonoBehaviour` state machine attached to a scene object or prefab. |
| States | `State` | `MonoBehaviour` state attached to a scene object or prefab. |
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
| Editor | `Editor`, `EditorWindow`, or `AttributeDrawer` | Editor-only inspectors/tooling, windows, or Unity property drawers respectively. The same suffixes apply to context-owned and feature-owned scripts. |
| Mappers | `Mapper` | Maps fields between model/data/DTO/view forms. |
| Presenters | `Presenter` | Plain C# per-view presentation owner in approved MVP; explicitly updates a narrow View contract. |
| Processors | `Processor` | Performs one focused pipeline step. |
| StateMachines | `StateMachine` | Owns bounded transitions and active state. |
| States | `State` | One behavior state used by a state machine. |
| Strategies | `Strategy` | Replaceable algorithm selected by caller or DI. |
| ViewModels | `ViewModel` | Plain C# presentation behavior owned by one view instance; constructor-injected dependencies do not make it a container-lifetime Manager. |

ViewModels and Presenters follow [the runtime UI standard](UI_TOOLKIT_STANDARD.md). They are not
passive `DataHolders/Models`, `MonoBehaviour`/`VisualElement` components, or scene
singletons. Their contracts and abstract bases belong in `Scripts/Abstract`.
MVVM is the default; Presenter placement does not itself approve the MVP exception.

`MonoBehaviour` state machines and states belong in `Components/StateMachines` and
`Components/States`; `Other/StateMachines` and `Other/States` contain only plain C#
implementations.

Do not create `Misc`, `Common`, `Helpers`, or `Utils` escape-hatch folders.

### Static

Every type in `Static` is a `static class` with no mutable global runtime
state, cached services, or service-locator access.

An architecture-documented static facade may keep one replaceable reference
to its DI-owned backend for call-site convenience. It must use a no-op backend
while detached and must not own runtime collections, Unity objects, timers, or
the backend lifecycle. This exception currently applies to Debug
Visualization only.

| Folder | Suffix | Meaning |
| --- | --- | --- |
| Constants | `Constants`, `Keys`, or `Defaults` | Stable identifiers, compile-time defaults, and immutable `static readonly` values. |
| Errors | `Errors` | Stable error definitions, codes, or factories. |
| Extensions | `Extensions` | Cohesive extension methods for one target concept. |
| Utilities | `Utility` | Stateless cohesive operations that are not extensions. |
| Validation | `Validation` | Stateless validation of inputs and configuration. |

A Utility must have a cohesive, specifically named responsibility; generic
names such as `GameUtility` or `CommonUtility` are not permitted.

Keep validation rules and `Validate*` methods in feature-owned static
`*Validation` classes. Components and lifecycle entries invoke those classes
from their explicit initialization or operation methods. Define errors and
diagnostic messages in the corresponding static `*Errors` classes.

Immutable `static readonly` values are allowed in `Constants`; mutable global
runtime state and cached services are not. If a static operation needs an
injected service, make it a Manager or an `Other` strategy instead.

### Tests

Test fixtures end with `Tests` and are stored directly under
`Contexts/<Context>/Tests/Editor` or `Contexts/<Context>/Tests/Play`; they do
not live inside production Feature folders. Nested test-category folders are
not used. Editor tests cover feature services, DI bindings, and editor-safe
logic. Play tests cover scenes, prefabs, Unity lifecycle, frame progression,
and runtime integration. Shared primitives do not receive dedicated test
fixtures. Test assemblies reference the runtime assembly and are never
production dependencies.

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
- Top-level abstract classes in contexts: `Base` prefix
  (`BaseNetworkStateMachine`), with the generic definition-family exception
  documented under Abstract. Nested implementation types and Shared primitives
  do not inherit the context-category rule.
- Private instance and static fields: `_camelCase`.
- Constants: `PascalCase` (`DefaultTimeout`), not screaming snake case.
- Local variables and parameters: `camelCase`.
- Project-owned identifiers use the canonical abbreviation forms below.
  Apply them consistently across types, methods, properties, fields, and
  parameters. At the start of a camelCase identifier, lowercase the abbreviation
  (`hudStyleSheet`, `_hudStyleSheet`, `id`); embedded abbreviations retain their
  canonical form (`ShouldShowHUDOnStart`, `_selectedLogId`, `ChangeFOV`).
  External API names and framework-required overrides retain the SDK spelling,
  for example Unity's `GetInstanceID`.
- Boolean state fields and properties use `Is`, `Has`, `Can`, or `Should`
  prefixes (`_isVisible`, `IsEnabledInEditor`, `ShouldOpenOnStart`). `Was` is
  allowed for historical/sample notifications such as `WasPressed` and
  `WasReleased`. This rule does not apply to operation-result methods such as
  `TryGet`, `TryFind`, `Equals`, `Execute(..., out result)` or comparison
  operators, nor to framework-required members.
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
`Shared`. Folder depth and feature responsibility do not extend the namespace. Third-party
code keeps its original namespace.

| Concept | Canonical form | Examples |
| --- | --- | --- |
| Identifier | `Id` | `NetworkEntityIdData`, `TargetId`, `logId` |
| User interface | `UI` | `BuildUI`, `uiRoot` |
| Heads-up display | `HUD` | `DebugVisualizationHUDWidgetView`, `hudStyleSheet` |
| Data transfer object | `DTO` | `LocalSaveDocumentDTO<TData>` |
| Frames per second | `FPS` | `FPS`, `fps` |
| Field of view | `FOV` | `ChangeFOV`, `newFOVValue`, `_fovSmoothTime` |
| JSON format | `Json` | `JsonLocalSaveSerializer` |
| Network Time Protocol | `Ntp` | `NtpTimeProvider` |

### Type Name Composition

For new context-owned types, prefer `[I|Base]<Subject><Detail><Role>` when
these segments help describe the responsibility. `I` and `Base` apply only
to their documented type kinds; detail is optional, and subject contracts may
omit the role as described under Abstract. Use a subject or feature term to
avoid ambiguity in the flat context namespace, without repeating the full
feature name mechanically. For example, `NetworkEntitySpawner` identifies
both its subject and role.

A mechanism qualifier may precede the subject when it distinguishes
implementations: `JsonLocalSaveSerializer` and `NtpTimeProvider`. This is a
readability convention, not a rigid ordering grammar. Preserve meaningful
existing names; reorder them when a related change resolves ambiguity or
incorrect responsibility, rather than for word order alone.

Generic parameters describe their purpose: `TSettings`, `TPayload`,
`TScene`, `TFeature`, or `TStateType`. `T` remains appropriate for a universal
operation with a single type parameter, such as `ReadValue<T>`. Preserve
parameter names required by external SDK contracts. This rule is independent
of the documented `T` filename suffix for generic types.

### Injection Method Names

Project-owned Zenject method injection uses `[Inject] Construct(...)`.
This is a dependency injection method, not a C# constructor or a second
initialization entry point. Non-Unity objects still prefer actual constructor
injection. Explicit method injection on other objects uses the same name.

When a base needs a distinct injection method name, use
`ConstructBase<Type>(...)`, for example `ConstructBaseNetworkEntityComponent`
on `BaseNetworkEntityComponent`. A plain `Construct` is also valid on a base
when a distinct name is unnecessary. This convention does not rename injected
fields/properties or framework-required methods.

### Empty, Default, and NoOp

| Qualifier | Meaning | Existing example |
| --- | --- | --- |
| `Empty` | A valid value or registration with no contents/payload. | `EmptySceneSettings`, `EmptyPopupPayload` |
| `Default` | The selected configuration or implementation used when no alternative is chosen; it may have behavior and non-empty values. | `DefaultLoadingScreenView`, `DefaultLoadingScreenDefinition` |
| `NoOp` | A valid behavioral implementation that performs no effects while honoring its contract's return values and lifecycle. | `DebugVisualizationNoOpAdapter` |

Choose by semantics, not as interchangeable spellings. These qualifiers do
not replace the final role suffix or change the owning category. They may
follow the subject when that reads naturally, as in
`DebugVisualizationNoOpAdapter`.

### Nested Implementation Types

The file/type and category suffix rules apply to top-level types. A nested
implementation type may use a short name scoped by its containing type,
such as `CheatRegistration`, `ConverterRegistration`, `ChannelTreeNode`,
or `State`; it does not require a separate file, feature prefix, `Base`
prefix, or top-level category suffix. It still follows casing, meaningful
naming, visibility, and member ordering rules. Serialization compatibility
rules still apply if a serialized nested type is renamed.

Keep a type nested while its responsibility is local to the containing type.
When extracted into a shared top-level API or dependency, give it an
unambiguous context-level name and place it in its actual role category.

### Enforcement

The architecture validator checks objective structure rules, including
file/type correspondence, category suffixes, interface prefixes, and
top-level abstract base prefixes with documented exceptions. It does not
infer a contract's responsibility from its name. Contract role selection,
word order, generic parameter meaning, injection method naming, and the
`Empty`/`Default`/`NoOp` distinction are reviewed against this document.
A clean validator report does not certify all naming semantics.

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
- Keep business rules in the owning behavioral types and presentation changes
  in Components/Views. Reusable Unity operations may belong to Services,
  Strategies, or Processors within their bounded responsibility, as in
  LevelCollisionService. External integrations are consumed through injected
  contracts; Unity API access alone does not select a category.
- Use UniTask for asynchronous runtime/initialization workflows; Shared must
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
- ScriptableObject assets: `Config_<OwnerContext>_<FeatureOrRole>_<Detail>`.
- `CreateAssetMenu.menuName`: `SO/<Context>/<Feature>/<Detail>`.
- Sprites: `Sprite_<FeatureOrRole>_<ActionOrPurpose>[_<Direction>][_<Frame>]`.
- Fonts: `Font_<FeatureOrRole>_<Purpose>` in `Assets/Fonts`.
- Reference images: `Reference_<Context>_<Purpose>` in `Assets/References`.
- Animation clips: `Animation_<Feature>_<Action>[_<Direction>]`. Existing combined
  action/direction names remain compatible; normalize them when editing animation content.
- Animator controllers: `AnimatorController_<Feature>_<Purpose>`.
- Input action sets: `InputActions_<OwnerContext>[_<Purpose>]`.
- UI documents: `Document_`, `Screen_`, `Popup_`, `Widget_` or `LoadingScreen_`
  followed by owner and purpose. USS uses the corresponding document's name;
  shared styles may use `Document_<Owner>_<Feature>_<Purpose>`. TSS uses `Theme_`.
- Keep feature-owned resources in `<Feature>/Assets`, grouped by resource type.
- Context composition resources live in `Contexts/<Context>/Assets`; project-wide
  rendering, default input settings and common game UI resources live in
  `Assets/ProjectCore/Assets`.
- Ownership of a resource instance can differ from ownership of its reusable C#
  type. Application catalogs belong to `Project/Assets/Catalogs`; the scene's
  gameplay input belongs to that scene context. Reusable Template defaults stay in Template.
- Keep SceneDefinition beside its matching scene in `Assets/Scenes`. Other
  definitions belong in `Assets/Definitions`; catalogs in `Assets/Catalogs`;
  behavior configs in `Assets/Configs`; PanelSettings in `Assets/UI/PanelSettings`.
- Choose a scene folder explicitly in the Scene Definition window; there is no
  hard-coded default context. The scene name must match the selected context.
- Do not create empty categories or rename imported SDK trees for consistency.
- Move Unity `.meta` files together with renamed assets.
- Use `FormerlySerializedAs` or `MovedFrom` when serialized fields/types are
  renamed.

## Common UI Resource Layout

The owner of game UI resources shared across contexts is
`Assets/ProjectCore/Assets`, not any concrete context. Use semantic resource
folders with a CommonUI group:

```text
Assets/ProjectCore/Assets/
  UI/
    UXML/CommonUI/
    USS/CommonUI/          # shared tokens, component styles and themes
    PanelSettings/        # shared panel policies only when actually needed
  Sprites/CommonUI/
    Buttons/ Icons/ Backgrounds/
  Fonts/CommonUI/
```

Create only needed directories. Examples: `Widget_CommonUI_Button.uxml`,
its `Widget_CommonUI_Button.uss`, `Document_CommonUI_Tokens.uss`,
`Theme_CommonUI_Runtime.tss`, `Sprite_CommonUI_Button_Primary.png`, and
`Font_CommonUI_Body.ttf`. These are naming examples, not existing assets.
Feature-specific screens/art and temporary prototype resources stay with their
owning feature. Reusable project-independent Template assets retain their owner.
Promotion and shared-resource change rules belong to
[the runtime UI standard](UI_TOOLKIT_STANDARD.md#common-ui-resources).
