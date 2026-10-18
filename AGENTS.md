# AGENTS.md

## Project Standards

Use the authoritative document for the task; do not duplicate its rules here:

- [Architecture](docs/ARCHITECTURE_RULES.md): contexts, features, ownership, DI and initialization.
- [Conventions](docs/CONVENTIONS.md): folders, naming, resources and code style.
- [Scene hierarchy](docs/SCENE_HIERARCHY_STANDARD.md): authored scene objects.
- [Runtime UI](docs/UI_TOOLKIT_STANDARD.md): UI Toolkit architecture and resources.

Keep `Shared` .NET-only and namespaces `Shared` or `ProjectCore.<Context>`.
Use DI and the unified initialization flow; avoid hidden cross-system startup chains.
Do not add `.asmdef` files or move `Assets/ProjectCore/Resources/ProjectContext.prefab`.

## Developer Interaction

- Respond in the developer's language. Write agent-instruction Markdown in English;
  actual project content follows its agreed language.
- Preserve accepted scope and prior authorization. Silence is not approval; do not
  ask again for an unchanged authorized action. Follow the selected skill's gates.
- Create only requested documents or files prescribed by the active skill.
  Keep root README short; details belong to their owning documentation.
- Delegate only with explicit authorization. Notion workers below are authorized;
  other subagent work requires developer authorization.

## Skill Selection

- New feature workflow: [unity-feature-development-v2](.agents/skills/unity-feature-development-v2/SKILL.md),
  only on a direct developer request to use it. Continue that feature through its
  flow; do not create workflow state for ordinary edits or tooling work.
- [unity-bugfix](.agents/skills/unity-bugfix/SKILL.md) is manual-only. Defects during
  feature/UI work remain corrections in the active subtask, without automatic invocation.
- [unity-ui-toolkit](.agents/skills/unity-ui-toolkit/SKILL.md) may be selected for relevant
  runtime UI work; it cannot start a feature or bugfix workflow.
- Feature-workflow Notion calls are limited to [requirements intake](.agents/skills/notion-feature-requirements/SKILL.md)
  and [whole-feature finalization](.agents/skills/notion-feature-finalization/SKILL.md).
  Use isolated Notion workers; the main agent never calls Notion MCP. Follow
  [integration rules](docs/notion/feature-integration.md); do not publish process history
  or invoke Notion for intermediate changes.

Preserve existing feature/bug records; do not silently migrate or restart them.
Skill-owned state, templates, response formats and stage transitions stay in the skills.

## Unity and Checks

Use installed `unity:unity-cli` for Editor operations with explicit project targeting.
Do not start a fallback Editor. Use `Tools/AI/validate-unity.ps1` for applicable
connected-Editor compilation/tests. Report blocking failures and unavailable required
checks honestly; do not claim unexecuted checks passed.

Run when relevant:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Validate-Architecture.ps1 -Mode Strict
dotnet build .\ProjectCore.Runtime.csproj --no-restore -v:minimal
git diff --check
```

## Git

- Preserve user work and unrelated staged changes. Never revert them without instruction.
- Preserve Unity GUIDs and keep `.meta` files with their assets during moves/renames.
- An accepted feature subtask or standalone bugfix may receive one scoped local commit
  after developer diff acceptance, required playtest/retest acceptance and passing
  applicable checks. Stage only accepted paths and related records.
- This permission excludes tooling commits, push, merge and release; those need
  separate developer authorization.
