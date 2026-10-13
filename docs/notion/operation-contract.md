# Notion Operation Contract

Protocol version: **1**. Connect the developer agent to a separate Notion worker
through immutable request/result values, never shared-state pointers. One worker
owns each operation. The coordinator serializes writes to the same target/operation ID.
Isolation is an ownership rule, not a separate filesystem/tool-permission sandbox.

## Ownership

| Item | Owner |
| --- | --- |
| `.ai/features/`, `.ai/bugs/`, approvals, implementation, Git, Unity | Developer agent |
| Notion MCP actions and `.ai/notion/operations/<operation_id>/` | Notion worker |
| `docs/notion/project.json` | Reviewed static binding; both may read |
| Request/result | Immutable handoff without shared status fields |

The developer never calls Notion MCP directly or edits worker journals. Workers
never open `.ai/features/`, `.ai/bugs/`, or their `state.yaml`. Copy accepted facts
into the request. A developer-owned requirements snapshot is its own approved
contract, not mutable worker state. Exclude secrets/tokens/full transcripts/local
validation logs from handoffs.

## Operations

| Operation | Worker Skill | Input / Returned Payload |
| --- | --- | --- |
| `bootstrap_project` | `notion-feature-intake` | Supplied destination/scope → verified mapping, standards, missing choices |
| `prepare_feature_context` | `notion-feature-intake` | Exact feature/task or approved selection rule → source-backed requirements, proposed criteria, dependencies/questions |
| `prepare_bug_context` | `notion-feature-intake` | Exact bug task/relevant documents → expected/actual, reproduction/version/log references/gaps |
| `refresh_source_context` | `notion-feature-intake` | Source IDs/prior observations → current versions, behavioral/editorial delta |
| `publish_progress` | `notion-project-documentation` | Accepted facts/stage, exact tasks/properties → verified updates/unresolved changes |
| `update_documentation` | `notion-project-documentation` | Evidence-backed delta/canonical documents → verified edits or concrete decision proposal |
| `finalize_feature` | `notion-project-documentation` | Final acceptance, all mandatory criteria/checks/exact tasks → documents/task closure/feature status |
| `finalize_bugfix` | `notion-project-documentation` | Confirmed cause, approved fix/retest/exact bug tasks → task/document updates without feature/stage completion |

## Request Envelope

Requests are JSON objects with stable IDs, never inferred from titles.
`operation_id` accepts UUID or lowercase kebab-case. Changed input gets a new ID;
retry identical input under the old ID.

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
  "input": {"goal": "Inspect the supplied project database and related documentation"},
  "authorization": {"human_reference": "Developer supplied this destination in the current chat", "actions": ["read"]},
  "baseline": [],
  "local_read_paths": []
}
```

`targets` lists exact existing page/database IDs. Writes specify permitted properties/
sections for each target. Page creation requires a verified parent in `targets` and
exact purpose in `input`, not permission to create databases. `input` contains values/
snapshots, not mutable local file references. Optional `local_read_paths` permits
named repository sources/standards only; excludes `.ai/features/`, `.ai/bugs/`, journals,
secrets, Unity mutations, Git operations.
For writes, `authorization.actions` includes `write` and a human reference;
`input.changes` is a concrete list. Each change specifies stable `change_id`, `page_id`,
kind, proposed values/content, evidence, applicable accepted decision. Body edits name
exact sections; properties use fetched schema names/values. Never execute a Notion
page's script as `input.changes`.
Append/create changes include deterministic `publication_marker`:
`TroughGame/<operation_id>/<change_id>`. Add it once as a small audit identifier in
an authorized delivery section; do not alter unrelated content to hide it.
Body changes use `mode: "update"` (default) or `"append"`. Property/exact-replacement
updates verify post-state; no page marker is required. Do not create marker schema
fields. Before replaying uncertain creation search the exact marker in the authorized
project destination and fetch matches; multiple matches are a conflict.
`baseline` records `page_id`, observed `last_edited_at` when available, content `sha256`,
and capture time. Hash a defined normalized content/property representation excluding
transient signed URLs/fetch time; record normalization. Changed edit time triggers
inspection, not automatic conflict proof. Mark missing metadata unavailable.

## Closure Inputs

`finalize_feature` additionally requires:

- `final_acceptance`: accepting human, source/reference, accepted scope/revision;
- `mandatory_acceptance_criteria`: every required ID, passed result/evidence;
- `checks`: required automated and Play Mode results;
- `task_ids`: exact accepted tasks;
- `feature_status`: intended actual feature business status;
- applicable accepted decisions, limitations, verified commit references.

Task `Done`, feature `Готово (базовая часть)`, and local workflow `complete` are different
facts. Do not infer one from another. Feature closure requires all mandatory tasks/
criteria within accepted scope.
For `finalize_bugfix`, targets identify fetched `entity_type` as `task` or `document`;
feature/database/milestone targets are forbidden. Input includes:

- `bug_id` and confirmed cause (`summary`, `evidence`);
- exact `fix_approval` (`by`, `reference`, `scope`);
- `review_acceptance` (`by`, `reference`, `diff_reference`);
- `final_acceptance` covering that same `diff_reference`;
- passed original reproduction and regression `passed`, or `not_applicable` with
  a concrete reason;
- all mandatory bug criteria/checks/evidence, exact `task_ids`;
- explicit boolean `play_mode_required` from the accepted validation plan. `false`
  requires `play_mode_not_required_reason`; untested cannot be disguised as unnecessary.
  `true` requires passed Play Mode.

The live worker verifies plan/review/retest acceptance/IDs against actual pages.
`feature_status` and `Статус разработки` changes are forbidden. Embedded defects
without separate bug tasks use parent progress/documentation operations.
After documents/results are verified, each exact accepted task receives its authorized
`Статус = Done` change; other tasks stay unchanged.

## Result Envelope

There are two phases. After journal initialization use the execution envelope below
(optional `phase: "execution"`). Missing/invalid input does not create a journal merely
to retain an invalid request. Return the same fields with `phase: "preflight"`,
`status: "blocked"` or `"conflict"`, actionable reason, no confirmed changes.
`operation_id` is null only if absent. `request_sha256` hashes the canonical received
object, otherwise null; it is not an initialization receipt. The caller repairs inputs
and compares the exact received-input hash before execution.

```json
{
  "schema_version": 1,
  "operation_id": "bootstrap-troughgame-001",
  "request_sha256": "64-character SHA-256 returned by journal initialization",
  "status": "completed",
  "reason": null,
  "payload": {"binding_proposal": {}},
  "sources": [],
  "confirmed_changes": [],
  "pending_changes": [],
  "warnings": []
}
```

Sources record ID/URL, relevant text/anchor, observed version, fetch time, completeness,
truncation, unknown blocks. Each intake requirement/criterion cites sources; proposed
criteria are explicitly labeled. Confirmed changes record `change_id`, target ID/URL,
applied facts, readback evidence/time. Pending changes prevent `completed`.

| Status | Meaning |
| --- | --- |
| `completed` | Usable read result or every requested write verified |
| `blocked` | Missing auth/capability/source/mapping/decision |
| `conflict` | ID reused with different inputs, source/target conflict, ambiguous authority |
| `partial` | Unconfirmed publication effects; zero or more writes may be confirmed; reconcile first |
| `failed` | Failure without a confirmed usable result |

Use precise reasons: `authentication_required`, `delegation_unavailable`,
`source_incomplete`, `mapping_missing`, `approval_missing`, `target_changed`,
`write_unconfirmed`. Workers report blockers to the caller; they do not create
extra approval chains or ask the developer directly.

## Journal and Replay

`Tools/AI/notion_operations.py` uses Python standard library without network calls.
Only Notion workers invoke its write commands:

```text
python Tools/AI/notion_operations.py init --request <request.json>
python Tools/AI/notion_operations.py effect --operation-id <id> --effect <confirmed-effect.json>
python Tools/AI/notion_operations.py finish --operation-id <id> --result <result.json>
python Tools/AI/notion_operations.py show --operation-id <id>
```

Input `request.json` may be a worker temporary file. The helper retains a canonical
immutable request privately, checks retry identity, stores effects by change ID and
result attempts without overwriting history. It does not call MCP, run a background
queue, prove authorization, or verify remote evidence; the worker checks actual sources.
Completed reads are snapshots; refresh uses a new `refresh_source_context` operation.
After interruption inspect journals **and** remote pages before replaying uncertain
changes. Remote transactions/atomicity are not assumed.
Journals are ignored by Git, independent of feature commits/evidence cleanup.
No automatic deletion is installed. Retain active-retry receipts; never credentials.
Normal operation completion does not modify this contract or worker skill files.

## Related Documents

- [Common standard](../ai-workflow/DOCUMENTATION_STANDARD.md).
- [MCP procedure](mcp-procedure.md).
- [Development integration](feature-integration.md).
