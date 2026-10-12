# Read-only Subagents in Feature Development

## When to delegate

Delegation is optional and requires authorization from the developer or applicable higher-priority instructions in the active environment. This reference does not itself authorize spawning agents. When authorized, use it if independent inspection materially improves the current stage. Keep short searches and dependent reasoning with the main agent. Otherwise continue with a separate read-only pass by the main agent, without changing gates or deliverables.

Useful boundaries include:

| Stage | Independent read-only assignments |
| --- | --- |
| Project research | Trace separate relevant responsibilities, such as gameplay state, UI/Unity integration, networking/peer authority or configuration. |
| Local architecture | Check one proposed dependency or lifecycle chain against its real owners while the main agent prepares the design. |
| AI code audit | Independently inspect the completed, validated diff for distinct risk areas; at least one reviewer should inspect the full relevant diff when feasible. |
| Bug diagnosis | Investigate separate plausible causes in existing logs, code paths or lifecycle evidence. |

Do not split work by arbitrary file count. Do not delegate requirements approval, product or architecture decisions, feature-state transitions, Git integration, implementation, Unity mutations, test runs or developer communication. A subagent finding is evidence to assess, not approval or a completed stage.

## Handoff contract

Give each subagent:

- the current feature and stage, approved requirement or subtask context, and the precise question;
- the repository areas, diff snapshot or existing logs to inspect, with any relevant exclusions;
- a requested result containing concrete paths, types/members or Unity objects, supporting evidence, uncertainty and integration implications;
- an explicit read-only boundary: no file/state/Git writes, Editor changes, test execution or developer-facing decisions.

Use a small number of assignments with non-overlapping questions. Parallelism means the inspection runs concurrently; it does not let one agent assume another's unfinished API or findings.

Subagents in a shared checkout may have the same filesystem permissions as the main agent; the read-only handoff is a behavioral boundary, not a separate sandbox. Prefer an enforced read-only role when the active Codex surface offers one. Otherwise capture `git status --short` before delegation and inspect it again after results return; investigate any unexpected change before using the results or continuing the workflow.

## Synthesis and gate ownership

Wait for all relevant delegated work before presenting the stage result. The main agent checks material claims in source, reconciles conflicting findings and fills any uncovered concern. It alone writes the Research Brief, architecture, audit findings or bug diagnosis, updates feature state and requests approval.

For AI audit, delegate only after the implementation diff and applicable automated validation are stable. If a correction changes the diff, assess whether affected delegated findings must be repeated. The complete relevant diff still receives the audit required by `ai-code-audit.md`; splitting risk areas never reduces that coverage.

Do not persist a separate subagent report by default. Record only findings that affect the feature artifact, evidence or decision. Delegation itself creates no subtask, approval gate or workflow-state transition.
