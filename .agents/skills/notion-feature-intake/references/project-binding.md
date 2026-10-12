# Project binding discovery

`docs/notion/project.json` is static project configuration. Bootstrap returns a
proposal, not a remote mutation or permission to choose a different workspace.

Start with the human-supplied destination ID. `app.notion.com` URLs may require
using their raw page UUID with fetch; a `v=` parameter is an unverified view ID,
not the database/data-source ID. Fetch discovers the entity type. For a database,
fetch its returned `collection://` sources for exact schemas and relations.

Verify stages, features, tasks, decisions and documentation destinations by
their real relationships. They may be views of one database, separate databases
or pages: do not require a new database hierarchy. Map existing properties and
statuses instead of creating assumed `Status`, `Stage`, or `Feature` properties.

Return for each mapping: exact ID, URL, title/type, schema property name/type,
relation direction, status values, evidence of project membership, and source
completeness. Include nested pages/content referenced by the selected task.
Missing roles remain null with an explanation; they are not a request to create
pages automatically. Binding is usable for an operation only when that
operation's required roles/mappings are verified.

Propose selection criteria from the actual plan: stage, eligibility, priority,
dependencies and a tie-breaker. If two candidates remain equally valid, ask the
caller to select or approve a stable rule. Do not claim a task merely by reading
it; a claim/status change is a separate authorized publication operation.

For writes, identify allowed existing pages/properties/sections, configured
creation parents (if any), and permitted lifecycle transitions. Preserve human
authored fields. Broad workspace traversal, deleting/archive operations, schema
changes, due dates, reassignment and notifications need explicit task scope.

Current repository standards retain authority until a specific document's
Notion canonical owner and repository mirror are agreed. Do not establish that
priority by interpreting a page title or a newer modification date.
