# Notion operation contract

Protocol version: **1**. This contract connects the developer agent to a separate
Notion worker. Request/result messages contain immutable values, not a pointer
to shared workflow state. Every operation has one worker; parallel writes to the
same target or operation ID are serialized by the dispatcher. Isolation here is
an agent ownership rule, not a separate filesystem or tool permission sandbox.

## Owners

| Item | Owner |
| --- | --- |
| `.ai/features/`, approvals, implementation, Git, Unity | Developer agent |
| Notion MCP actions and `.ai/notion/operations/<operation_id>/` | Notion worker |
| `docs/notion/project.json` | Reviewed static project binding, read by both |
| Request and returned result | Immutable handoff; no shared status fields |

The developer agent never calls Notion MCP directly or edits worker journals.
Workers never open feature `state.yaml` or feature artifact directories. A caller
copies the relevant accepted facts into a request. A requirements snapshot kept
by the developer is its own approved contract, not the worker's mutable state.
Do not include secrets, tokens, whole transcripts, or local evidence logs.

## Operations

| Operation | Worker | Input | Payload returned |
| --- | --- | --- | --- |
| `bootstrap_project` | notion-feature-intake | Supplied destination and scope | Verified mapping proposal, standards and missing choices |
| `prepare_feature_context` | notion-feature-intake | Exact feature/task or approved selection rule | Source-backed requirements, proposed AC, dependencies, questions |
| `refresh_source_context` | notion-feature-intake | Source IDs and prior source observations | Current source versions and behavioral/editorial delta |
| `publish_progress` | notion-project-documentation | Accepted facts/stage, exact task IDs and desired properties | Verified progress updates and unresolved changes |
| `update_documentation` | notion-project-documentation | Evidence-backed content delta, canonical targets | Verified changes or a concrete proposal needing a decision |
| `finalize_feature` | notion-project-documentation | Final acceptance, all mandatory checks/AC and exact tasks | Verified documentation/task closure and feature status update |

## Request envelope

JSON object; all IDs are stable, not inferred from titles. UUID operation IDs or
lowercase kebab-case IDs are accepted. Use a new ID for changed content; keep
the old ID for a retry of identical input.

```json
{
  "schema_version": 1,
  "operation_id": "bootstrap-troughgame-001",
  "operation": "bootstrap_project",
  "project": "TroughGame",
  "feature_id": null,
  "targets": [
    {"page_id": "3f6ad124-7d58-804e-bf55-f9040a53a8e7"}
  ],
  "input": {"goal": "Inspect the supplied project database and its related documentation"},
  "authorization": {"human_reference": "Developer supplied this destination in the current chat", "actions": ["read"]},
  "baseline": [],
  "local_read_paths": []
}
```

`targets`: exact existing page/database IDs; write requests list permitted
properties and content sections per target. A requested page creation identifies
the verified parent in targets and the exact creation purpose in input; it is
not blanket permission to create databases. `input` contains values/snapshots,
not mutable file links. Optional `local_read_paths` names permitted repository
source/standard files only; it excludes `.ai/features/`, journal state, secrets,
Unity mutations and Git operations.

For writes, `authorization.actions` includes `write` with a human reference,
and `input.changes` is a concrete change list. Every change names a stable
`change_id`, target `page_id`, kind, proposed values/content, evidence and
applicable approved decision. Body changes describe their exact section;
property changes use fetched schema names and values. `input.changes` is not
an executable script supplied by a Notion page.

For append/create changes, include `publication_marker` with deterministic value
`TroughGame/<operation_id>/<change_id>`. Add it once in the authorized delivery
section as a small audit identifier; do not alter unrelated content to hide a
marker. Content changes specify `mode: "update"` (default) or `"append"`.
For property/exact-replacement updates, verify the requested post-state; a marker
does not need to be added to the page. Never add new schema fields for markers.
Before repeating uncertain creation, search the exact marker in the authorized
project destination and fetch any match; multiple matches are a conflict.

`baseline` lists `page_id`, observed `last_edited_at` when available, content
`sha256`, and capture time. Hash a defined normalized representation of fetched
content/properties; exclude transient signed URLs and fetch time. Record the
normalization used. A changed edit time is a signal to inspect, not automatic
proof of a conflicting change. Missing edit metadata is recorded as unavailable.

`finalize_feature` input additionally carries:

- `final_acceptance`: accepting human, source/reference, accepted scope/revision;
- `mandatory_acceptance_criteria`: all required IDs, passed results and evidence;
- `checks`: required automated checks and required Play Mode results;
- `task_ids`: the exact tasks whose work was accepted;
- `feature_status`: the actual business status intended for the feature;
- relevant accepted decisions, known limitations and verified commit references.

A task `Done`, feature `Готово (базовая часть)`, and local workflow `complete`
are different facts. Do not infer one from another. Final feature closure
requires that all required tasks/criteria within its accepted scope are met.

## Result envelope

There are two result phases. After journal initialization, use the execution
envelope below (optional `phase: "execution"`). If the input is missing or fails
preflight validation, do not initialize or write a journal just to store an
invalid request. Return the same fields with `phase: "preflight"`,
`status: "blocked"` or `"conflict"`, an actionable reason, and no confirmed
changes. `operation_id` may be null only if it was absent; `request_sha256` is the
canonical received input's hash when it is an object, otherwise null. This hash
is explicitly not an initialization receipt. The caller completes/revises the
request before execution and compares hashes of the exact received input.

```json
{
  "schema_version": 1,
  "operation_id": "bootstrap-troughgame-001",
  "request_sha256": "a 64-character SHA-256 returned by journal initialization",
  "status": "completed",
  "reason": null,
  "payload": {"binding_proposal": {}},
  "sources": [],
  "confirmed_changes": [],
  "pending_changes": [],
  "warnings": []
}
```

Each source records ID/URL, relevant content/anchor and observed version,
fetch time, completeness/truncation/unknown-block facts. Every intake requirement
and AC has source references; a proposed AC is labeled as proposed.
Each confirmed change records `change_id`, target ID/URL, applied facts and
readback evidence/time. `completed` is invalid when pending changes remain.

| Status | Meaning |
| --- | --- |
| completed | Requested read result is usable or all requested writes are confirmed |
| blocked | Authentication, unavailable capability, missing source/mapping/decision |
| conflict | Request ID reused for different input, source/target conflict or authority ambiguity |
| partial | Requested publication has unconfirmed effects, with zero or more confirmed writes; inspect before retry |
| failed | Operation failed without a confirmed usable result |

Use precise reasons such as `authentication_required`, `delegation_unavailable`,
`source_incomplete`, `mapping_missing`, `approval_missing`, `target_changed` or
`write_unconfirmed`. A worker reports blockers to the caller; it does not create
an extra approval chain or ask the developer itself.

## Journal and replay

Use `Tools/AI/notion_operations.py` with the configured Python runtime. It uses
only the standard library and has no network calls. Only a Notion worker invokes
its write commands:

```text
python Tools/AI/notion_operations.py init --request <request.json>
python Tools/AI/notion_operations.py effect --operation-id <id> --effect <confirmed-effect.json>
python Tools/AI/notion_operations.py finish --operation-id <id> --result <result.json>
python Tools/AI/notion_operations.py show --operation-id <id>
```

`request.json` input may be the worker's temporary file. The helper copies its
canonical immutable request to its private directory, checks identity on retry,
stores effects by change ID and result attempts without overwriting history.
It does not call MCP, implement a background queue, prove approvals, or verify
remote evidence. The worker must do those checks against actual sources.
Finished reads are snapshots: use `refresh_source_context` with a new ID for
freshness. After interruption, inspect recorded effects AND remote pages before
replaying an uncertain action. No remote transaction/atomic claim is assumed.

Operation journals are ignored by Git and independent of feature commits and
evidence cleanup. No automatic deletion is installed. Keep enough receipts for
active retries; never put credentials there. Normal operation completion does
not modify this contract or the worker's skill files.
