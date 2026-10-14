# UI Workflow Integration

## Purpose and Ownership

Specialize UI work without duplicating host state, approvals or task ownership.
The developer agent uses this skill directly. Feature workflow owns its FEAT,
state, scope and checkpoint. Bugfix workflow owns causal analysis, diagnostic
actions, selected correction and retest. Notion owns independent worker journals.

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
   after its required acceptance/checks and publishes through Notion coordination.

Store UI plan/revision/prototype decisions in `subtasks/FEAT-XX.md`; temporary
screenshots/logs belong in ignored `.ai/evidence/`. Link evidence instead of
putting screenshot data in YAML. Existing state versions are not silently migrated;
agree introduction of new prototype gates before resuming an older active task.

## Defect Integration

Use `unity-bugfix` for layout/scaling, binding, input, navigation, lifetime or
collection defects. First establish expected/actual behavior, steps, version,
resolution, input mode and evidence. Confirmed cause → exact fix proposal;
uncertainty → approved probes, not speculative redesign.

UI forms extend the diagnostic/fix handoff without replacing hypotheses,
confidence, conditional complexity or diagnostic approval. Reuse unchanged
hierarchy/assets; skip full prototype approval for a targeted fix unless the
approved correction actually redesigns the UI. Validate the original scenario.

An embedded bug stays in the existing FEAT/interruption/case, with one host state
and corrected checkpoint. A standalone bug stays in its bug state. Return to the
recorded phase after accepted retest; do not close the whole feature on bug closure.

## Feedback and Resume

Record the exact accepted revision and why a phase reopens. Cosmetic corrections
within accepted structure/resources stay in the same phase. Changes to product
behavior, domain contracts, owner, shared PanelSettings or missing-resource scope
require the host's applicable revision approval. Do authorized preparation first.
Resume from current facts/approval; do not repeat unchanged approvals or accept
silence. Every handoff identifies phase, outcome/decision, next action and owner.
