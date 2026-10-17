# BUG-001 — FEAT-01 Validation Blockers

Host: FEAT-01. Stage: verification_review. Revision: 1.
Status: approved fix applied and available retests passed; awaiting combined
FEAT-01 review/retest acceptance. No separate commit or bug closure claimed.
Approval source: «Подтверждаю, делай фикс бага и продолжай реализацию».
This also authorizes return to the existing feature implementation after the
approved correction's checks; it does not accept an unseen final diff/Play Mode.

## Expected and Actual

Expected: new LocalInputReaderTests process synthetic key events in Edit Mode,
and the complete implementation satisfies the required whitespace check.
Actual: full Edit Mode run completed with 647 total / 643 passed / 4 failed.
Four new input callback tests fail before their sequence assertions because
WasPressed is false. Geometry/phase/buffer and all existing tests passed.
`git diff --check` separately reports trailing spaces emitted by Unity in newly
added empty m_Name lines of the three edited player/enemy prefabs.

Evidence: `.ai/evidence/combat-system/feat01-edit-all-output.json`, its linked
connected Editor result; current git diff --check. Runtime build: 0 errors,
10 dependency warnings. Strict architecture: only 4 existing unrelated ASSET003
errors. Automated Solo Play fixture passed; this is not developer acceptance or
Host/client validation. Main source integration has not received combined audit/review.

## Confirmed Cause

LocalInputReaderTests.PressKeys queues KeyboardState and calls InputSystem.Update.
Installed Input System source at
`Library/PackageCache/com.unity.inputsystem@21a28c3a6c83/InputSystem/InputManager.cs:3105`
rejects player updates while not playing unless runPlayerUpdatesInEditMode is set.
Its defaultUpdateType at line 151 selects Editor updates outside Play Mode.
`InputSystem/Actions/InputActionState.cs:1359` explicitly returns without
processing action state changes during an Editor update. InputSystem.Update
delegates through defaultUpdateType. The new fixture provides
neither an isolated test runtime nor that opt-in. Callback sequence assertions
therefore have no delivered key-start event. This is a test-environment defect;
no production fix is proposed. Static SDK mechanism plus four deterministic
failures confirm the cause; the correction itself remains untested.

The second failure is literal whitespace in Editor-produced serialized text,
not a gameplay/configuration defect. No resource field/reference change is needed.

## Exact Fix Proposal — Revision 1

| Path / member | Proposed change | Scope and verification |
| --- | --- | --- |
| GameCore/Tests/Editor/LocalInputReaderTests.cs fields | Store original InputSettings and a temporary clone | Test-only ownership; no SDK/package/production edits |
| Same file SetUp | Clone settings, assign temporary InputSystem.settings, enable RUN_PLAYER_UPDATES_IN_EDIT_MODE through installed public SetInternalFeatureFlag | Existing action callbacks execute under the same synthetic input tests; no weakened assertions |
| Same file TearDown | Restore original settings and destroy clone after provider/device cleanup, including failed-test paths | No persistent project settings change; rerun full Edit Mode to detect leakage |
| TP Player prefab and both TP Enemy prefabs | Trim only trailing whitespace in Editor-created empty m_Name lines | Formatting-only exception to serialized-resource text editing; no asset creation/wiring/GUID/reference/data change. Requires explicit approval together with this exact scope |

The installed public InputSettings.SetInternalFeatureFlag API and its feature name
were verified in local package source. The flag is experimental/version-specific;
the correction stays confined to the fixture and current installed version.
No competing production-code change is supported by the evidence.

## Validation After Approval

1. Rerun LocalInputReaderTests (original four failures plus fixture regressions).
2. Rerun full Edit Mode; require all selected tests pass and no settings leakage.
3. Run connected Editor compilation, runtime dotnet build and git diff --check.
4. Confirm no semantic prefab diff was introduced by whitespace normalization.
5. Repeat scoped audit, present exact correction for developer review/retest;
   resume FEAT-01 only under the workflow's accepted return decision.

## Applied Result and Return

The approved fixture correction is applied without weakening assertions.
Only new empty m_Name lines are normalized in the three actor prefabs; original
blank-name formatting is preserved by serialized object ID. No values/references
or GUIDs changed as part of that formatting correction.

Original fixture retest: 10/10 passed, compared with the previous 4 failures.
After continuing the approved integration test coverage, final full Edit Mode:
653/653 passed. Automated Solo Play: 2/2 passed. Unity compilation passed within
those runs; runtime build 0 errors/8 dependency warnings; git diff --check passed.
Strict architecture still has only the existing 4 unrelated ASSET003 image errors.

Evidence:
- `.ai/evidence/combat-system/bug001-validation/20261016-003137-255-60da4972-EditMode/result.json`
- `.ai/evidence/combat-system/bug001-validation/20261016-003556-620-7a7d2f92-EditMode/result.json`
- `.ai/evidence/combat-system/bug001-validation/20261016-003712-893-cb2ed4e0-PlayMode/result.json`

APR-005's explicit instruction to continue authorizes return to FEAT-01
implementation after these checks; remaining planned fixture coverage was added.
Combined review revision 1 now covers this corrected diff and its original retest.
No approval of the unseen final diff or required manual/network Play Mode is inferred.
The host owns one eventual checkpoint after the complete feature-subtask gates.

Independent supported work already completed: A/B source integration and Unity
configuration; automated Solo phase/lock/expiry/chain/cancel fixture passed.
Not executed: Host/client, authority change, complete contact lifecycle, developer
Play Mode acceptance. No FEAT accepted commit or feature closure.
