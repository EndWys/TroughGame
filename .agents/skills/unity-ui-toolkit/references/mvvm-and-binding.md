# Presentation Architecture and Runtime Binding

## Purpose

Apply [the runtime UI standard](../../../../docs/UI_TOOLKIT_STANDARD.md) to exact
contracts, initialization, updates and input. It owns architectural policy;
this reference supplies the implementation procedure.

## Architecture Proposal

Select MVVM by default. An MVP proposal compares actual complexity/contracts with
the compact MVVM alternative and requires the host's architecture acceptance.
Show Model services/state, one presentation owner (ViewModel or Presenter), View
and its synchronization. Do not invent a parallel navigation or lifecycle system.
For each command define inputs, validation authority, busy/reentry policy, result,
error/cancellation and notification. For each binding name the element/property,
ViewModel property, mode and update mechanism where binding is used. In MVP show
the View interface, its actions/events, Presenter decisions and explicit updates.
Explain who constructs/owns each
instance and what survives hide, close, reopen, scene unload and row recycling.

Use existing `IClassFactory`, BaseScreenView/BasePopupView initialization and
catalog/definition composition where appropriate. Verify actual method signatures
before edits. Settings/payloads supply operation data. Initialize after UXML clone;
do not invent a second UIDocument or inject a presentation owner as a global AsSingle.
Concrete per-view ViewModels use Other/ViewModels; Presenters use Other/Presenters
and View contracts use Abstract. Unity-bound notification code
stays outside Shared. Select native APIs compatible with the installed version.

## MVVM Binding and Explicit Updates

Use native DataBinding only where it simplifies the approved synchronization.
Manual View callbacks and projection from ViewModel state remain MVVM: the
ViewModel does not acquire a View dependency merely because a field is unbound.

1. Expose only required presentation properties. `[CreateProperty]` makes a
   property available to property bags; define change notification/versioning
   separately. Do not claim that exposing a getter makes updates event-driven.
2. Declare modes explicitly. ToTarget projects service-derived state; TwoWay edits
   a local draft with a command commit. Feedback for parse/validation failures
   must not become a binding feedback loop or unauthorized domain mutation.
3. Use `INotifyBindablePropertyChanged`, `IDataSourceViewHashProvider` or explicit
   scoped updates where justified. Inspect current API signatures. Cache expensive
   computed projections; notify only relevant changed values. Marshal Unity-facing
   updates to the supported main-thread path.
4. Register discrete input callbacks once. Forward to a ViewModel command; keep
   focus, event propagation, modal input and visual transitions in the View.
5. Attach bindings/data sources at supported initialization; detach owned bindings,
   callbacks and model subscriptions at the specified lifetime boundary. Clear
   references to old instances and prevent late results reaching replacements.

Do not use Editor SerializedObject binding for runtime domain state. Prefer typed
paths/property names supported by the project rather than dispersed string keys.
Converters handle presentation conversion, not side-effecting service calls.
Structural collection changes require collection refresh policy in addition to
property notifications; use [dynamic-collections.md](dynamic-collections.md).

## Approved MVP Implementation

1. Define the narrow View contract approved for this unit: required user events,
   input values and visual update operations, without exposing VisualElements.
2. Construct the Presenter through the existing factory, then explicitly attach
   the View contract once. Avoid circular constructor dependencies. The View owns
   actual controls/focus/geometry; the Presenter owns presentation decisions.
3. Register user/model events once, call authoritative services, and explicitly
   update the View contract for state changes. Explain initial render, invalid
   input, busy/reentry, error/retry and cancellation. Synchronization must not
   rely on incidental render events or change listeners multiplying on reopen.
4. On release detach owned event/model subscriptions and cancel owned operations.
   Keep the same hide/close policy as the host. Do not dispose borrowed services
   or update a released View. Do not add a duplicate ViewModel to hold the same state.

If direct updates become more complex than the accepted MVVM alternative, present
a scoped architecture revision; do not silently migrate the window while fixing it.

## Lifecycle and Input Checks

Verify service change → presentation owner → UI, input → presentation action →
service, invalid input,
busy/repeated activation, error/retry, cancellation, close/reopen and hidden-history
behavior. Confirm no duplicate callbacks, stale data sources or updates after final
close. For popups verify uppermost input, blocker lifetime and typed response once.
Navigation/focus and input subscriptions must follow existing ownership contracts.

## Technical Sources and Version Boundaries

- [Unity 6.3 runtime binding](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-runtime-binding.html).
- [Data-source exposure and update interfaces](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-runtime-binding-define-data-source.html).

The technical plugin contains examples for later Unity releases. TroughGame was
6000.3.14f1 during skill creation: use its UIDocument infrastructure, not a newer
PanelRenderer example. Recheck ProjectVersion on use. Project presentation/lifetime rules
override generic MonoBehaviour ViewModel scaffolds and flat UI directory layouts.
