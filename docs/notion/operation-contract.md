# Notion Worker Handoff

## Purpose and Dispatch

There are exactly two feature-workflow operations. Their timing is owned by
[feature integration](feature-integration.md). This replaces the former multi-operation
coordinator, journal helper and bootstrap/progress/bugfix/refresh protocol.
The main agent explicitly reads the relevant skill at its permitted stage, then
spawns one worker with `fork_turns: "none"`, repository root and absolute skill path.
The skills disable implicit invocation; their names in incidental text are no trigger.
If subagents are unavailable report `delegation_unavailable`; never substitute main-agent MCP.

## Inputs and Results

| Skill | Immutable inputs | Result |
| --- | --- | --- |
| `notion-feature-requirements` | Selected feature and source IDs/URLs, specific questions | Source-backed goal, requirements, criteria, dependencies, decisions, task IDs, gaps |
| `notion-feature-finalization` | Whole-feature readiness acceptance (human/reference/scope), exact target IDs, desired task/feature status, short accepted specification/plan deltas | Confirmed and pending edits/statuses with remote readback |

Pass only necessary values. The developer owns local contracts, state, gates, code,
Unity and Git. Workers never read/write `.ai/features/` or `.ai/bugs/`. No full history,
local audit/test logs, state pointers or shared queues. Workers report to the caller;
they do not ask the developer or advance workflow stages.

Use `completed`, `blocked`, `conflict`, or `partial`, with sources and concrete gaps.
For finalization include per-target confirmed/pending effects and readback; an agent's
success statement alone is not evidence. The main agent serializes writes to the same
targets. Only if interrupted/partial finalization needs recovery may its worker retain
a short receipt under ignored `.ai/notion/operations/`; no mandatory journal/hash/CLI
initialization. Existing historical receipts remain untouched. Re-fetch pending targets
before retrying, skip applied changes, and stop after one unresolved reconciliation.
