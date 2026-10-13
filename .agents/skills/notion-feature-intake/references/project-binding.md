# Notion Project Binding

## Purpose

`bootstrap_project` prepares a proposal for static `docs/notion/project.json`.
It neither mutates remote content nor permits choosing another workspace.

## Discovery and Evidence

Start at the developer's supplied destination. `app.notion.com` may require the
raw UUID for fetching; `v=` likely identifies a view, not database/source.
Fetch establishes entity type. For databases read returned `collection://` sources
and actual schemas/relations.
Verify stages/features/tasks/decisions/documents through project relations. They
may be pages, separate databases, or views of one database; no new hierarchy is
required. Map actual properties/statuses. Return ID/URL/title/type, exact property
names/types, relation direction, options, project membership, completeness.
Read selected task material. Missing roles remain `null` with an explanation,
not automatic permission to create pages. Readiness depends on the operation's
required roles/fields.

## Selection and Write Scope

Selection uses stage, eligibility, order/priority, dependencies, and tie-breaker.
Return ambiguity to the main agent. Task claiming/status changes are separately
authorized operations, not side effects of reading.
For publication define exact pages/properties/sections, authorized creation parents,
and state transitions. Preserve human fields. Arbitrary traversal, schema changes/
delete/archive/move, dates/assignees, notifications require specific separate requests.

## Source Authority

Repository standards remain authoritative until an explicit decision selects a
specific Notion canonical owner/local mirror. Page titles/newer edit times alone
do not change precedence.
