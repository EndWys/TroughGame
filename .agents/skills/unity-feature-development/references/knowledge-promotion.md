# Project Knowledge Promotion

## Purpose

After final acceptance/retrospective, decide which proven reusable lesson becomes
a permanent rule. This does not reopen implementation, create subtasks, or change
the product without a decision. The developer approves each candidate; the agent
applies the exact accepted change.

## Candidates and Artifact

Up to three candidates: an explicitly supported direction, independently evidenced
rule, or material recurring risk. Exclude one-off workarounds, unaccepted guesses,
feature-specific rules, temporary tool failures, and preferences.
Details go in `knowledge-promotion.md`, a compact index in state.

| Field | Content |
| --- | --- |
| ID | Stable `KNP-XXX` |
| Fact/evidence | Observation and source/decision/check |
| Rule/applicability | Exact verifiable wording and bounds |
| Owner/conflict | One canonical file, related rules/duplicates/conflicts |
| Effect | Reduced risk/manual work |
| Decision | `proposed`, `approved`, `rejected`, `change_requested`, `applied` |
| Result | Paths, validation, separate actual commit |

## Owner Selection

| Topic | Document / Tool |
| --- | --- |
| Mandatory overall policy | `AGENTS.md` |
| Architecture/ownership/lifecycle/dependencies | `docs/ARCHITECTURE_RULES.md` |
| Style/structure/logging | `docs/CONVENTIONS.md` |
| Stable discovery map | `docs/project-map.md` |
| Stages/gates/evidence | workflow-reference or relevant reference |
| Response presentation | response-style / stage-deliverables |
| Deterministic action | Responsible `Tools/AI/` script |

Do not copy rules across documents. Present duplicates/conflicts and obtain a
decision rather than silently reconciling policy.

## Approval and Application

1. Read retrospective/audit/source evidence and check applicability.
2. Inspect the canonical target and related rules for duplicates/conflicts.
3. Show fact, evidence, exact rule, scope, owner, effect, and concrete change.
4. Await approval/rejection/changes; silence is not a decision.
5. Change only the accepted target and necessary links; run appropriate checks,
   including `git diff --check`.
6. Record workflow semantic changes in CHANGELOG; unrelated knowledge needs no
   workflow history entry.
7. Record result and a separate commit. Candidate approval authorizes that commit
   without another question; changed rule/target requires revision.

No candidates → `not_applicable`; rejection also completes this stage without
policy changes. A Notion target is updated by its separate worker.

## Versions and Resume

New features use the current CHANGELOG version. Do not rewrite existing state
automatically. Changes to gates/state contract/approval semantics require a choice:
continue the old process or migrate explicitly. Editorial/language changes alone
do not require migration.
