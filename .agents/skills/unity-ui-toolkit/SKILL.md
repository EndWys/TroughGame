---
name: unity-ui-toolkit
description: Design, implement, bind and verify TroughGame runtime UI Toolkit screens, popups and reusable widgets with MVVM by default and scoped, approved MVP alternatives. Use for new UI and UI layout, input, binding or collection defects, including work inside the feature or bugfix workflow. Exclude uGUI, IMGUI and unrelated gameplay changes.
---

# Unity UI Toolkit Development

## Purpose and Scope

Own the UI technical work from design analysis through functional verification.
Reproduce the approved design with adaptive layout, reusable resources, MVVM by default,
data binding and input behavior. Correct targeted defects within the current host
scope; do not automatically start a bugfix workflow or redesign the screen.
This skill runs in the developer agent. It creates no independent workflow state
and does not dispatch another agent by default.
Automatic selection for relevant UI work remains enabled; a manual invocation is
not required. This does not authorize starting another skill's workflow.

## Documents and Rules

Read project AGENTS and the documentation, architecture and naming standards.
[UI_TOOLKIT_STANDARD.md](../../../docs/UI_TOOLKIT_STANDARD.md) owns runtime UI
architecture. Local standards and developer decisions take precedence over
generic plugin examples. Read installed `unity:ui-uitk` for technical UITK
operations and `unity:unity-cli` before Editor operations; verify APIs against
the project's installed Unity version. Neither skill grants scope or approval.

This project skill includes functional UI logic and binding for the agreed scope;
the generic plugin's default UXML/USS-only screen output does not narrow it.
Execute available Editor refresh/Console/runtime checks yourself before requesting
manual actions. Runtime geometry/style updates are allowed by the UI standard;
check versioned API/USS support rather than copying blanket plugin prohibitions.

| When to Read | Reference |
| --- | --- |
| Start/resume, approval, feedback, host return | [workflow-integration.md](references/workflow-integration.md) |
| Design analysis, hierarchy, existing assets, prototype resources | [design-and-resources.md](references/design-and-resources.md) |
| Panel scaling, aspect ratios, adaptive layout | [responsive-layout.md](references/responsive-layout.md) |
| Pattern selection, ViewModel/Presenter, synchronization, commands, DI and lifecycle | [mvvm-and-binding.md](references/mvvm-and-binding.md) |
| Scrolls, variable rows, virtualization, recycling | [dynamic-collections.md](references/dynamic-collections.md) |
| Every stage handoff | [response-formats.md](references/response-formats.md) |
| Implementation audit, visual and functional checks | [validation.md](references/validation.md) |

## Ownership and Inputs

Identify the owning feature and any active host FEAT or bug case and approved scope. For direct UI
requests, use `unity-feature-development-v2` only if the developer has explicitly
invoked it for that feature. Do not invoke it or create feature state automatically.
Otherwise handle UI work within the developer's authorized scope. Defects and
existing-UI improvements use applicable host scope/approvals without fabricating
feature/bug state. `unity-bugfix` requires a direct developer command.
Without recorded host state, retain applicable decisions in the current developer
handoff or existing change artifact. A narrow explicit edit may already authorize
its exact changes; do not add a full planning/prototype gate for an unaffected area.

Inspect the supplied design/source, actual view/base classes, definitions,
catalogs, UIDocument/PanelSettings, domain contracts and owning resources.
Use Template ScreenNavigation/Popup infrastructure when applicable. Choose the
actual owning context/feature, preserve flat namespaces and `.meta`, and add no
`.asmdef`. Shared remains .NET-only. Do not silently migrate unrelated UI.
Common UI assets use the root `Assets/ProjectCore/Assets` layout in CONVENTIONS.
Search there and within the owning feature/context and relevant Template resources.
Another concrete context's UI assets require approved promotion before reuse.

MVP is an architecture choice, not automatic alternation. Show the concrete
benefit against a compact MVVM alternative and accept it through the host gate.
Use one presentation owner per unit; manual callbacks do not themselves require
MVP. Keep navigation, DI, lifetime and verification common. Decorative/stateless
elements need no artificial ViewModel or Presenter.

## Stages and Gates

| Phase | Required Deliverable | Next Action / Gate |
| --- | --- | --- |
| Analyze | Design facts/inferences, existing implementation, behavior and states | Resolve blocking product/design questions; inspect reusable resources |
| Plan | Hierarchy, exact files/types, presentation pattern/contracts/lifetime, resource inventory, resolution/check matrix | Approve one consolidated UI architecture/resource plan, including any MVP choice, through host gate |
| Visual prototype | Approved structure/styles/resources, representative preview data, render evidence and differences | Developer accepts visual prototype before complete functional wiring |
| Functional implementation | Binding, input/commands, dynamic content, lifecycle, navigation and service integration | Applicable checks and independent inspection of the final relevant diff |
| Review and verification | Exact changes, visual/functional results, risks and remaining limits | Host diff review and required Play Mode/retest acceptance |
| Return | Accepted facts, remaining actions and exact host continuation | Host owns state/checkpoint/commit; no UI-stage Notion invocation |

The visual phase may include minimal input or sample binding needed to exercise
layout, but must not present mock actions as working product behavior. Targeted
edits/bugs may skip unaffected phases with an explicit reason; no forced complete
screen redesign or MVVM migration. Bugfix-specific gates apply only inside a
developer-invoked bugfix workflow; do not import them into ordinary UI corrections.

## Iteration and Stop Conditions

Persist applicable proposals, decisions and evidence references in the host's
current artifact. Use existing approvals with exact subject/revision; create no
UI YAML, task, journal or second checkpoint. A visual prototype acceptance is
specific to that prototype and never accepts the complete subtask diff.

Work within the accepted phase. Feedback corrects that phase in the same scope.
Changed behavior/owner/contracts/layout policy/resources requires a visible
revision of the applicable approval. Existing authorization persists; silence
is not approval. Show the concrete proposal before asking and explain a stop
with the exact rule's link and a short quote.

If a defect appears, inspect UI evidence and correct the current phase within host
scope, then repeat relevant checks. Do not invoke/load `unity-bugfix` or add its
gates unless directly commanded by the developer. A manually requested bugfix uses
the same host artifact/case, state and checkpoint.

## Completion

Use stage-specific responses in the developer's language. Report actual evidence
and unavailable checks separately. Complete UI work only when the approved visual
and behavior criteria are covered, required review/retest is accepted, resources
are identified as final or accepted placeholders, and the host receives the result.
Commit permissions come from the host. UI stages never invoke Notion; accepted
specification decisions remain local until whole-feature finalization.
