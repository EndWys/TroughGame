# Knowledge Promotion Reference

## Purpose

Use this reference only after final readiness and the feature retrospective. It decides whether an evidence-based lesson from the completed feature should become one permanent project rule. It does not reopen feature implementation, create a subtask or change product behavior.

The developer owns every promotion decision. The agent may prepare, apply and separately commit only the exact approved change.

## Candidate eligibility

Create at most three candidates from the retrospective. A candidate must be either an explicitly developer-endorsed project direction, a rule supported by independent feature evidence, or a material recurring risk that merits a developer decision.

Do not create a candidate for a one-off workaround, unapproved assumption, feature-specific product rule, temporary tool failure or personal preference that the developer has not adopted as a convention.

## Candidate record

Store the long-form records in `.ai/features/<feature>/knowledge-promotion.md`; keep only their compact index in `state.yaml`.

Each candidate contains:

| Field | Required content |
| --- | --- |
| ID | Stable `KNP-XXX` identifier. |
| Observed fact | Factual observation without a proposed conclusion. |
| Evidence | Concrete feature records, audit findings, validation results or explicit developer decision. |
| Proposed rule | Exact, testable wording. |
| Applicability | When the rule applies; include an exception only when one is known and material. |
| Target | One canonical file. |
| Conflict check | Existing related rule, duplicate or contradiction, if found. |
| Expected effect | Specific future uncertainty, defect or manual work reduced. |
| Decision | `proposed`, `approved`, `rejected`, `change_requested` or `applied`. |
| Result | Changed paths, validation and separate commit when applied. |

## Target selection

| Knowledge type | Canonical target |
| --- | --- |
| Repository-wide hard policy | `AGENTS.md` |
| Architecture boundary, lifecycle, ownership or dependency rule | `docs/ARCHITECTURE_RULES.md` |
| Handwritten C# style, logging or file-structure convention | `docs/CONVENTIONS.md` |
| Stable discovery information | `docs/project-map.md` |
| Feature-workflow stage, gate or evidence rule | `docs/ai-workflow/workflow-reference.md` or the responsible skill reference |
| Developer-facing response presentation | `references/response-style.md` or `references/stage-deliverables.md` |
| Deterministic mechanical action | The responsible `Tools/AI/` script |

Choose one target. Do not copy a rule into several documents. If the candidate would duplicate or contradict an existing rule, show that fact and request a decision instead of silently reconciling it.

## Promotion gate

1. Read the retrospective, workflow audit and cited evidence. Confirm that the candidate meets the eligibility rule.
2. Inspect the proposed target and directly related project guidance for an existing rule, duplication or conflict.
3. Record the candidate and present the `Knowledge promotion` handoff from `stage-deliverables.md`.
4. Wait for the developer to approve, reject or request changes to each candidate. Silence is not approval.
5. For an approved candidate, change only the approved canonical target and any indispensable reference. Run the validation appropriate to that target, including `git diff --check`.
6. If the change alters workflow semantics, add a concise entry to `docs/ai-workflow/CHANGELOG.md`. Do not add changelog entries for unrelated project knowledge.
7. Record the result, evidence and commit in feature state. Create a separate commit for the promoted knowledge; the developer's approval of that candidate authorizes this commit without an additional commit question.

If no eligible candidate exists, record `not_applicable` and complete the workflow. A rejected candidate also completes without a persistent change.

## Workflow-version compatibility

Read the current workflow version from `docs/ai-workflow/CHANGELOG.md` before creating a new feature state. New features use that version. Do not rewrite an existing feature state automatically.

When a resumed feature has a different workflow version and the difference changes a required gate, state contract or approval meaning, explain the delta and ask the developer whether to continue under its recorded version or explicitly migrate it. Editorial corrections do not require migration.
