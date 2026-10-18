# TroughGame Documentation

This index identifies authoritative standards and capability guides. Update the
owning document when a rule changes; other documents reference that owner.

## Authoritative Standards

| Topic | Owner |
| --- | --- |
| Contexts/features, ownership, dependencies, DI, initialization | [ARCHITECTURE_RULES.md](ARCHITECTURE_RULES.md) |
| Owner folders, categories, suffixes, naming, style, resource names | [CONVENTIONS.md](CONVENTIONS.md) |
| Authored gameplay scene hierarchy | [SCENE_HIERARCHY_STANDARD.md](SCENE_HIERARCHY_STANDARD.md) |
| Runtime UI Toolkit presentation (MVVM default, scoped MVP), binding and lifetime | [UI_TOOLKIT_STANDARD.md](UI_TOOLKIT_STANDARD.md) |
| Validator commands, diagnostic codes, fixtures, analysis limits | [Tools/Architecture/README.md](../Tools/Architecture/README.md) |
| Agent instructions/mandatory checks | [AGENTS.md](../AGENTS.md) |
| New-feature stages/gates/state | [unity-feature-development-v2](../.agents/skills/unity-feature-development-v2/SKILL.md) |
| Bug diagnosis/accepted fixes/feature interruptions | [bugfix-reference.md](ai-workflow/bugfix-reference.md) |
| Workflow discovery/integration | [project-overview.md](project-overview.md), [project-map.md](project-map.md) |
| Separate Notion workers/documentation/development integration | [notion/README.md](notion/README.md) |

Invocation is described in [ai-workflow/README.md](ai-workflow/README.md).
Architecture/style remain governed by existing standards above. Root README stays
brief; it and AGENTS reference policy rather than duplicating complete rule tables.

## Capability Guides

- [Network Entities](features/NETWORK_ENTITIES.md): uint IDs, protocol, lifetime,
  component-query API.
- [Local Config](features/LOCAL_CONFIG.md): catalogs, lookup, config ownership.
- [Logging](features/LOGGING.md): feature categories, logger lifetime, handlers.
- Template feature READMEs: [Cheat](../Assets/ProjectCore/Contexts/Template/Features/Cheat/README.md),
  [Command Line](../Assets/ProjectCore/Contexts/Template/Features/CommandLine/README.md),
  [Debug Tools](../Assets/ProjectCore/Contexts/Template/Features/DebugTools/README.md),
  [Debug Console](../Assets/ProjectCore/Contexts/Template/Features/DebugConsole/README.md),
  [Debug Visualization](../Assets/ProjectCore/Contexts/Template/Features/DebugVisualization/README.md),
  [Loading Screen](../Assets/ProjectCore/Contexts/Template/Features/LoadingScreen/README.md),
  [Local Save](../Assets/ProjectCore/Contexts/Template/Features/LocalSave/README.md),
  [Popup](../Assets/ProjectCore/Contexts/Template/Features/Popup/README.md),
  [Screen Navigation](../Assets/ProjectCore/Contexts/Template/Features/ScreenNavigation/README.md),
  [Scene Flow](../Assets/ProjectCore/Contexts/Template/Features/SceneFlow/README.md),
  and [Scene Definition window](../Assets/ProjectCore/Contexts/Template/Features/SceneFlow/SCENE_DEFINITION_WINDOW.md).
