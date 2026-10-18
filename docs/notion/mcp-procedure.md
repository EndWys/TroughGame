# Notion MCP Procedure

## Purpose

Define read and verified-write procedures for separate Notion workers. Discover
actual tools from the current connection; do not copy an API manual or assume another
installation's capabilities. Use Notion MCP exclusively, not browser automation,
web search or a separate HTTP client. Apply these rules only within the
[two permitted feature operations](feature-integration.md).

## Reading and Preparation

Use Notion search for **every content search**. Fetch content/schema by exact supplied/
resolved IDs. Prefer relation IDs/saved views for bounded selection; check pagination/
completeness. Fetch `collection://` schemas; distinguish database/source/view IDs.
Task `Статус` was observed as `select`, not `status`. Actual schema controls query
fields, not assumed SQL names. Do not expand to unrelated projects/services for missing
requirements.
Before content edits read `notion://docs/enhanced-markdown-spec` through Notion fetch/
resource reader, never web tools. Use native mentions for pages/databases/people/dates;
preserve icons, exact child tags, existing style. `<page>`/`<database>` tags can move
entities; mentions reference them. Table cells contain rich text; indentation affects
nested blocks.

## Writing and Verification

Fetch targets before edits. Prefer smallest unique `update_content` replacements;
`insert_content` only prepends/appends. Preserve unrelated content. Re-fetch after
structural/multipart edits and verify requested properties. Report warnings/truncation/
unknown blocks. Do not bypass child-deletion protection; routine publication never
removes/archives/moves entities.
For asynchronous writes retain operation/task ID and await final outcome before
readback. Accepted request is not completed publication. On timeout/ambiguous failure
fetch the target and compare the requested delta with current content/properties.
Reconcile once, then return `partial`/`blocked` if uncertainty remains. Skip already
applied edits. These skills never create pages/tasks or append audit markers.

## Limits and Recovery

Auth failure stops live operations until the connection changes. Distinguish auth,
page permission, unsupported capability, quota, missing content, schema mismatch.
When advanced analysis is unavailable, use bounded row/view queries only if they
preserve scope/completeness; otherwise report the limit. If MCP requires its access/
upgrade card, follow returned tool instructions and include its recovery link in
caller handoff.

## Related Documents

- [Operation contract](operation-contract.md).
- [Notion documentation standard](documentation-standard.md).
