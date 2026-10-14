---
name: unity-bugfix
description: Diagnose and fix TroughGame defects with evidence, approved investigation and exact fix plans, regression checks, review and retest gates. Use for standalone bugs and defects within active features; exclude feature design, general refactoring and security-specific remediation.
---

# Unity Bug Diagnosis and Fixing

## Purpose and Scope

Analyze existing evidence read-only first. A confirmed cause leads to a fix proposal;
an uncertain cause leads to approved data collection. The developer selects material
options and accepts results. Do not design new features or silently change agreed
behavior. Missing/conflicting expectations require a product decision. Bugs do not
authorize unrelated refactoring. Vulnerabilities follow the applicable security-fix skill.

## Documents and Rules

Read AGENTS, architecture/conventions, applicable contracts, and
`docs/ai-workflow/DOCUMENTATION_STANDARD.md`. Use installed `unity:unity-cli`
for Editor operations and the scene hierarchy standard for scenes.

| When to Read | Reference |
| --- | --- |
| Causes, hypotheses, experiments | [diagnosis.md](references/diagnosis.md) |
| Start/resume, standalone/embedded | [state-and-integration.md](references/state-and-integration.md) |
| Every handoff/approval | [response-formats.md](references/response-formats.md) |
| Implementation, checks, commit, Notion | [verification-and-delivery.md](references/verification-and-delivery.md) |
| UI layout, input, binding, navigation, lifetime or collection defects | [unity-ui-toolkit](../unity-ui-toolkit/SKILL.md) |

## Stages and Gates

| Stage | Deliverable | Transition / Stop |
| --- | --- | --- |
| `intake` | Expected/actual, source, steps, version/commit, roles/logs | Read-only analysis; blocking questions only |
| `root_cause_analysis` | Mechanism, facts, inferences, alternatives | Confirmed → `fix_proposal`; otherwise → `diagnostic_plan` |
| `diagnostic_plan` | Hypotheses/likelihood, discriminating probes, conditional fix complexity; exact logs/Profiler/test steps | Approve new actions/instrumentation |
| `diagnostic_execution` | Actual results, unavailable checks, remaining hypotheses | Analyze again; changed/new probes need an approved revision |
| `fix_proposal` | Cause, before/after, exact files/members/assets, real options/recommendation | Approve selected fix/scope |
| `implementation` | Accepted correction, probe cleanup, useful regression | Checks/audit; stop for scope/design change |
| `verification_review` | Checks, original scenario, complete relevant diff audit/risks | Required findings → correction/analysis; developer diff acceptance |
| `retest_acceptance` | Original case, adjacent paths, required Play Mode/peer/lifecycle cases | Failure → analysis; explicit acceptance before delivery |
| `delivery` | Accepted commit and Notion outcome/publication blocker | Standalone complete; embedded awaits explicit return to feature |

Authorization persists; do not re-approve unchanged probes/fixes. Mechanical edits
within the exact accepted plan add no gate. New mechanism/owner/API/behavior/scope/
experiment needs revision. Complete available authorized preparation before asking.
Silence and compilation success prove neither consent nor original-case remediation.

## Stop Conditions

For runtime UI defects use `unity-ui-toolkit` in this agent, with this workflow's
case/host state. Add UI-specific context, probes, exact hierarchy/binding changes
and render/input evidence. Its technical forms supplement causal analysis and
approved diagnostic/fix plans; do not force a whole-screen prototype/redesign or
unrelated MVVM migration. This workflow retains review/retest and delivery gates.

Diagnostic approval permits described probes, not a production fix. Fix approval
permits the selected correction, not hypothesis investigation or unrelated cleanup.
Review accepts the final diff and does not replace retest/Play Mode; both are needed
before commit. Show proposals in chat; explain stops with the exact rule's link/short
quote. If new evidence/failed retest disproves the fix, return to analysis.

## Ownership and Completion

Within an active feature the same developer agent uses current FEAT/interruption,
one host state, and `bugs/<bug-id>/case.md`. Create no second state/new subtask.
The host owns transitions and one corrected checkpoint. Standalone bugs use
`.ai/bugs/<bug-id>/` from the template, without feature state. Notion uses a separate
worker through the coordinator, receiving immutable facts/decisions/results and
never reading developer state/cases. Closing a bug task does not complete its feature/stage.
