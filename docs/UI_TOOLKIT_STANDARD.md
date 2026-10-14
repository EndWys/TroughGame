# Runtime UI Toolkit Standard

## Purpose and Scope

Use MVVM by default for new runtime UI Toolkit screens, popups and interactive
UI systems. MVP is a scoped alternative selected through architecture approval
when its concrete implementation is demonstrably simpler or clearer.
This standard owns runtime presentation responsibilities and lifetime rules.
[Architecture rules](ARCHITECTURE_RULES.md) own dependency/DI/initialization policy;
[conventions](CONVENTIONS.md) own folders, names and style. Existing UI is migrated
only within an approved change. Editor IMGUI/uGUI are outside this standard.

## Responsibilities and Dependencies

| Layer | Responsibility | Dependencies / Boundaries |
| --- | --- | --- |
| Model | Existing domain state, services, authority and validation | Does not know Views, ViewModels or Presenters; domain validation stays authoritative |
| ViewModel | Presentation state, drafts, validation feedback, busy/error state, commands and model-to-UI projection | Plain C# with injected domain contracts; no VisualElement/UIDocument, scene lookup or navigation visuals |
| Presenter, in approved MVP | Handles View actions, owns presentation decisions/state and explicitly updates a View contract | Plain C# with injected domain contracts and a narrow View interface; no VisualElement/UIDocument, scene lookup or business-rule duplication |
| View | UXML/USS hierarchy, visual states, bindings, input forwarding, focus and visual transitions | Owns VisualElements and one presentation lifetime; no duplicated business rules |

Input flows View → ViewModel command → domain service → state/notification →
ViewModel → View. A presentation contract may request navigation; its coordinator
owns window creation/history. Do not inject a concrete View into a domain service
or let the ViewModel manipulate the visual tree. In MVP, input flows View →
Presenter → domain service → Presenter → View contract. The View implements
that contract and owns actual controls, geometry, focus and visual transitions.

An interactive screen/popup normally has a ViewModel; approved MVP uses a Presenter.
Purely decorative frames/icons and stateless controls need no separate presentation
owner. A reusable widget with independent state/commands may own one; item
ViewModels represent row presentation where useful.
Do not create a general MVVM/MVP framework or package without a demonstrated need and
approved architecture. Shared cannot reference Unity UI/Properties APIs.

## Pattern Selection

| Situation | Preferred Approach |
| --- | --- |
| Forms, settings, inventories, filters or interdependent loading/error/selection state | MVVM; explicit presentation state and synchronization |
| Small popup with a few actions | Compact MVVM by default; consider MVP only if its actual contract/control flow reduces complexity |
| Predominantly explicit interaction sequence with little bindable state | Compare a concrete MVP design with the MVVM alternative |
| Decorative/stateless element | View only; no artificial ViewModel or Presenter |
| Focus, drag/drop, geometry and animation mechanics | View/Manipulator; these alone do not justify changing pattern |

Before choosing MVP, show the reason, MVVM alternative, exact View/Presenter
contracts, state/command owner, synchronization cost and checks in the existing
architecture proposal. The developer accepts the choice there; no additional gate
or separate state is needed. Complexity of the screen alone is not sufficient
evidence of benefit. Do not switch an already accepted design without revision.

Each presentation unit has one owner of its state and actions: ViewModel or
Presenter. Do not layer both over the same window with overlapping responsibilities.
Independent child widgets may own their bounded presentation state, without
duplicating their parent's decisions. Navigation, DI, services, lifecycle and
verification remain common; do not build parallel window frameworks.

Manual callbacks or scoped state-to-control updates within a View are compatible
with MVVM. Native DataBinding is optional where it does not simplify the task.
The distinction is ownership/dependency direction, not whether every field binds.

## Construction, Ownership and Lifetime

Construct Views and ViewModels/Presenters through the existing DI/factory flow.
A presentation owner
belongs to one screen/popup/widget instance, not an application/scene `AsSingle`
binding. Constructor injection does not turn it into a Manager. Domain services
keep their existing container lifetime. Abstract contracts/base classes go in
`Scripts/Abstract`; concrete ViewModels go in `Scripts/Other/ViewModels` with
`ViewModel` suffix; concrete per-view Presenters use `Scripts/Other/Presenters`
with `Presenter` suffix. MVP View interfaces belong in `Scripts/Abstract` and
expose only necessary state/events/operations. Views use Components/Screens,
Popups or Widgets. Wire the View/Presenter explicitly after construction; avoid
circular constructor dependencies and hidden service-locator resolution.

Integrate screens with Template ScreenNavigation and popups with Template Popup.
Use typed definitions, scene catalogs and their existing UIDocument hosts.
Settings/payloads are operation inputs passed directly, not DI registrations.
UXML is cloned before view initialization; query and bind in the supported
initialization hook. Do not introduce hidden Awake/Start initialization chains.

Specify the owner and release point for each subscription, binding, row and task.
Final close disposes owned subscriptions/resources and cancels owned operations;
it never disposes borrowed domain services. A hidden screen retained in navigation
history is not necessarily closed: explicitly define pause/resume/update policy.
Popup cancellation/dismissal/failure follows the existing typed response contract.
Avoid continuations updating a closed/replaced presentation owner or recycled row. Disable
or coalesce duplicate submissions and expose failure/retry/cancel behavior.

## Binding and Input

Use Unity runtime DataBinding for properties where it simplifies synchronization.
Select binding modes explicitly: ToTarget for projected read-only state, TwoWay
for editable drafts where appropriate. A command validates and commits the draft
through the domain service; TwoWay does not bypass authority. Discrete actions
may use explicit registered callbacks invoking commands.

`[CreateProperty]` exposes a property to property bags; it is not notification.
Use supported property-change notification and/or data-source versioning when
appropriate. Cache expensive projections, notify on actual changes, and keep
structural collection refresh explicit. Avoid whole-screen polling every frame.
Unregister owned callbacks and bindings at their actual lifecycle end.

In MVP, register View events once, update the accepted View contract explicitly
and detach owned subscriptions on release. A local input draft may be read through
that contract, but presentation decisions and domain commit authority remain with
the Presenter and service respectively. Do not create a duplicate ViewModel merely
to wrap Presenter state. Both patterns require runtime synchronization/input checks.

## Presentation and Resources

Keep layout/style primarily in UXML/USS, reusable templates and shared design
tokens. Runtime style changes are allowed for actual geometry/visual state;
do not duplicate an entire stylesheet in C#. Panel scaling and adaptive layout
are separate decisions: reuse the host's policy and approve shared changes.
Verify FHD/UHD plus relevant aspect ratios, window sizes, content and input modes.

Use the [common UI resource layout](CONVENTIONS.md#common-ui-resource-layout)
under `Assets/ProjectCore/Assets`. Feature and context resources keep their narrow
owner; cross-context UI resource dependencies follow the architecture rules.

## Common UI Resources

Search the owning feature, the approved root CommonUI groups, resources within the
same context and relevant Template capabilities before requesting new art.
Other concrete contexts may inform a promotion proposal, not supply direct private
resource dependencies. Confirm actual paths, visual suitability and import/slice
settings; explain reuse or absence in the resource plan.

Add a resource to CommonUI only for multiple actual consumers or an explicitly
accepted baseline element of the game's design system. It must be independent of
concrete features/contexts. Common UXML/USS/art may reference other root common
resources or appropriate generic Template resources, not context-private assets.
Do not create a Feature or runtime registry solely to store static shared assets.

Use scoped component selectors and shared tokens; global control selectors or
PanelSettings changes must expose affected consumers and be explicitly accepted
in the plan. Existing shared-resource changes include known references, intended
before/after behavior and applicable consumer checks. Preserve .meta/GUID during
approved promotion and verify references; do not duplicate assets to bypass ownership.

Generated prototype art is explicitly temporary until accepted and stays with its
feature. Common promotion requires acceptance as a shared resource, provenance,
purpose, states/import requirements and known consumers in the host resource plan.
No independent approval journal or duplicate global inventory is required.
Resource reuse/addition/promotion uses the existing architecture/resource gate;
material shared changes outside that approval require its scoped revision.

## Workflow and Verification

Use [unity-ui-toolkit](../.agents/skills/unity-ui-toolkit/SKILL.md) inside the host
feature/bugfix workflow. Approve hierarchy/resources/presentation architecture
together, including any MVP choice;
for new UI accept the visual prototype before complete logic. Final diff review
and required Play Mode/retest remain distinct. A targeted fix preserves existing
causal/approval gates and does not require a new complete design.

Verify rendered hierarchy, resolution scaling, overflow, long/empty/error content,
binding/input, navigation/focus, close/reopen and recycled rows as applicable.
Compilation and static checks alone do not prove visual or runtime correctness.
The skill's validation reference defines evidence collection and response forms.
