# TroughGame Workflows

## Purpose

Feature workflow version: **1.4-tg.1**. Bugfix workflow version: **1.0**.
Follow the [common standard](DOCUMENTATION_STANDARD.md); version compatibility is
recorded in [CHANGELOG.md](CHANGELOG.md).

| Workflow | Reference | Skill |
| --- | --- | --- |
| New feature | [workflow-reference.md](workflow-reference.md) | [unity-feature-development](../../.agents/skills/unity-feature-development/SKILL.md) |
| Standalone/embedded bug | [bugfix-reference.md](bugfix-reference.md) | [unity-bugfix](../../.agents/skills/unity-bugfix/SKILL.md) |
| Notion sources/publication | [notion/README.md](../notion/README.md) | Coordination, intake, and publication through separate workers |

## Invocation and Roles

Example: use $unity-feature-development for a new feature, supplying specification
text/document/Notion URL. Unapproved requirements first become an approval proposal.
For continuation supply feature ID/state. Notion may prepare the next ready Dev task
in the current stage after verifying dependencies.
Use $unity-bugfix with a description or Notion task URL. Embedded defects reuse the
same FEAT/interruption/state. The developer approves requirements, plan, architecture,
diff, Play Mode, and readiness. The agent works between gates. Accepted subtasks
commit after required passed checks/Play Mode. Push/merge/release need separate
requests. Notion owns independent journals and never receives development state.

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
