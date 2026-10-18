# Notion and TroughGame

## Two Explicit Skills

| Skill | Invocation |
| --- | --- |
| [notion-feature-requirements](../../.agents/skills/notion-feature-requirements/SKILL.md) | Selected Notion sources at feature requirements intake; read-only |
| [notion-feature-finalization](../../.agents/skills/notion-feature-finalization/SKILL.md) | Whole-feature acceptance, before retrospective; exact statuses and concise decisions |

Implicit invocation is disabled. No coordinator, progress publication, bugfix operation,
automatic refresh, scheduler or mandatory journal helper remains.
Main agents dispatch isolated workers only at these two points. No Notion operation
is required for code edits, patches, accepted subtasks or routine resume.

## Rules

- [Integration and publication boundary](feature-integration.md).
- [Small worker handoff](operation-contract.md).
- [Notion MCP procedure](mcp-procedure.md).
- [Live project documentation standard](documentation-standard.md).
- [Static project IDs](project.json), verified live only as needed by the worker.

This tooling change does not edit Notion pages or repair historical remote content.
