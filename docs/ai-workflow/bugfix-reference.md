# Bugfix Workflow

## Purpose and Inputs

Version **1.0**. Fix ordinary defects standalone or within an active feature using
[unity-bugfix](../../.agents/skills/unity-bugfix/SKILL.md). Inputs: description/task,
expected/actual behavior, reproduction, version, roles, and available logs.
Preserve agreed behavior. Analyze first: confirmed cause leads to an approved exact
fix; uncertainty leads to approved data collection until proven cause or a concrete blocker.

## Stages and Gates

UI-related defects use [unity-ui-toolkit](../../.agents/skills/unity-ui-toolkit/SKILL.md)
inside the same case/host state. Its technical forms supplement diagnostic/fix
proposals and rendered/binding/input checks. Preserve this workflow's causal and
approval gates; targeted corrections do not require a complete new screen design.

| Stage | Deliverable | Decision / Transition |
| --- | --- | --- |
| Intake | Expected/actual, authority, steps, version/roles/logs | Blocking gaps only |
| Analysis | Causal chain or hypotheses | Confirmed → fix; otherwise → diagnosis |
| Diagnostic plan | Likelihood/evidence, probes, conditional fix complexity | Approve new actions |
| Investigation | Actual results and updated hypotheses | Analyze; revised probes need approval |
| Fix proposal | Cause, exact changes, real options | Approve selected fix |
| Implementation | Fix, probe cleanup, useful regression | Applicable checks/audit |
| Review | Full diff, checks, findings/risks | Developer acceptance |
| Retest/Play Mode | Original/adjacent scenarios | Acceptance; failure → analysis |
| Delivery | Commit/checkpoint and Notion outcome | Embedded bugs await explicit return |

## Responses and Boundaries

Reading code/existing logs needs no extra diagnostic approval. New probes wait for
an approved plan, which does not permit a production fix. Fix approval is separate;
review does not replace retest. Existing authorization persists; silence is not approval.
Present behavior/cause/hypothesis/probe/fix-option/exact-change/result tables per
[response forms](../../.agents/skills/unity-bugfix/references/response-formats.md).
Explain stops with a rule link and short quotation. Render responses in the developer's language.

## Ownership and Resume

Standalone: `.ai/bugs/<bug-id>/state.yaml` and `case.md`. Embedded: current FEAT/
interruption and `bugs/<bug-id>/case.md` inside the feature, one host state/checkpoint,
no new subtask. Shared review/retest applies only to matching final diff/scenarios/roles.
Explicit return follows acceptance. See [state/integration](../../.agents/skills/unity-bugfix/references/state-and-integration.md).

## Verification and Completion

Verify the original reproducer, meaningful regression, applicable automated/Editor
checks, affected peer/lifecycle/performance cases, and audit. Manual scenarios include
steps, expectations, and actual evidence. Installing the skill does not start a bug or
authorize tooling commits. After accepted diff, accepted required retest/Play Mode,
and passing checks, standalone fixes receive one local commit; the host commits embedded
fixes after its gates. Separate Notion workers execute `finalize_bugfix` for exact bug
tasks/documents without feature/stage completion. State/journals remain independent.
See [verification/delivery](../../.agents/skills/unity-bugfix/references/verification-and-delivery.md).

## Related Documents

- [Feature workflow](workflow-reference.md).
- [Common standard](DOCUMENTATION_STANDARD.md).
- [Notion contract](../notion/operation-contract.md).
