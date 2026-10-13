# Fix Verification and Delivery

## Implementation and Regression

Implement the approved fix only. Inspect affected DI/lifecycle/ownership/async/
cancellation/subscriptions/authority/serialized references. Use the [readiness map](../../unity-feature-development/references/unity-lifecycle-safety.md),
without hidden `Awake`/`Start` startup chains. Modify assets through the connected
Editor per the CLI skill; source files may be edited directly.
Regression verifies the violated observable contract. Prefer failure before and
success after; disclose if comparison was not run. Proven diagnostic tests may remain
as regression. Do not mirror implementation in tests or equate compilation with
remediation. For unavoidable manual cases retain exact scenario/actual result.
Remove only owned temporary probes. Retained diagnostics/reason/overhead must be
accepted and visible. Preserve `.meta`, asset paths, and assemblies.

## Checks and Acceptance

Run applicable AGENTS checks and `Tools/AI/validate-unity.ps1` with narrow modes/filters
and actual evidence. Repeat original/adjacent behavior. Network/lifecycle fixes cover
applicable roles and spawn/despawn/scene/reuse transitions. Compare performance in
equivalent conditions accounting for instrumentation. Unavailable/untested is not `passed`.
Separately inspect the whole relevant final code/Unity diff using [P0/P1/P2 audit](../../unity-feature-development/references/ai-code-audit.md).
A second reviewer requires authorized delegation. Required findings return to correction.
Before acceptance show exact changes, reviewed scope, original/regression results,
and limits. Execute/obtain required Play Mode/retest. Existing results for the exact
same revision may be reused with executor attribution; otherwise retest. Explicit
acceptance precedes delivery; code review is not Play Mode acceptance.

## Commit

Standalone: after accepted diff, accepted required retest/Play Mode, and passing
checks, make one `fix(<bug-id>): <outcome>` commit. The approved process permits this
local commit, not tooling setup/push/merge/release. Stage accepted fix/test/case paths
only; inspect the index and preserve unrelated staged work. Do not commit temporary
evidence or Notion journals.
Embedded: the host owns reconciliation/subtask checkpoint and includes the correction
once after its gates. No separate bug commit before/alongside it. Record actual commit
references in the case. Publication failure does not undo accepted commits or require
replaying implementation.

## Notion Handoff

If needed, `prepare_bug_context` reads the exact bug task; refresh compares sources.
After standalone acceptance send `finalize_bugfix`: exact tasks, expectation, cause/
mechanism, selected fix, reproduction/regression, mandatory checks/Play Mode, approvals,
limits, actual commits. Required fields are defined in `docs/notion/operation-contract.md`.
Workers update authorized bug tasks/documents only, not feature/stage completion.
Embedded results normally use parent progress/documentation; `finalize_bugfix` requires
a separate accepted bug task. Preserve historical requirements. Development acceptance
and publication are independent: report confirmed/`partial`/`blocked`, retry via worker.
