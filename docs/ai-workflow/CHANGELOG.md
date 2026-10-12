# Workflow changelog

## 1.3-tg.1 — 2026-10-12

- Added dedicated Notion dispatch, intake and documentation skills under the
  developer's explicit separate-subagent requirement.
- Bound existing tasks/features/docs/ideas IDs and schema from read-only MCP
  discovery; reused the enabled Notion documentation standard v1.2.
- Added immutable request/results and independent private Notion receipts.
  Feature state gained no remote sync fields; workers cannot read/write it.
- Added source-version refresh, authorized gate publication and verified final
  documentation/task updates while preserving existing semantic approvals.
- Added an offline journal helper and tests for replay identity, scoped writes,
  partial outcomes and completion evidence. These do not claim remote tests.

New states use `1.3-tg.1`. Existing `1.2-tg.1` states are not automatically
migrated; adding the Notion publication/source contract to an active feature
requires an explicit migration decision. See [Notion usage](../notion/README.md).

## 1.2-tg.1 — 2026-10-12

Adapted from the supplied `rivaldom-unity-feature-workflow-porting-v1.2.zip`.

- Retained the v1.2 stages, explicit semantic approvals, developer Play Mode
  evidence, final traceability, audit, retrospective, and knowledge promotion.
- Developer selected staged approvals and automatic subtask commits after
  accepted review and successful Play Mode; no deployment/tooling auto-commit.
- Reused TroughGame's canonical `ARCHITECTURE_RULES.md`, `CONVENTIONS.md`, and
  scene hierarchy standard; added evidence-based overview and integration map.
- Historical DOCX has no policy authority; active Markdown is the workflow
  reference, subordinate to developer instructions and project standards.
- Adapted tool paths to `Tools/AI` and CLI override to
  `TROUGHGAME_UNITY_CLI_PATH`; preserved installed CLI/Pipeline versions.
- Hardened connected-Editor validation against rejected commands, mixed
  string/object payloads, wrong Editor targeting, and inconclusive test results.
- Scoped evidence cleanup to project evidence, with dry-run reporting and
  protection against link traversal. Documented audit as structural evidence,
  not proof of semantic approvals or completeness.
- Subagents remain optional and require authorization under the active agent
  environment; no delegation is needed to run the workflow.

This was the initial adaptation. Existing workflow versions are migrated only
by an explicit decision when gate or state semantics change.
