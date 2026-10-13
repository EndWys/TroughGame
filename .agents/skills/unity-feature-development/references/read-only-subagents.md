# Read-Only Inspection Subagents

## Purpose and Scope

Optional research/code-review delegation only when the developer or applicable
instructions authorize it. Short searches and dependent reasoning stay with the
main agent. Without authorization/subagents, perform a separate read-only review
yourself while preserving gates/results.
Notion uses its separately authorized `docs/notion/operation-contract.md`: workers
may publish scoped remote changes and own journals, but not feature/bug state,
Git, or Unity. Code-review restrictions do not revoke authorized Notion publication.

## Tasks and Boundaries

| Stage | Suitable Independent Inspection |
| --- | --- |
| Research | Distinct gameplay/UI/network/config concerns |
| Architecture | Separate dependency or lifecycle chain |
| Audit | Distinct material risks; one reviewer may inspect the whole diff |
| Diagnosis | Different causes in existing data/logs/code |

Do not divide by file count. Do not delegate approvals, product/design decisions,
feature transitions, Git, implementation, Unity mutations, tests, or developer
communication. A finding requires verification; it is not stage acceptance.

## Request and Result

Provide feature/stage, approved context, exact question, areas/diff snapshot/logs,
and exclusions. Ask for actual paths/types/members/objects, evidence, uncertainty,
and integration impact. Prohibit writes to files/state, Git, Editor mutations,
test runs, and decisions for the developer. Keep tasks few and non-overlapping;
unfinished agent output is not a fact/API.
A shared checkout may expose equal permissions: read-only is an instruction,
not a sandbox. Use enforced permissions if available; otherwise compare
`git status --short` before/after and investigate unexpected edits.

## Synthesis and Completion

Await material results, verify claims in sources, resolve contradictions/gaps.
Only the main agent records research/architecture/audit/diagnosis/state and asks
the developer. Delegate audit after a stable diff and applicable checks; after
correction assess whether affected reviews need repeating. The whole relevant
diff remains covered. Do not retain separate agent reports by default; retain
consequential facts. Delegation creates no FEAT or new approval gate.
