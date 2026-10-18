---
name: notion-feature-finalization
description: Synchronize TroughGame tasks, feature status and concise accepted specification deltas only at explicit notion_finalization after full feature acceptance, before retrospective. Never publish intermediate progress or bugfix steps.
---

# Notion Feature Finalization

## Invocation and Ownership

Use only at `notion_finalization`: the whole feature, all planned subtasks, required
review/retests/Play Mode and final verification have passed, and the developer has
accepted final readiness. A completed FEAT, patch or bug is insufficient.
The main agent supplies the readiness acceptance, exact feature/task/document targets,
desired task/feature outcomes and short accepted decisions absent from the original
specification/plan. Pass values, never local development-state paths or transcripts.
Dispatch one isolated subagent (`fork_turns: "none"`) with this skill's absolute path
and [handoff](../../../docs/notion/operation-contract.md). No coordinator skill.
The worker does not inspect code, Git, Unity, `.ai/features/` or `.ai/bugs/`, or decide
readiness. Missing full-feature acceptance means `blocked` before MCP actions.

## Worker Task

1. Read [MCP rules](../../../docs/notion/mcp-procedure.md),
   [binding](../../../docs/notion/project.json) and the live
   [project documentation standard](https://app.notion.com/p/3f7ad1247d5881409ce4f30510f6a1b0).
   Verify current target content, relations and schemas using Notion MCP only.
2. Update existing canonical documents only for supplied accepted decisions that
   clarify/change the original specification or plan. Make short, concrete edits;
   skip documents with no such delta. Preserve historical requirements and human edits.
3. Update only supplied accepted task statuses and the feature's actual business
   status using existing schema options. Documents first, task statuses next, feature
   status last. Do not close unrelated/deferred tasks or an entire project stage.
4. Re-fetch changed content/properties and return confirmed effects and unresolved
   targets. If already applied, verify and report unchanged; never append duplicates.
   For ambiguous failures inspect once before retry; unresolved effects mean `partial`.

## Completion and Limits

No workflow journals, FEAT/review/diagnostic steps, approval receipts, audits, test
matrices/logs or retrospective reports in Notion. Do not create pages/tasks, change
schemas, delete/archive/move entities, comment or notify people. Moving a board card
means changing its existing status property. Do not restyle or clean historical pages.
Use `completed` only when all requested effects are verified (or already match).
Otherwise return `blocked`, `conflict` or `partial` with exact confirmed/pending changes.
Retry only pending finalization; never restart implementation. No background operation.
