# Notion and TroughGame Documentation

## Purpose and Roles

Three project-local skills provide Notion integration:

| Skill | Role |
| --- | --- |
| [notion-project-coordination](../../.agents/skills/notion-project-coordination/SKILL.md) | Main agent dispatches a separate worker and consumes its result |
| [notion-feature-intake](../../.agents/skills/notion-feature-intake/SKILL.md) | Worker reads stage/features/tasks/decisions and returns requirements/proposed acceptance criteria (AC) |
| [notion-project-documentation](../../.agents/skills/notion-project-documentation/SKILL.md) | Worker publishes accepted facts and updates documents/statuses |

Read-only Notion MCP preflight on 2026-10-12 verified the supplied
[tasks database](https://app.notion.com/p/3f6ad1247d58804ebf55f9040a53a8e7), its
TroughGame ancestor, and feature/document/idea relations. Static IDs/properties/
actual options are in [project.json](project.json). Re-fetch live schemas before use.
Use the existing enabled [TroughGame documentation standard](https://app.notion.com/p/3f7ad1247d5881409ce4f30510f6a1b0)
for page presentation; do not create a competing standard. The [requirements map](documentation-standard.md)
identifies relevant sections.

## Handoff and Ownership

Every dispatch uses the [contract](operation-contract.md): operation ID, exact targets,
authority, immutable facts, and verified-change result. The main agent never calls
Notion MCP. Workers never read/write `.ai/features/` or `.ai/bugs/`; journals live in
Git-ignored `.ai/notion/operations/`. `prepare_bug_context` prepares task context;
`finalize_bugfix` publishes accepted results/closes exact bug tasks, not features/stages.
No shared state machine or Notion sync fields in feature YAML. Isolation is defined
by requests/ownership, not a separate process sandbox.

## Invocation

Example requests, rendered in the developer's language during actual interaction:

- Prepare new-feature requirements from this Notion task URL.
- Inspect the current stage and propose the next ready Dev task.
- Publish the accepted result and update this feature's documentation.
- Update this document from verified changes without starting a feature.

## Approvals and Completion

Development gates persist. Task `Статус` is a `select` with `Backlog`, `Ready`,
`In progress`, `Review`, `Done`. Features have separate `Статус разработки`;
accepted requests specify actual intended options. `Объединена` is not Done.
Static inspection/owner acceptance is not an executed game/network test.
After final acceptance: current results/documents → verified task results → final
feature status. Partial failures retain worker receipts and reconcile actual effects
before retry. Do not repeat implementation to repair publication. See
[feature-integration.md](feature-integration.md).

## Checks and Limits

Local tool checks (Python 3.12+, standard library only):

```powershell
python -m unittest discover -s Tools/AI -p test_notion_operations.py -v
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\Test-WorkflowTools.ps1
```

Use bundled Python when absent from PATH. Skills run on request/from feature work;
no background scheduler, endless backlog processing, or separate Notion-agent service.
Schema/database creation, deletion, comments, notifications are not routine publication.
Local architecture remains authoritative; switching to mirrored standards requires
a separate decision. Local instruction formatting follows the
[common workflow standard](../ai-workflow/DOCUMENTATION_STANDARD.md).
