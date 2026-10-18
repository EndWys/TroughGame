# UI Workflow Integration

## Purpose and Ownership

Specialize UI work without duplicating host state, approvals or task ownership.
The developer agent uses this skill directly. Feature workflow owns its FEAT,
state, scope and checkpoint. A manually invoked bugfix workflow owns its agreed
diagnostics/correction/retest; ordinary UI defects remain host corrections. No UI stage dispatches a Notion worker.

## Feature Integration

1. During research inspect UI sources, services and reusable assets. Include the
   UI outcome and validation in the approved feature plan; do not add a subtask
   just because this skill is invoked.
2. In `subtask_architecture`, present hierarchy, resources, exact types/contracts,
   lifecycle, bindings, input and validation using the UI response forms. One
   architecture approval covers these sections; no duplicate skill approval.
   Use MVVM by default; an MVP choice includes its concrete comparison and View/
   Presenter ownership. Record the accepted pattern/revision in the same artifact.
3. During `implementation`, build the approved visual prototype and present render
   evidence. Record acceptance in the existing approvals as `gate: review` with
   subject `UI visual prototype: <view>, revision <n>` and the actual decision.
   Keep the subtask in progress; this is not its final developer review/audit.
4. After prototype acceptance implement complete behavior, audit and present the
   final relevant diff through the host's developer-review/Play Mode gates.
5. Hand back accepted UI facts and exact continuation. The host commits once
   after its required acceptance/checks. No UI-stage Notion dispatch; retain accepted
   specification decisions for whole-feature finalization.

Store UI plan/revision/prototype decisions in `subtasks/FEAT-XX.md`; temporary
screenshots/logs belong in ignored `.ai/evidence/`. Link evidence instead of
putting screenshot data in YAML. Existing state versions are not silently migrated;
agree introduction of new prototype gates before resuming an older active task.

## Defect Corrections and Manual Bugfix Integration

Layout/scaling, binding, input, navigation, lifetime and collection defects stay in
the current host scope and existing verification/review gates. Inspect the actual
view, resolution, input and lifecycle conditions. Do not load/invoke `unity-bugfix`,
create its state, import its approval/response forms or ask to activate it.
Only a direct developer command starts that process. If requested, supplement its
short case with relevant UI evidence, preserve one host state/checkpoint and reuse
unchanged hierarchy/assets. Targeted fixes do not require a full visual prototype
unless the accepted correction redesigns the UI. Retest the original scenario.

## Feedback and Resume

Record the exact accepted revision and why a phase reopens. Cosmetic corrections
within accepted structure/resources stay in the same phase. Changes to product
behavior, domain contracts, owner, shared PanelSettings or missing-resource scope
require the host's applicable revision approval. Do authorized preparation first.
Resume from current facts/approval; do not repeat unchanged approvals or accept
silence. Every handoff identifies phase, outcome/decision, next action and owner.
