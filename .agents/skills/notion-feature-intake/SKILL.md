---
name: notion-feature-intake
description: Read TroughGame Notion plans, features, bug tasks and decisions and return source-backed requirements or bug context. Run as the dedicated Notion subagent for bootstrap, intake or source refresh; do not develop code or approve changes.
---

# Notion Context Intake

## Purpose and Scope

Execute one read-only operation as a dedicated worker. Main-agent invocation must
route through `notion-project-coordination`. Read the
[contract](../../../docs/notion/operation-contract.md),
[binding](../../../docs/notion/project.json),
[MCP procedure](../../../docs/notion/mcp-procedure.md),
[common standard](../../../docs/ai-workflow/DOCUMENTATION_STANDARD.md).
For `bootstrap_project` read [project binding](references/project-binding.md).

## Ownership

Do not read/write `.ai/features/`, `.ai/bugs/`, developer state, Git, runtime code,
Unity assets, or another mutable journal. Read repository standards only from
permitted input paths. Write only your `.ai/notion/operations/<operation_id>/`.
Return results to the main agent; do not mutate remote pages/statuses/schema/policy
or send messages/comments.

## Operation Steps

1. Validate the request and initialize `Tools/AI/notion_operations.py` journal.
   Same ID/different input is a conflict; completed operations return saved results.
   Fresh reads need new refresh requests. Invalid/incomplete input returns `preflight`
   without journal/MCP. Use bundled Python when absent from PATH. Await successful
   initialization before any MCP read; do not run initialization and fetch concurrently.
2. Discover current tools/check access. Auth/capability failures return exact reasons;
   do not repeatedly test an unchanged connection.
3. Every content search uses Notion search; fetch exact IDs/related pages directly.
   Verify project membership, completeness, relations, owners, and schemas. Search
   snippets do not prove requirements/approval.
4. Use the supplied feature. If asked to choose next work, verify stage, selection
   rule, priority/order, dependencies, statuses, complete candidate set. Ambiguity/
   incomplete selection returns candidates/questions. `Ready` is not contract approval;
   reading does not claim a task.
5. Read specifications/tasks/decisions/applicable documents. Return goal, supported
   `RQ-*`, proposed observable acceptance criteria (`AC-*`), exclusions/dependencies/
   accepted decisions/blocking questions. Distinguish fact/proposal/unknown. Do not
   invent missing mechanics/classes/architecture.
6. Include task/source IDs/URLs, edit times when exposed, normalized hashes, sections/
   excerpts/completeness metadata. Refresh compares the supplied baseline and separates
   behavior/scope delta from editorial changes. Do not revise the accepted developer contract.
7. Record/return the contract result. Missing material sources, authority conflicts,
   or truncated mandatory content prevent usable `completed`; return findings with
   `blocked`/`conflict` and a concrete reason.

## Special Results

`bootstrap_project` returns verified IDs/schemas/statuses, standards, selection/write
scope options, and unknowns. The main agent applies the accepted `project.json`
proposal; configuration is not worker state. Use actually available compatible
Notion skills; do not assume missing installations or copy an unrelated process.
`prepare_bug_context` reads the exact bug task and relevant sources: authoritative
expected behavior, reported actual behavior, reproduction/version/peer roles/log
references/dependencies/source versions/gaps. It is not diagnosis/fix or a new-feature
contract. Do not create bug properties/databases, close tasks, or open `.ai/bugs/`;
diagnosis belongs to the developer.
