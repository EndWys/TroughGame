---
name: notion-feature-requirements
description: Collect TroughGame feature requirements from selected Notion pages only when explicitly dispatched at the feature requirements stage. Read-only; never run for resume, implementation, patches or bugfixes.
---

# Notion Feature Requirements

## Invocation and Ownership

Use only at `requirements` when the developer selected Notion sources, or explicitly
requested a replacement requirements intake. A task URL elsewhere is not a trigger.
The main agent sends one isolated subagent (`fork_turns: "none"`) this skill's absolute
path, repository root, selected feature/source IDs, URLs or page titles, and specific questions.
Use the [handoff](../../../docs/notion/operation-contract.md). No coordinator skill.
The worker returns facts; it never opens `.ai/features/`, `.ai/bugs/`, code, Git or Unity.
If delegation is unavailable, report it; the main agent does not call Notion MCP.

## Worker Task

1. Read [MCP rules](../../../docs/notion/mcp-procedure.md),
   [project binding](../../../docs/notion/project.json) and the live
   [project documentation standard](https://app.notion.com/p/3f7ad1247d5881409ce4f30510f6a1b0).
   Use Notion MCP only. Resolve exact sources and relevant linked specifications,
   plans, decisions and tasks; stay within the selected feature. Resolve a supplied
   page title through bounded project search. If multiple candidates fit, return
   their URLs for developer selection instead of choosing by guesswork.
2. Collect goal, explicit requirements, exclusions, dependencies, accepted decisions,
   task IDs, and observable acceptance criteria. Cite source URL/section for each
   requirement. Label proposed criteria and missing or conflicting information.
   Page statuses do not establish developer approval.
3. Return one concise packet with sources, requirements/criteria and blocking questions.
   Report inaccessible or incomplete material sources; do not invent their contents.

## Completion and Limits

Return `completed` only for a usable source-backed packet; otherwise `blocked` with
the exact missing source/access/question. No remote writes, task claiming, status
changes, bootstrap, automatic refresh, progress publication, schedules or polling.
The developer agent owns the approved local contract and all later decisions.
Resume, subtask boundaries and code changes reuse that contract without a Notion call.
