---
name: notion-project-coordination
description: Delegate TroughGame Notion intake, project documentation and task status updates to a separate subagent using isolated requests and results. Use when requirements come from Notion or accepted results must be published there.
---

# Notion Project Coordination

## Purpose and Scope

The main agent dispatches a bounded operation to a separate Notion worker with
its own journal and consumes the result. This skill never calls MCP directly.
Read the [contract](../../../docs/notion/operation-contract.md),
[project binding](../../../docs/notion/project.json),
[common standard](../../../docs/ai-workflow/DOCUMENTATION_STANDARD.md),
and, for development, [integration](../../../docs/notion/feature-integration.md).

## Dispatch and Operations

1. Choose one operation; build a request with stable ID for exact inputs/revision,
   allowed targets/actions, evidence, and necessary context. Identical retries keep
   the ID; changed inputs receive a new ID.
2. Spawn a separate subagent with `fork_turns: "none"`, repository root, absolute
   worker SKILL path, configuration, and request. Do not forward the whole transcript/
   development state. Supply known Python runtime; otherwise the worker discovers
   bundled dependencies before initialization. Override models only when instructed.
3. `bootstrap_project`, `prepare_feature_context`, `prepare_bug_context`,
   `refresh_source_context` use `notion-feature-intake`. `publish_progress`,
   `update_documentation`, `finalize_feature`, `finalize_bugfix` use
   `notion-project-documentation`.
4. Serialize writes to the same target/operation. Await the result, verify ID/hash,
   outcome, sources, confirmed changes. A final agent message alone is not proof
   of successful remote writes.
5. Consume immutable results. The main agent owns developer questions/gates;
   workers neither approve requirements nor advance development state. Present
   a concrete proposal/stop rule before necessary questions.

`phase: "preflight"` means incomplete/rejected input without a journal receipt.
Hash/identity describe received data or are unavailable. Repair inputs before
execution; preflight is not an executed remote operation.

## Ownership

Authorization covers separate Notion delegation, not parallel implementation agents,
arbitrary workspace edits, schema migrations, or messages/comments to people.
The main agent owns `.ai/features/` and `.ai/bugs/`; only workers use
`.ai/notion/operations/`. No queues/locks/receipt/retry status in development state.
A requirements snapshot is the developer's own contract, not shared state.
Never edit worker journals or provide development-state paths; pass facts/decisions
as values. Configuration is a static read-only input for both sides.

## Results and Recovery

| Status | Main-Agent Action |
| --- | --- |
| `completed` | Use data/confirmed changes; approvals still apply |
| `blocked` | Report exact access/mapping/input gap; continue independent authorized work |
| `conflict` | Show source/target delta; revise when behavior/scope/authority changes |
| `partial` / `failed` | Report confirmed facts; reconcile/retry the same ID via worker without duplicating successful creation |

If subagents are unavailable return `delegation_unavailable`; never substitute direct
main-agent MCP calls. Skills execute on request, without background schedules/infinite polling.
