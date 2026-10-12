---
name: notion-feature-intake
description: Read TroughGame Notion stage plans, features, tasks, and decisions and return a source-backed requirements handoff. Run as the dedicated Notion subagent for bootstrap, intake, or source refresh; do not develop code or approve requirements.
---

# Notion Feature Intake

Execute one read-only Notion operation as a separate subagent. If invoked by
the main agent, dispatch through `notion-project-coordination` first.

Read [the contract](../../../docs/notion/operation-contract.md),
[project binding](../../../docs/notion/project.json), and
[connector procedure](../../../docs/notion/mcp-procedure.md).
For `bootstrap_project`, read [binding discovery](references/project-binding.md).

## Isolation

Do not read or edit `.ai/features/`, feature `state.yaml`, the caller's Git
index, runtime code, Unity assets, or another operation's mutable journal.
Read repository standards only when the request includes those allowed paths.
Write only this operation's `.ai/notion/operations/<operation_id>/` journal.
Return the result to the caller; do not send messages/comments to people or
change remote pages, statuses, schema, or project policy.

## Prepare or refresh context

1. Validate the request and journal its immutable inputs using
   `Tools/AI/notion_operations.py`. An existing operation ID with different
   input is a conflict. For a completed operation, return its stored result;
   source freshness requires a new refresh operation, not a silent rewrite.
   Invalid/incomplete input returns a preflight result without a journal or
   remote calls. Use the bundled Python runtime when Python is absent from PATH.
   Wait for successful initialization before ANY live connector reads; do not
   run initialization and remote fetches in parallel. If initialization fails,
   resolve the runtime/input error first or return the preflight blocker.
2. Discover the live connector tools and check access. On authentication or
   unavailable capability, return a blocked result with the exact error; do
   not repeatedly retry an unchanged connection failure.
3. Search content with the Notion search tool; fetch exact supplied IDs and
   related pages. Verify project membership, source completeness, relations,
   stage/feature/task ownership, and status/property mappings. Search snippets
   alone do not establish requirements or approvals.
4. If a feature is named, use it. For an explicit "next ready task" request,
   use the verified selection rule, active stage, priorities, dependencies,
   current status, and full candidate set. If selection is ambiguous or the
   query is incomplete, return candidates/questions instead of guessing.
5. Read the feature's specification, related tasks, decisions and relevant
   existing docs. Return goal, source-backed `RQ-*`, proposed measurable `AC-*`,
   explicit exclusions, dependencies, confirmed decisions, and blocking
   questions. Label source facts, proposed criteria, and uncertainty separately.
   Do not introduce architecture, classes, gameplay rules or requirements that
   are absent from the sources. Status `Ready` is not human contract approval.
6. Include page IDs/URLs, selected task IDs, version observations (edit time
   when exposed and normalized content hashes), source excerpts/anchors, and
   completeness metadata. Compare a refresh with the supplied baseline and
   separate behavior/scope changes from editorial changes. Do not change the
   caller's approved contract yourself.
7. Record and return the result envelope. Missing material pages, unresolved
   authority conflicts, or truncated required content cannot be reported as a
   usable completed contract. Return the partial facts with `blocked` or
   `conflict` and an actionable reason.

## Bootstrap result

Return verified IDs, schema/status mappings, relevant standards, selection-rule
options, write-scope candidates, and uncertainties as a binding proposal.
Only the caller applies an agreed proposal to static `docs/notion/project.json`.
You do not own that configuration as mutable runtime state. Existing Notion
skills may be reused when actually available and compatible with this boundary;
never assume an uninstalled skill or copy an unrelated workflow into the task.
