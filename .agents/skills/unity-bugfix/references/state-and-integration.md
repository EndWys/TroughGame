# Bug State and Feature Integration

## Purpose and Standalone Bugs

Use `.ai/bugs/<bug-kebab-id>/state.yaml` and `case.md` from `_template`.
Replace ID/version, omit irrelevant examples. State indexes stage/revision/approvals/
Git/checks. Case holds reproduction/cause/hypotheses/diagnostic plan/fix options/diff/
results. Do not create a feature plan.
Before writes/commits reconcile expected diff and staged/unstaged/untracked paths.
Preserve user work; unclear overlap needs a decision. Apply [Git reconciliation](../../unity-feature-development/references/state-and-git.md)
to bug paths, without feature setup/markers/gates. Resume approved revisions; do not restart blindly.

## Embedded Feature Defects

Establish relevance to the current implementation/check/audit/review/Play Mode:

1. Reuse an existing interruption or record one for current FEAT, with origin stage/
   trigger, preserving the accepted plan.
2. Keep `bugs/<bug-id>/case.md` inside the feature. Its header has stage/revision;
   decisions/evidence live in the case and existing host indexes. No second state
   or standalone `.ai/bugs/<bug-id>/`.
3. Baseline is accepted host behavior/scope. Scope/design changes use host gates,
   not another subtask.
4. Index approvals as `bug_diagnostic_plan`/`bug_fix_plan` with exact subject/revision.
   Use `defect_investigation` when no more precise interruption kind applies.
   These do not replace host architecture/review/Play Mode gates.
5. Corrected host audit/review/retest may cover both processes only with an explicit
   match of final diff, scenarios/peer roles, and required evidence. No double commit.
6. After acceptance record completion evidence and `return_pending`. Await explicit
   return; one accepted-and-continue message may cover both decisions. The host resumes
   the appropriate stage and creates one checkpoint.

Do not commit the bug separately before host diff acceptance. A fix neither completes
the feature nor removes its other requirements.

## Iterations and Failures

Probes record actual outcomes. Failed retest returns to analysis/approved correction,
not commit. Obsolete mechanism/scope/diff approval cannot be reused; retain history
and revision. Changed/undefined original requirements are resolved by the contract owner.

## Notion Isolation

The developer owns case/state. Workers never open `.ai/bugs/` or `.ai/features/`;
send necessary accepted values only. Publication receipts/queues/locks stay out of
development state. A task URL is a source, not shared state. Retry uses the coordinator
and independent worker journal.
