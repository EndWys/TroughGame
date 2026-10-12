"""Offline tests of isolated journal semantics; does not call Notion or Unity."""

import copy
from pathlib import Path
import tempfile
import unittest

import notion_operations as journal


PAGE = "3f6ad124-7d58-804e-bf55-f9040a53a8e7"


class OperationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="trough-notion-test-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.request = {"schema_version": 1, "operation_id": "intake-001",
                        "operation": "prepare_feature_context", "project": "TroughGame",
                        "targets": [{"page_id": PAGE}], "input": {},
                        "authorization": {"actions": ["read"]}, "baseline": [], "local_read_paths": []}

    def write_request(self):
        self.request.update(operation="publish_progress", operation_id="progress-001")
        self.request["authorization"] = {"actions": ["read", "write"], "human_reference": "Human accepted review"}
        self.request["targets"][0]["allowed_properties"] = ["Статус"]
        self.request["input"] = {"changes": [{"change_id": "status-001", "page_id": PAGE,
                                             "kind": "properties", "properties": {"Статус": "Review"},
                                             "evidence": ["Accepted review, revision 1"]}]}

    def result(self, status="completed"):
        return {"schema_version": 1, "operation_id": self.request["operation_id"],
                "request_sha256": journal.fingerprint(self.request), "status": status,
                "reason": None if status == "completed" else "write_unconfirmed", "payload": {},
                "sources": [], "confirmed_changes": [], "pending_changes": [], "warnings": []}

    def test_replay_preserves_request_and_result(self):
        first = journal.initialize(self.request, self.root)
        result = self.result()
        journal.finish("intake-001", result, self.root)
        second = journal.initialize(self.request, self.root)
        self.assertEqual(first["request_sha256"], second["request_sha256"])
        self.assertEqual(result, second["result"])
        self.assertFalse((self.root / ".ai/features").exists())

    def test_same_id_with_changed_input_is_conflict(self):
        journal.initialize(self.request, self.root)
        self.request["input"] = {"changed": True}
        with self.assertRaisesRegex(ValueError, "different input"):
            journal.initialize(self.request, self.root)

    def test_state_path_and_traversal_are_rejected(self):
        for path in (".ai/features/player/state.yaml", "../outside", ".ai/notion/operations/other"):
            with self.subTest(path=path):
                self.request["local_read_paths"] = [path]
                with self.assertRaises(ValueError):
                    journal.validate_request(self.request, self.root)
        self.request["local_read_paths"] = []
        self.request["operation_id"] = "../escape"
        with self.assertRaises(ValueError):
            journal.initialize(self.request, self.root)

    def test_intake_cannot_request_remote_write(self):
        self.request["authorization"]["actions"].append("write")
        with self.assertRaisesRegex(ValueError, "cannot write"):
            journal.validate_request(self.request, self.root)

    def test_write_scope_and_human_reference_are_required(self):
        self.write_request()
        self.request["authorization"]["human_reference"] = None
        with self.assertRaises(ValueError):
            journal.validate_request(self.request, self.root)
        self.request["authorization"]["human_reference"] = "Accepted"
        self.request["input"]["changes"][0]["properties"] = {"InventedStatus": "Done"}
        with self.assertRaisesRegex(ValueError, "outside target scope"):
            journal.validate_request(self.request, self.root)

    def test_completion_requires_all_confirmed_effects(self):
        self.write_request()
        journal.initialize(self.request, self.root)
        with self.assertRaisesRegex(ValueError, "omits requested effects"):
            journal.finish("progress-001", self.result(), self.root)
        effect = {"change_id": "status-001", "page_id": PAGE, "verified_at": "2026-10-12T00:00:00Z",
                  "verification": {"Статус": "Review"}}
        journal.record_effect("progress-001", effect, self.root)
        result = self.result()
        result["confirmed_changes"] = [effect]
        self.assertEqual(journal.finish("progress-001", result, self.root), result)

    def test_partial_history_and_completed_immutability(self):
        journal.initialize(self.request, self.root)
        journal.finish("intake-001", self.result("blocked"), self.root)
        final = self.result()
        journal.finish("intake-001", final, self.root)
        changed = copy.deepcopy(final)
        changed["payload"] = {"different": True}
        with self.assertRaisesRegex(ValueError, "immutable"):
            journal.finish("intake-001", changed, self.root)
        self.assertEqual(len(list((self.root / ".ai/notion/operations/intake-001/results").glob("*.json"))), 2)

    def test_pending_changes_and_fingerprint_mismatch_cannot_complete(self):
        journal.initialize(self.request, self.root)
        result = self.result()
        result["pending_changes"] = ["missing"]
        with self.assertRaisesRegex(ValueError, "pending changes"):
            journal.finish("intake-001", result, self.root)
        result["pending_changes"] = []
        result["request_sha256"] = "0" * 64
        with self.assertRaisesRegex(ValueError, "fingerprint"):
            journal.finish("intake-001", result, self.root)

    def test_closure_cannot_bypass_acceptance(self):
        self.write_request()
        self.request["operation"] = "finalize_feature"
        with self.assertRaisesRegex(ValueError, "final acceptance"):
            journal.validate_request(self.request, self.root)
        self.request["input"].update(final_acceptance={"by": "Human", "reference": "approval", "scope": "v1"},
                                     mandatory_acceptance_criteria=[{"status": "passed", "evidence": "AC"}],
                                     checks=[{"kind": "compile", "status": "passed", "evidence": "compile"}],
                                     task_ids=[PAGE], feature_status="Готово (базовая часть)")
        with self.assertRaisesRegex(ValueError, "Play Mode"):
            journal.validate_request(self.request, self.root)
        self.request["input"]["checks"].append({"kind": "play_mode", "status": "not tested", "evidence": "missing"})
        with self.assertRaisesRegex(ValueError, "must pass"):
            journal.validate_request(self.request, self.root)

    def test_append_requires_replay_marker(self):
        self.write_request()
        self.request["targets"][0]["allowed_sections"] = ["Результат"]
        change = self.request["input"]["changes"][0]
        change.update(kind="content", section="Результат", mode="append", content="Принято")
        with self.assertRaisesRegex(ValueError, "publication marker"):
            journal.validate_request(self.request, self.root)
        marker = "TroughGame/progress-001/status-001"
        change.update(publication_marker=marker, content="Принято\n" + marker)
        journal.validate_request(self.request, self.root)


if __name__ == "__main__":
    unittest.main()
