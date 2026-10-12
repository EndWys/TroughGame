"""Private Notion-worker request/receipt journal. No MCP or feature state access."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import sys


PROJECT_ROOT = Path(__file__).resolve().parents[2]
READ_OPERATIONS = {"bootstrap_project", "prepare_feature_context", "refresh_source_context"}
WRITE_OPERATIONS = {"publish_progress", "update_documentation", "finalize_feature"}
STATUSES = {"completed", "blocked", "conflict", "partial", "failed"}
IDENTIFIER = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
PAGE_ID = re.compile(r"^(?:[0-9a-f]{32}|[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12})$", re.I)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def canonical(value):
    return json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":"))


def fingerprint(value):
    return hashlib.sha256(canonical(value).encode("utf-8")).hexdigest()


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def write_new(path, value):
    with Path(path).open("x", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(value, ensure_ascii=False, indent=2) + "\n")


def validate_request(request, root=PROJECT_ROOT):
    require(isinstance(request, dict), "Request must be an object")
    allowed = {"schema_version", "operation_id", "operation", "project", "feature_id",
               "targets", "input", "authorization", "baseline", "local_read_paths"}
    require(not set(request) - allowed, "Unknown request fields; do not pass workflow state")
    require(request.get("schema_version") == 1, "Unsupported request schema")
    require(IDENTIFIER.fullmatch(request.get("operation_id", "")), "Invalid operation_id")
    operation = request.get("operation")
    require(operation in READ_OPERATIONS | WRITE_OPERATIONS, "Unknown operation")
    require(request.get("project") == "TroughGame", "Wrong project")
    targets = request.get("targets")
    require(isinstance(targets, list) and targets, "Exact target IDs are required")
    for target in targets:
        require(isinstance(target, dict) and PAGE_ID.fullmatch(target.get("page_id", "")), "Invalid target page_id")
    require(len({t["page_id"] for t in targets}) == len(targets), "Duplicate target IDs")
    data = request.get("input")
    require(isinstance(data, dict), "input must contain immutable values")
    require(not {"feature_state", "workflow_state", "state_path"} & set(data), "Shared feature state is forbidden")
    require(isinstance(request.get("baseline", []), list), "baseline must be a list")
    paths = request.get("local_read_paths", [])
    require(isinstance(paths, list), "local_read_paths must be a list")
    for name in paths:
        path = (root / name).resolve()
        require(path.is_relative_to(root.resolve()), "Local read path escapes project")
        require(not path.is_relative_to((root / ".ai").resolve()), "Local .ai state cannot be passed as a read path")
    auth = request.get("authorization", {})
    require(isinstance(auth, dict), "authorization must be an object")
    actions = auth.get("actions", [])
    require(isinstance(actions, list) and "read" in actions, "Read authorization is required")
    require(set(actions) <= {"read", "write"}, "Action outside operation scope")
    if operation in READ_OPERATIONS:
        require("write" not in actions, "Intake operations cannot write Notion")
        return request
    require("write" in actions and bool(auth.get("human_reference")), "Human write authorization reference is required")
    changes = data.get("changes")
    require(isinstance(changes, list) and changes, "Write request needs a concrete changes list")
    require(len({c.get("change_id") for c in changes}) == len(changes), "Duplicate change IDs")
    by_id = {t["page_id"]: t for t in targets}
    for change in changes:
        require(IDENTIFIER.fullmatch(change.get("change_id", "")), "Invalid change_id")
        target = by_id.get(change.get("page_id"))
        require(target is not None, "Change target is outside authorized targets")
        require(bool(change.get("evidence")), "Every change needs evidence")
        kind = change.get("kind")
        if kind == "properties":
            properties = change.get("properties")
            require(isinstance(properties, dict) and properties, "Property values are required")
            require(set(properties) <= set(target.get("allowed_properties", [])), "Property outside target scope")
        elif kind == "content":
            require(change.get("section") in target.get("allowed_sections", []), "Content section outside target scope")
            require(bool(change.get("content")), "Content delta is required")
            require(change.get("mode", "update") in {"update", "append"}, "Unknown content mode")
        elif kind == "create_page":
            require(target.get("allow_create") is True and bool(change.get("content")), "Explicit parent creation scope is required")
        else:
            raise ValueError("Unsupported change kind")
        if kind == "create_page" or (kind == "content" and change.get("mode") == "append"):
            marker = f'TroughGame/{request["operation_id"]}/{change["change_id"]}'
            require(change.get("publication_marker") == marker and marker in change.get("content", ""), "Append/create needs its deterministic publication marker in authorized content")
    if operation == "finalize_feature":
        acceptance = data.get("final_acceptance", {})
        require(all(acceptance.get(k) for k in ("by", "reference", "scope")), "Explicit final acceptance is required")
        criteria = data.get("mandatory_acceptance_criteria")
        checks = data.get("checks")
        require(isinstance(criteria, list) and criteria, "Mandatory AC results are required")
        require(isinstance(checks, list) and checks, "Required check results are required")
        require(all(c.get("status") == "passed" and c.get("evidence") for c in criteria + checks), "Mandatory checks/AC must pass with evidence")
        require(any(c.get("kind") == "play_mode" for c in checks), "Required Play Mode evidence is missing")
        require(data.get("task_ids") and data.get("feature_status"), "Exact accepted task IDs and feature business status are required")
    return request


def operation_directory(operation_id, root=PROJECT_ROOT):
    require(IDENTIFIER.fullmatch(operation_id), "Invalid operation_id")
    directory = root.resolve() / ".ai/notion/operations" / operation_id
    require(directory.resolve() == directory, "Journal path contains a symbolic link or junction")
    return directory


def initialize(request, root=PROJECT_ROOT):
    validate_request(request, root)
    directory = operation_directory(request["operation_id"], root)
    directory.mkdir(parents=True, exist_ok=True)
    path = directory / "request.json"
    if path.exists():
        require(fingerprint(read_json(path)) == fingerprint(request), "Operation ID already belongs to different input")
    else:
        write_new(path, request)
    return {"operation_id": request["operation_id"], "request_sha256": fingerprint(request),
            "directory": str(directory), "result": latest_result(directory)}


def latest_result(directory):
    paths = sorted((directory / "results").glob("*.json"))
    return read_json(paths[-1]) if paths else None


def validate_effect(request, effect):
    change_id = effect.get("change_id", "")
    require(IDENTIFIER.fullmatch(change_id), "Invalid effect change_id")
    changes = {c["change_id"]: c for c in request["input"].get("changes", [])}
    require(change_id in changes and effect.get("page_id") == changes[change_id]["page_id"], "Effect is outside requested changes")
    require(effect.get("verified_at") and effect.get("verification"), "Effect needs actual remote readback evidence")


def record_effect(operation_id, effect, root=PROJECT_ROOT):
    directory = operation_directory(operation_id, root)
    request = read_json(directory / "request.json")
    validate_effect(request, effect)
    effects = directory / "effects"
    effects.mkdir(exist_ok=True)
    path = effects / (effect["change_id"] + ".json")
    if path.exists():
        require(read_json(path) == effect, "Existing confirmed effect cannot be overwritten")
    else:
        write_new(path, effect)
    return effect


def finish(operation_id, result, root=PROJECT_ROOT):
    directory = operation_directory(operation_id, root)
    request = read_json(directory / "request.json")
    require(result.get("schema_version") == 1 and result.get("operation_id") == operation_id, "Result identity mismatch")
    require(result.get("request_sha256") == fingerprint(request), "Result request fingerprint mismatch")
    require(result.get("status") in STATUSES, "Unknown result status")
    require(isinstance(result.get("payload"), dict), "Result payload is required")
    for key in ("sources", "confirmed_changes", "pending_changes", "warnings"):
        require(isinstance(result.get(key), list), f"{key} must be a list")
    if result["status"] != "completed":
        require(bool(result.get("reason")), "Unsuccessful result needs an actionable reason")
    else:
        require(not result["pending_changes"], "Completed result has pending changes")
    confirmed = result["confirmed_changes"]
    require(len({c.get("change_id") for c in confirmed}) == len(confirmed), "Duplicate confirmed changes")
    for effect in confirmed:
        validate_effect(request, effect)
        require(read_json(directory / "effects" / (effect["change_id"] + ".json")) == effect, "Confirmed effect was not journaled")
    if result["status"] == "completed" and request["operation"] in WRITE_OPERATIONS:
        require({c["change_id"] for c in confirmed} == {c["change_id"] for c in request["input"]["changes"]}, "Write completion omits requested effects")
    prior = latest_result(directory)
    if prior and prior["status"] == "completed":
        require(prior == result, "Completed receipt is immutable; use a new operation for changed input")
        return prior
    results = directory / "results"
    results.mkdir(exist_ok=True)
    attempt = len(list(results.glob("*.json"))) + 1
    write_new(results / f"{attempt:06d}.json", result)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("init").add_argument("--request", required=True)
    for name, file_flag in (("effect", "--effect"), ("finish", "--result"), ("show", None)):
        command = sub.add_parser(name)
        command.add_argument("--operation-id", required=True)
        if file_flag:
            command.add_argument(file_flag, required=True)
    args = parser.parse_args()
    try:
        if args.command == "init":
            result = initialize(read_json(args.request))
        elif args.command == "effect":
            result = record_effect(args.operation_id, read_json(args.effect))
        elif args.command == "finish":
            result = finish(args.operation_id, read_json(args.result))
        else:
            directory = operation_directory(args.operation_id)
            result = {"request": read_json(directory / "request.json"), "result": latest_result(directory),
                      "effects": [read_json(p) for p in sorted((directory / "effects").glob("*.json"))]}
        print(json.dumps(result, ensure_ascii=False, indent=2))
        return 0
    except (ValueError, OSError, KeyError, TypeError) as error:
        print(json.dumps({"status": "failed", "reason": str(error)}, ensure_ascii=False))
        return 1


if __name__ == "__main__":
    sys.exit(main())
