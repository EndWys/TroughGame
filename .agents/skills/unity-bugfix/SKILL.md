---
name: unity-bugfix
description: Run the TroughGame bugfix process only when the developer explicitly requests this skill or starts/resumes the bugfix workflow. Never select automatically for a defect, failed test, review feedback or feature correction.
---

# Manual Bugfix Workflow

## Invocation

Start only on a direct developer command naming `$unity-bugfix`, this skill, or
explicitly starting/resuming the bugfix workflow. A generic request to fix code,
reported bug, failed check, task URL or feature-workflow reference is not a command
to invoke this skill. Never invoke it automatically from feature or UI work.
Once manually started, continue the same authorized bug process across turns without
asking to invoke it again. Do not treat an old automatic invocation as authorization
to resume it. The developer can return to ordinary feature work at any time.

## Process

1. **Analyze.** Establish expected/actual behavior, a minimal reproducer and available
   evidence. Inspect existing code/logs first. Explain the likely cause and confidence;
   distinguish a confirmed mechanism from a hypothesis. Ask only blocking questions.
2. **Agree the next action.** A confirmed cause leads to an exact fix proposal:
   affected files/objects, before/after behavior, risks and checks. If evidence is
   insufficient, propose the smallest useful diagnostic action with expected outcomes,
   impact and a stop condition. Get approval for new probes or the fix unless already
   authorized. Diagnostic approval does not authorize a production fix. Do not invent
   alternative fixes or repeat approval for an unchanged plan.
3. **Fix.** Implement the agreed correction within scope. Remove temporary probes
   unless their retention is accepted. Changed scope or an invalidated hypothesis
   returns to analysis/proposal; do not silently redesign the feature.
4. **Verify and accept.** Run relevant project checks, repeat the original case and
   check affected adjacent behavior. Review the final diff and report actual results
   and limitations. Obtain developer diff acceptance and required retest/Play Mode
   acceptance; one response can cover both when its evidence is explicit. Failed
   retest returns to analysis. Compilation alone does not prove the defect fixed.
5. **Deliver.** Summarize cause, correction and verification. For an embedded fix,
   return to the same feature subtask/checkpoint under its existing gates. A developer
   instruction to fix and continue already authorizes that return; no extra ceremony.

## Records and Integration

Keep one short case: expected/actual, cause/evidence, agreed action, changes and
verification. No mandatory response tables, likelihood/complexity matrix, separate
audit report or repeated stage receipts. Use a table only when it improves clarity.
For standalone work use `.ai/bugs/<bug-id>/case.md`; new work needs no state YAML.
For a manually requested embedded process reuse the current feature artifact or its
existing bug case. Never create a new FEAT or duplicate state/evidence/checkpoints.
Preserve existing case/state/approvals on resume; do not migrate or regenerate them.

Follow AGENTS and local architecture/conventions. Use `unity:unity-cli` for Editor
operations and UI Toolkit guidance when relevant; do not start a fallback Editor.
Apply relevant architecture/build/diff and connected-Editor checks. State unavailable
checks honestly and add a regression test only when it meaningfully protects behavior.
After accepted standalone diff/retest and passing checks, the existing permission for
one scoped local fix commit remains; preserve unrelated staged work. Embedded commits
belong to the host. No push/merge/release or Notion invocation is authorized here.
