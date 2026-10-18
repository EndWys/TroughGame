---
name: unity-feature-verification-v2
description: Prepare observable Play Mode scenarios for the developer to accept one reviewed TroughGame subtask. Run only when explicitly dispatched at the new workflow's verification stage or directly requested. Do not execute tests, fix code or start another stage.
---

# Developer Playtest V2

## Entry and Inputs

Run at `verification` with permission to enter this stage, or on a direct developer
request to use this skill. Inputs are the feature ID, selected `FEAT-*`, accepted
requirements/plan/architecture and developer-accepted implementation. This stage
prepares the developer's playtest and receives their result; it does not repeat
technical checks from implementation or diff review.

## Task

1. Derive a focused set of playtest cases from the selected subtask's agreed behavior
   and acceptance criteria. Include relevant adjacent/edge cases and network roles
   only where needed. Inspect the implementation as necessary to identify the actual
   controls, scene and observable output; do not invent requirements or test features
   that this subtask has not implemented yet.
2. Give each case clear starting conditions, developer actions and a mandatory
   observable expected result. Prefer visible game/UI behavior. For intermediate
   subtasks without a final presentation, existing debug visualization or logs are
   valid: name exactly what should appear and where to look. Do not claim such output
   exists without checking. If no observation is available, report that gap to the
   coordinator instead of inventing instrumentation or declaring the case testable.
3. Present the cases and wait for the developer's result. A confirmation that all
   cases behave as described is acceptance. For a mismatch, identify the case and
   expected versus reported actual behavior; ask only for missing information needed
   to understand that mismatch. Return issues to the coordinator without diagnosing
   or fixing them. After a correction, provide only the affected cases for retest.

## Response Standard

Use the developer's language. Provide the scenarios as a list; each case contains:

- **Case ID and name:** stable IDs such as `CASE-01`, preserved during retest.
- **Starting conditions:** scene, setup and peer roles only as needed to run it.
- **Actions:** short ordered steps the developer can follow.
- **Expected result:** concrete visible behavior for the important steps/outcome;
  for debug/log-based cases, the exact observable signal and its location.

End with one request: confirm that all cases match the described behavior, or name
the failing case IDs and describe what happened instead. Do not require a full test
report, screenshots or logs for every case; ask for them only to clarify a failure.
No routine test/build summaries or separate results table. On the developer's reply,
acknowledge acceptance briefly or summarize the specific mismatches and missing data.

## Output and Completion

The output is the case list and the developer's acceptance or reported mismatches
in chat. The coordinator records the result and handles authorized transitions or
corrections. Missing results or failing cases keep acceptance pending; silence and
earlier diff acceptance are not a playtest result. No extra approval is needed after
the developer explicitly confirms that all cases match.

No test/build runs, Editor control, code/asset edits, instrumentation, MD reports,
state edits, Notion calls, subagents or Git mutations belong here. Do not invoke
`unity-bugfix` or start another stage. This acceptance concerns only the selected
subtask, not completion of the whole feature.
