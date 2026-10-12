# Feature State and Git Reference

## Purpose

Use this reference before starting or resuming a feature and before every write, review-to-commit or evidence-cleanup stage. It preserves a resumable factual record without staging unrelated work.

## Feature artifacts

Notion is accessed through a dedicated worker and immutable packets. Its
operation journal is independent of these artifacts. Do not put Notion queues,
locks, publication statuses or journal paths in `state.yaml`, and do not let
workers read/write `.ai/features/`. Copy source references and the approved
snapshot into `requirements.md` from the returned values. Only the development
agent owns feature artifacts and Git.

Each feature directory contains:

- `state.yaml` — compact factual index of status, approvals, repository snapshot and evidence;
- `requirements.md` — approved source-traceable requirements contract and Acceptance Criteria;
- `decisions.md` — long-form approved or pending decisions;
- `research.md` — approved Research Brief and its source-backed findings;
- `subtasks/FEAT-XX.md` — approved local architecture, including lifecycle map when applicable.
- `final-verification.md` — final requirement/AC traceability and evidence status.
- `knowledge-promotion.md` — eligible post-feature candidates, their evidence, developer decisions and applied result.

Record artifact references and statuses in `state.yaml`; do not duplicate their long-form content there. Record approvals, decisions, questions, stage results and evidence immediately after they occur. A state record is not approval evidence by itself.

When developer feedback, review changes or a defect interrupts the main workflow, record an `interruptions` entry with its origin stage, current subtask, trigger, completion evidence and return decision. It remains part of the same task and subtask; do not create a new feature or subtask for it.

## Start or resume

1. Determine a stable lowercase kebab-case feature identifier.
2. Create the feature directory from `.ai/features/_template/` when it does not exist; remove illustrative decision records that are not real.
3. Read `state.yaml`, `requirements.md`, `decisions.md`, `research.md`, `knowledge-promotion.md` and the current subtask artifact when they exist.
4. Reconcile the repository before any write. Do not restart a completed stage without a recorded reason.

## Reconciliation

Collect staged, unstaged and untracked paths, a deterministic fingerprint of the tracked diff and visible stash refs. Compare expected feature paths and the current approved scope with the observed state.

- A matching snapshot can be marked `verified` and work may continue.
- A missing or changed expected feature path/fingerprint is a mismatch. Inspect visible stashes for a matching feature-only recovery candidate before blocking.
- Automatically record an observed path in `repository.ignored_paths` when it is demonstrably outside the approved feature scope. It remains untouched and is never staged or committed. Do not ask the developer to classify such a path.
- A path that overlaps the approved scope, is expected by state, or can plausibly contain a missing/altered feature change is not ignored automatically. Stop when its state cannot be established safely.
- When the developer has authorized automatic stash recovery, inspect a candidate with `git stash show` before restoring. Restore only exact missing expected feature paths from a non-conflicting candidate, preserve the stash, and reconcile again. Never use `stash apply`, `pop` or a whole-stash restore; never recover mixed or uncertain paths.

## Commit after Play Mode acceptance

The developer's review acceptance authorizes automatic commit only after the relevant required Play Mode scenario has passed and that result is recorded.

1. Reconcile Git and confirm the candidate paths are the accepted subtask diff plus related feature-state artifacts. Ignore and exclude every path outside the approved scope. Never stage `.ai/evidence/`.
2. Stage only those approved paths, inspect the staged diff and add a unique `CP-*` checkpoint marker to state.
3. Commit only the candidate paths with `feat(<feature>): <subtask summary>` or `fix(<feature>): <subtask summary>`, including `Workflow-Checkpoint: <marker>`. Preserve unrelated staged paths rather than modifying or committing them.
4. Verify the commit contains the marker, then record hash and subject in state.

Do not auto-commit requirements, plans, unapproved architecture, rejected work, unresolved audit findings or the final merge decision.

## Evidence cleanup

After final readiness, retrospective and the recorded knowledge-promotion outcome, use only `Tools/AI/cleanup-feature-evidence.ps1 -Feature <feature-kebab-case>`. Record the result, clear removed evidence paths and retain factual status, approvals, checkpoints and decision history. Do not clean evidence for an active, blocked or rejected feature.
