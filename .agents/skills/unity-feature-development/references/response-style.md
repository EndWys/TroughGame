# Common Response Format

## Purpose

Follow `docs/ai-workflow/DOCUMENTATION_STANDARD.md`. This reference governs
presentation, not stages/authority. The developer's explicit format takes precedence.
Use the smallest structure that makes the result/decision reviewable.
Instruction documents are English; render responses and table labels in the
developer's language (Russian in this project unless requested otherwise).

## Order and Wording

1. Result or required action.
2. Concrete facts and evidence.
3. Next step and owner.

Prose explains causality; tables compare scope/options/subtasks/findings/checks.
One item means one fact/choice. Actual files/types/members/objects/commands/evidence
make claims verifiable. Omit empty sections, decorative warnings, and duplicated
diff/requirements.

| Situation | Required Wording / Content |
| --- | --- |
| Approval gate | Developer decision required; exact question |
| Missing mandatory input/check | Blocked; reason |
| Approved scope changes | Scope revision required; exact delta |
| Material risk | Consequence and location |
| Manual action | Executor, preconditions, steps, returned data |
| Result | Passed / failed / not tested / not run / unavailable |

Emphasize actions/risks, not every response. For stops link/briefly quote the
exact rule; complete the proposal before asking. Number independent questions
Q1/Q2 with dependencies. Do not invent options or treat silence/praise of another
item as approval.

## Stage Content

| Stage | Applicable Required Content |
| --- | --- |
| Requirements | Goal, requirements/acceptance criteria (AC), scope/decisions; no implementation design |
| Research/plan | Sources, integration, full proposed FEAT/order/scope/checks; no raw search dump |
| Architecture | Exact inventory, before/after, chain, integration, validation |
| Implementation | Every changed production file/asset by area, actual checks/limits |
| Audit | Scope/evidence/findings or explicit absence |
| Review | Full relevant diff, executed checks, accept/change |
| Play Mode | Preconditions, steps, expected result, actual result/failure data |
| Bug | unity-bugfix forms: expected/actual, cause/hypotheses, approved probes/fix, exact diff, retest |
| Plan | Delta only when explicitly requested; otherwise unchanged |
| Final verification | All RQ/AC with implementation/evidence/status/gaps |
| Workflow audit | Command/report, process findings separately from code audit |
| Retrospective | Up to three supported proposals or none |
| Knowledge promotion | Fact/source/exact rule/owner, consistency, approve/reject/change |

Detailed content belongs to the topic references and [stage deliverables](stage-deliverables.md).

## Evidence and Links

A command was executed only if actually run. Checks cite actual test/run/log/
Inspector/snapshot, not code presence. Show minimal consequential excerpts with
sources instead of entire logs/diffs. Distinguish facts/inferences/proposals.
Use verified absolute file links in Codex responses; relative links are valid
inside artifacts. Avoid unsupported readiness claims, mixed questions/risks/statuses,
style-only findings without consequences, empty tables, needless nesting, and decorative headings.
