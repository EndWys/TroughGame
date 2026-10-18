# TroughGame Workflows

The active feature workflow is the accepted V2 skill pack. Its coordinator owns
stage routing and its stage skills own outputs and response formats.

| Workflow | Entry / reference |
| --- | --- |
| New feature | [unity-feature-development-v2](../../.agents/skills/unity-feature-development-v2/SKILL.md) |
| Manual bugfix | [unity-bugfix](../../.agents/skills/unity-bugfix/SKILL.md), [bugfix reference](bugfix-reference.md) |
| Runtime UI specialization | [unity-ui-toolkit](../../.agents/skills/unity-ui-toolkit/SKILL.md) |
| Notion requirements/finalization | [Notion integration](../notion/feature-integration.md) |

Start a feature by explicitly requesting `$unity-feature-development-v2` and supplying
its requirements/source. Feature templates live with the V2 coordinator and stage
skills. Invocation and Git permissions are defined in [AGENTS.md](../../AGENTS.md).

## Tools

- `Tools/AI/validate-unity.ps1`: connected-Editor compilation and scoped EditMode/PlayMode tests.
- `Tools/Architecture/Validate-Architecture.ps1`: project architecture validation.
- `Tools/AI/Test-WorkflowTools.ps1`: regression checks for the Unity validation helper.

[CHANGELOG.md](CHANGELOG.md) records workflow changes.
