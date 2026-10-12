# Live MCP procedure

Discover tools from the active Notion MCP connection; do not copy an API manual
or assume capabilities from another installation. Current read-only preflight
on 2026-10-12 verified search, fetch, tool access inspection, row/view queries,
property/content editing and page creation as advertised. A tool being available
does not grant access to every target or authorize all of its operations.

Use Notion search for EVERY content search. Fetch exact supplied/resolved IDs
for content and schema. Prefer existing relation IDs or saved views for bounded
selection; confirm pagination/completeness. Fetch `collection://` sources for
schema and do not confuse database, source and view IDs. Current task `Статус`
is a select, not a Notion status property. Avoid assumed SQL column names;
the actual exposed schema owns query fields. Do not broaden a search to other
connected services or projects to fill missing project requirements.

Before content writes read `notion://docs/enhanced-markdown-spec` using Notion
fetch or its resource reader. Do not fetch that URI through web tools. Use
native Notion mentions for internal page/database/person/date references,
preserve icons and exact child/entity tags, and match existing page style.
Referenced child `<page>`/`<database>` tags may move entities; mentions are
references. Table cells are rich text; indentation affects nested blocks.

Fetch targets before changing them. Prefer smallest unique `update_content`
replacements; `insert_content` only prepends/appends. Preserve unrelated content.
Refetch after structural/multipart edits, and verify requested properties after
updates. Surface warnings, truncation and unknown blocks. Never bypass child
deletion protection or delete/archive/move entities under routine publication.

For an asynchronous write, retain its operation/task ID and wait for its final
outcome before readback. An accepted request is not a completed remote write.
On timeout or ambiguous failure, fetch the target and search the exact stable
publication marker if creation outcome is unknown. Reconcile once, then return
partial/blocked if uncertainty persists. Do not retry blindly or create duplicates.

Authentication failure stops further live operations until the connection
changes. Distinguish auth, page permission, unsupported capability, quota,
missing content, and schema mismatch in the result. If advanced analysis is
unavailable, use available bounded rows/view queries only when they preserve
the same scope and completeness; otherwise report the limitation. If the MCP
requires its access/upgrade card, follow the tool's returned instructions and
include its recovery link in the caller handoff.
