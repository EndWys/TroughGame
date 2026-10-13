# Feature State and Git

## Purpose

Read before start/resume, writes, review-to-commit transitions, and cleanup.
Keep verifiable state and preserve unrelated staged work.

## Ownership and Artifacts

The main agent owns `.ai/features/` and Git. Notion workers receive immutable values
and own separate journals. Queues, locks, publication status, and journal paths
never enter `state.yaml`. Copy source observations and the approved contract from
worker results into developer-owned `requirements.md`.

| File | Purpose |
| --- | --- |
| `state.yaml` | Compact stage/approval/Git/validation index |
| `requirements.md` | Accepted behavior, sources, acceptance criteria (AC) |
| `decisions.md` | Rationale for accepted/proposed decisions |
| `research.md` | Research and accepted plan |
| `subtasks/FEAT-XX.md` | Local architecture and applicable lifecycle map |
| `final-verification.md` | Requirement/AC coverage and results |
| `knowledge-promotion.md` | Permanent-rule candidates and decisions |

Record facts/questions/decisions/approvals/checks promptly; details belong in artifacts.
A record is not approval evidence. An interruption remains the current subtask:
origin, trigger, evidence, and return decision go in `interruptions`.
Defects use embedded `unity-bugfix` and `bugs/<bug-id>/case.md`; no second state;
the host creates the checkpoint once.

## Start and Reconciliation

Choose `feature-kebab-case`; use the template when state is absent, remove fictional
examples, read artifacts, reconcile Git before writing. Do not restart completed
stages without reason. Capture staged/unstaged/untracked paths, a stable tracked-diff
fingerprint, and visible stash refs; compare expected paths and approved scope.

- Match → `verified`; continue.
- Missing/changed expected path/fingerprint → mismatch. Before blocking, inspect
  visible stashes for recovery of this feature only.
- Clearly unrelated/generated paths enter `repository.ignored_paths` automatically;
  do not change/commit them or ask to classify obvious cases.
- Overlapping/expected paths or possible missing-work recovery cannot be ignored
  automatically; uncertainty blocks writes.
- If stash recovery is already authorized, first `git stash show`, then restore
  only exact missing non-conflicting paths, preserving stash and reconciling again.
  Do not use `stash apply`, `pop`, whole-stash recovery, or mixed/unclear sets.

## Accepted Commit

Review permits an automatic local commit only after recorded required Play Mode
success and applicable checks. Reconcile accepted changes/related state artifacts;
exclude unrelated paths and `.ai/evidence/`. Stage accepted paths only, verify the
index and unique `CP-*`. Format: `feat(<feature>): <short outcome>` or
`fix(<feature>): <short outcome>`, with `Workflow-Checkpoint: <marker>`.
Do not alter/include unrelated staged paths. Verify the marker and record actual
commit hash/subject.
Do not automatically commit requirements/plans, unaccepted architecture, rejected
changes, unresolved audit findings, or final merge decisions.

## Cleanup

After final acceptance, retrospective, and knowledge-promotion outcome use
`Tools/AI/cleanup-feature-evidence.ps1 -Feature <feature-kebab-case>`.
Record cleanup, remove deleted evidence links, retain actual statuses/decisions/
approvals/checkpoints. Do not clean active, blocked, or rejected features.
