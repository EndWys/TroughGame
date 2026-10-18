# Manual Bugfix Workflow

## Scope and Entry

Version **2.0**. The single process owner is
[unity-bugfix](../../.agents/skills/unity-bugfix/SKILL.md).
Its implicit invocation is disabled. Only a direct developer command to use the
skill or start/resume this workflow activates it. Merely reporting a defect,
requesting a correction or encountering a failed test never does.

## Process and Records

Analyze evidence → agree a minimal diagnostic action or exact fix → implement →
verify the original/adjacent behavior and review the diff → developer acceptance →
deliver. Existing authorization persists. Diagnostic approval does not authorize
a production fix; failed verification returns to analysis. Keep a short case, not
mandatory matrices, a separate audit report or a second state machine.
Standalone cases use `.ai/bugs/<bug-id>/case.md`; existing YAML remains resumable
but is not required for new cases. Embedded work stays in the current feature
artifact/case with one host checkpoint. A fix-and-continue instruction covers return.
Accepted standalone fixes retain scoped local commit permission after applicable
checks and accepted diff/retest. No push, merge, release or Notion operation.

## Feature and UI Corrections

Without a manual bugfix command, correct defects within the current approved scope
and existing host review/verification gates. Do not load this skill, create a bug
workflow, import its extra approval gates or ask the developer to activate it.
Material scope/design uncertainty still uses the host's normal decision process.
The manual-only boundary applies immediately to active work without replaying or
rewriting accepted history. Existing explicitly authorized bug work may continue.
