# Stage Deliverables and Response Forms

## Purpose

Present applicable facts in chat using the common format and developer's language.
Artifacts retain records; they do not replace visible approval proposals.
Omit empty/inapplicable sections. The forms below describe content in English;
translate developer-facing headings/table labels when rendering them.

## Requirements and Research

Use [requirements-contract](requirements-contract.md) and [project-research](project-research.md):
sources, requirements/acceptance criteria (RQ/AC), questions, research, integration,
and the whole initial plan. Do not start subtasks before plan approval.

## Architecture

Present applicable [subtask architecture](subtask-architecture.md): scope/outcome,
inventory, data/state/lifecycle, Unity/DI/references, choices, risks/decisions, checks.
Before asking, expose before → after and the actual execution chain.

| Path / Object / Type | Change | Behavior / API / Data | Owner / Dependencies | Check |
| --- | --- | --- | --- | --- |
| Verified name | Reuse / extend / create | Concrete contract | Actual owner/callers | Observable result |

A FEAT link does not hide material API/serialization/lifecycle choices.

## Implementation and Review

For runtime UI extend the applicable handoff with
[UI stage forms](../../unity-ui-toolkit/references/response-formats.md): hierarchy,
resources, accepted presentation pattern/synchronization/lifetime, visual prototype comparison and resolution/input
evidence. Visual prototype review accepts only that revision's presentation;
the complete subtask diff still needs this workflow's final review and Play Mode.

Start with delivered behavior. Group by area while accounting for every changed
production file/Unity asset.

| Area / Files / Objects | Actual Change | Behavioral Effect |
| --- | --- | --- |
| Actual set | Implementation/integration/cleanup | Observable result |

| Check | Command / Scenario / Outcome | Evidence / Limit |
| --- | --- | --- |
| Applicable action | Actual run/result | Data and gap |

Name checks/limits. Source inspection does not prove runtime bindings. Do not
repeat the whole architecture, every method, or final RQ traceability.
Next: [code audit](ai-code-audit.md), review of the same diff, then Play Mode.

## Current Subtask Defect

Show expected/actual behavior, relevant evidence, correction and actual verification
within the current FEAT. Existing audit/review/Play Mode gates remain; a defect does
not start an extra workflow, case/state file or mandatory bugfix response format.
Do not invoke/load `unity-bugfix` unless the developer explicitly commands it.
If manually requested, reuse the same feature artifact/case and checkpoint.

## Scope or Architecture Revision

Only for a fact changing boundaries/design. Show reason, previous accepted solution,
new evidence, and minimal revision.

| Accepted Solution | New Proven Fact | Impact |
| --- | --- | --- |
| Scope/owner/contract | Code/Unity/check fact | Why the original solution fails |

| Revision Type | Affected Files / Systems / Objects | Exact Change | Validation Delta |
| --- | --- | --- | --- |
| Scope or architecture | Actual area | Minimal delta | New/changed checks |

Scope expansion adds a boundary; architecture changes owner/contract/lifecycle
inside it. Mechanical corrections of the same solution do not need this gate.
Do not create another subtask or rewrite the whole accepted plan.

## Plan Update

Only after an explicit developer request; otherwise state that the plan is unchanged.

| Basis / Evidence | FEAT | Add / Remove / Split / Merge / Reorder | Reason / Effect |
| --- | --- | --- | --- |
| Request/accepted fact | ID | Exact delta | Dependencies/scope/checks/outcome |

Do not rewrite completed work or create a subtask for a bug/review iteration.
Requirements changes return to the contract instead of hiding in plan deltas.

## Final Verification

After all review/Play Mode gates, verify the whole contract. Show passed/failed/
not-tested counts and every RQ/AC:

| RQ/AC | Implementation / Object | Actual Check / Evidence | Result |
| --- | --- | --- | --- |
| Every ID | Type.Member/asset | Test, Play Mode, log, Inspector | Passed / failed / not tested |

Expose gaps/consequences. Failed/not-tested mandatory criteria return to an existing
subtask or data request, not final readiness. Save `final-verification.md` and
present it before workflow audit.

## Workflow Audit

Before final readiness run `Tools/AI/workflow-audit.ps1 -Feature <feature>`.
Show actual command, result/report paths, outcome, and findings:

| Process Point | Evidence | Effect | Recommended Action |
| --- | --- | --- | --- |
| Gate/state/check/Git | Audit fact | Process gap | Minimal decision |

Explicitly state when there are no findings. Do not repeat code audit, architecture,
or traceability. `complete` allows readiness discussion; `attention_required`
remains visible for a decision/retrospective; `not_available` does not skip the gate.
Record developer presentation only after sending the response. Findings do not
authorize automatic policy changes.

## Notion Finalization

After whole-feature final readiness acceptance and before retrospective, explicitly
load `../../notion-feature-finalization/SKILL.md`. Pass exact task/feature targets,
desired statuses and short accepted specification/plan deltas. No intermediate
workflow reports. Show confirmed/pending outcomes from remote readback, or record
not applicable without a worker if there are no Notion targets. Retain that outcome
in main-agent-owned `notion-finalization.md`; retry this stage only.

## Retrospective

After final acceptance and Notion finalization, present up to three evidenced improvements or explicitly
none. Use audit/iteration/check/scope-correction facts; one-off bugs/preferences
do not become general rules.

| Observation | Evidence | Exact Proposed Change | Owner | Effect |
| --- | --- | --- | --- | --- |
| Recurring issue | Process fact | Small rule/tool change | File | Reliability/speed |

Each permanent change requires separate approval.

### Optional Local Knowledge Promotion

Per [knowledge-promotion](knowledge-promotion.md), show eligible KNP, fact/evidence,
rule, applicability, one owner, duplicates/conflicts, effect, approve/reject/change
outcomes. Do not reopen the feature. Approval permits the exact change/checks/
separate commit. Changed rule/target needs revision; record rejection/not applicable.
