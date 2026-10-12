---
name: notion-project-coordination
description: Delegate TroughGame Notion intake, project documentation, and task status updates to a separate subagent using isolated requests and results. Use when development starts from Notion or its results must be published there.
---

# Notion Project Coordination

Dispatch bounded Notion operations. The developer requested that Notion actions
run in a separate subagent with its own journal, independent of feature state.
This skill belongs to the caller; it does not call Notion MCP itself.

Read [the operation contract](../../../docs/notion/operation-contract.md) and
[project binding](../../../docs/notion/project.json). For a feature, also read
[workflow integration](../../../docs/notion/feature-integration.md).

## Dispatch

1. Choose one operation and form the contract's request. Use an operation ID
   stable for that exact input/revision; preserve it when retrying. Include the
   user-authorized destination, permitted actions, evidence, and only the
   context needed for this operation.
2. Spawn a dedicated subagent with `fork_turns: "none"`. Pass the repository
   root, absolute worker SKILL path, static configuration path, and request.
   Do not pass the whole conversation or a feature state file. Do not select a
   different model unless the developer or applicable instructions require it.
   Include the resolved Python runtime path when known; otherwise the worker
   locates workspace dependencies before initializing its journal.
3. Route `bootstrap_project`, `prepare_feature_context`, and
   `refresh_source_context` to `notion-feature-intake`. Route `publish_progress`,
   `update_documentation`, and `finalize_feature` to
   `notion-project-documentation`.
4. Serialize writes for the same remote pages. Wait for the result, inspect its
   operation ID, request fingerprint, outcome, source references, and verified
   changes. An agent final message alone is not evidence that remote writes
   succeeded.
5. Consume the returned immutable result. The caller owns developer-facing
   questions and feature gates; a worker never approves requirements or marks
   a feature workflow complete. Include the exact document/skill rule causing
   an approval request and make the proposed change reviewable first.

A `phase: "preflight"` result reports rejected/incomplete input without a
journal receipt. Its identity/hash refer only to the received input (or are null
when absent). Resolve the stated input problem before dispatching execution;
never treat that packet as a successful remote operation.

The user has authorized this dedicated Notion delegation for this project.
It does not authorize other parallel implementation agents, arbitrary workspace
changes, schema migrations, or messages/comments to other people.

## Ownership

The caller may maintain its own `.ai/features/<feature>/` artifacts. Only Notion
workers read/write `.ai/notion/operations/`. Never put Notion progress fields,
retry queues, receipt status, or locks in feature `state.yaml`. The caller can
retain source references and the approved requirements snapshot in its own
`requirements.md`; that is the product contract, not shared mutable state.

Do not write a worker journal from this role or tell the worker to read feature
state. Pass evidence and decisions as values in the request. Operation request
and response messages are the boundary; static configuration is read-only input.

## Outcomes and recovery

- `completed`: consume the payload and confirmed changes; gates still apply.
- `blocked`: report the missing input/auth/mapping and continue independent
  authorized work. Do not silently perform the Notion operation in the caller.
- `conflict`: show source or target changes; request a revised contract only
  when behavior/scope/authority changes. Preserve existing authorization.
- `partial` / `failed`: report exactly which writes were confirmed. Retry the
  same operation ID through a worker; do not replay successful page creation.

When subagents are unavailable, return `delegation_unavailable`; preserve the
requested isolation instead of falling back to main-agent MCP calls. Skills
run when invoked/dispatched; they do not create background polling or timers.
