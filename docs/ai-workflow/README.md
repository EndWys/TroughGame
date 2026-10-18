# TroughGame Workflows

## Purpose

Feature workflow version: **1.7-tg.1**. Manual bugfix workflow version: **2.0**.
Version compatibility is recorded in [CHANGELOG.md](CHANGELOG.md).

| Workflow | Reference | Skill |
| --- | --- | --- |
| New feature | [workflow-reference.md](workflow-reference.md) | [unity-feature-development](../../.agents/skills/unity-feature-development/SKILL.md) |
| Standalone/embedded bug | [bugfix-reference.md](bugfix-reference.md) | [unity-bugfix](../../.agents/skills/unity-bugfix/SKILL.md) |
| Runtime UI specialization | [UI_TOOLKIT_STANDARD.md](../UI_TOOLKIT_STANDARD.md) | [unity-ui-toolkit](../../.agents/skills/unity-ui-toolkit/SKILL.md) |
| Notion sources/publication | [notion/README.md](../notion/README.md) | Two explicit workers: requirements intake and whole-feature finalization |

## Invocation and Roles

Start a new feature workflow only by explicitly requesting $unity-feature-development,
supplying specification text/document/Notion URL. Implicit invocation is disabled;
generic feature requests or other skills do not activate it. After that first manual
invocation, the same feature continues through its flow without repeated invocation.
Unapproved requirements first become an approval proposal.
For continuation supply feature ID/state and reuse its accepted local requirements.
Selected Notion sources are read only at explicit requirements intake.
Only explicitly use $unity-bugfix or command the bugfix workflow when that separate
process is desired. Generic fix requests and feature/UI defects do not trigger it.
A manually requested process needs a description and expected behavior; a Notion
task URL does not trigger a worker. Embedded defects reuse the
same FEAT/interruption/state. The developer approves requirements, plan, architecture,
diff, Play Mode, and readiness. The agent works between gates. Accepted subtasks
commit after required passed checks/Play Mode. Push/merge/release need separate
requests. Notion workers never receive development state; optional recovery receipts
are separate from feature state.

`unity-ui-toolkit` remains automatically selectable for a screen, popup, widget or
UI-related defect; no manual skill command is required. It operates
inside the current feature/bugfix task with no independent state. Its consolidated
hierarchy/resource/presentation plan uses the host approval; MVVM is the default
and justified MVP choices are accepted there. New UI adds visual prototype
acceptance before complete logic. Stage-specific response forms show architecture,
changes, evidence and the next action. Tooling installation does not start a task.

## Files and Tools

`.ai/features/_template/` and `.ai/bugs/_template/` are templates, not started tasks.
Temporary JSON/logs/reports belong in `.ai/evidence/`; Notion receipts belong in
independent ignored `.ai/notion/operations/`. Retain active-work facts until authorized
cleanup. Tools and regression tests are part of the working workflow.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\validate-unity.ps1 -Mode Diagnose
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\validate-unity.ps1 -Mode Compile
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\validate-unity.ps1 -Mode EditMode -Filter ProjectCore
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\workflow-audit.ps1 -Feature <feature-kebab-case>
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\cleanup-feature-evidence.ps1 -Feature <feature-kebab-case> -WhatIf
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\Test-WorkflowTools.ps1
```

`Relevant` compiles/runs Editor tests; `-Filter` narrows selection. Automated `PlayMode`
does not replace manual scenario acceptance. Use the connected project Editor.
Resolve CLI through parameter, `TROUGHGAME_UNITY_CLI_PATH`, PATH, or standard Windows
installation. Unavailable/incomplete results are not `passed`. Structural audit does
not prove every approval/AC; the agent checks them separately. Cleanup verifies paths
and supports `-WhatIf`.
