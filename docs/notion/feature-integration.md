# Notion Integration with Development

## Purpose and Roles

The user authorized separate Notion subagents/independent state. The developer uses
`notion-project-coordination`; worker skills run only in dedicated subagents.
Code-review delegation is separate. Permission for Notion writes does not permit
implementation/development-state changes.

## Integration Points

| Development Point | Operation / Immutable Inputs |
| --- | --- |
| New feature from Notion or next-ready selection | `prepare_feature_context`: exact IDs or verified selection rule |
| Requirements approval | Developer retains source snapshot in `requirements.md`; approved contract may be published as values |
| Resume/before source-dependent subtask | `refresh_source_context`: prior observations; inspect material delta before stale requirements |
| Accepted plan/architecture/review/blocker/Play Mode result | `publish_progress`: approved delta, tasks, actual evidence, desired mapped status |
| Accepted facts affect documentation | `update_documentation`: exact delta, canonical target, evidence |
| Explicit final acceptance | `finalize_feature`: full scope, mandatory criteria/checks/Play Mode, tasks, business status, commits/limits |
| Retrospective promotes reusable policy | `update_documentation` for the exact separately approved rule/canonical target |

Publish meaningful transitions, not every tool call. Review-ready tasks may become
`Review`; `Done` requires mandatory acceptance. Select feature status from verified
actual options. Publish documentation/results before feature-completion properties.
Include unexpected accepted facts and explicitly deferred work.

## Approvals and State

Requirements/plan/local architecture/review/Play Mode/final readiness gates persist.
`Ready`, search results, intake packets are not approval. The developer prepares a
reviewable result and records the human decision in its own state; the worker later
receives authorized values. Do not re-request existing authorization.
The developer may retain source snapshots/contracts under `.ai/features/` from
returned values; never provide that directory to a worker. No Notion retry/journal
fields enter feature state; workers never advance stages. Worker journals are
independent/ignored by Git. Operation IDs correlate messages, not shared mutable variables.

## Publication Failures

Preserve completed development facts. Report local completion/Notion publication
separately; do not claim synchronized statuses. Resume identical publication under
the same ID through coordinator/private receipts. A new session may ask a worker to
reconcile pending operations for a specified feature; only the worker reads journals.
Do not replay implementation to repair documentation. Missing mandatory sources/
approvals block dependent development; independent preparation can continue.
Standalone documentation requests create no feature folder/state. If subagents are
unavailable report that restriction; the main agent never substitutes direct MCP calls.

## Bugs

Defects use standalone/embedded `unity-bugfix`. `prepare_bug_context` reads the exact
bug task without starting a feature contract. Accepted facts use the same isolated
workers. `finalize_bugfix` closes exact tasks/authorized documents, never feature/stage
completion. Workers read neither `.ai/bugs/` nor feature cases; the developer passes
values and owns diagnostic/fix/review gates.

## Source Authority

Notion owns product requirements/plans/documentation. Local `ARCHITECTURE_RULES.md`,
`CONVENTIONS.md`, `SCENE_HIERARCHY_STANDARD.md` retain code/asset policy authority.
Notion standards govern page presentation. Behavioral/policy conflicts require explicit
decisions; this integration does not silently transfer architecture standards to Notion.

## Related Documents

- [Operation contract](operation-contract.md).
- [Feature workflow](../ai-workflow/workflow-reference.md).
- [Bugfix workflow](../ai-workflow/bugfix-reference.md).
- [Common standard](../ai-workflow/DOCUMENTATION_STANDARD.md).
