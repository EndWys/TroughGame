---
name: unity-feature-development
description: Run TroughGame's workflow only for a new approved feature or an explicitly requested continuation of its recorded workflow. Do not use for standalone fixes, research, or existing-feature changes.
---

# Unity Feature Development

Run the controlled feature workflow for TroughGame. Own the workflow state, evidence collection and automatic work inside approved scope; the developer owns product decisions, semantic approvals, Play Mode results and final readiness. Use workflow version `1.3-tg.1` for new states.

## Applicability

Use this skill only when the developer starts a new feature from approved requirements, or explicitly resumes that feature's recorded workflow. Do not create a feature directory, subtasks or workflow state for a standalone bug fix, project research, code review, or modification/improvement of an existing feature. An already completed feature enters this workflow again only when the developer explicitly identifies the request as a continuation of that feature workflow.

## References

Read these before acting:

- `AGENTS.md` for non-negotiable project rules.
- `docs/ai-workflow/workflow-reference.md` for stages, roles, gates and completion criteria.
- `docs/project-overview.md` when project context can affect a decision.
- `docs/project-map.md` before broad research or work in an unfamiliar context.
- `docs/ARCHITECTURE_RULES.md` before architecture design, an architecture-relevant implementation or an architecture-focused audit.
- `docs/CONVENTIONS.md` before handwritten C# implementation or C# code audit.
- `docs/SCENE_HIERARCHY_STANDARD.md` before gameplay scene integration.

Before starting or resuming a feature, and before every write or commit stage, read `references/state-and-git.md`.

Before writing a developer-facing result, read the relevant parts of `references/response-style.md`. It controls presentation only. Explicit developer instructions about response format take priority.

Before preparing requirements approval, read `references/requirements-contract.md`. It defines the approved product contract and the developer-facing requirements handoff.

Before presenting requirements approval, research and feature-plan approval, a local architecture, implementation result, final verification, workflow-audit result or retrospective, read `references/stage-deliverables.md`. It defines the non-skippable content that must be visible to the developer; do not replace that content with a link to a feature artifact.

When a subtask changes Unity/Fusion lifecycle code, runtime UI initialization, event-driven initialization, pooling, RPCs, coroutines or `UniTask`, also read `references/unity-lifecycle-safety.md`. It defines the conditional Lifecycle Preflight; do not add an empty lifecycle section when none of those concerns apply.

Before decomposing a feature or proposing its first subtask plan, read `references/project-research.md` and produce its conditional Research Brief. It prevents requirements and architecture from being based on a feature-name search or an unverified assumption.

Before proposing local architecture, read `references/subtask-architecture.md`. It defines the required concrete change inventory and execution flow; omit only sections that are demonstrably irrelevant to the approved subtask.

Before AI code audit, read `references/ai-code-audit.md`. It defines the risk taxonomy, criticality and developer-facing audit result.

When independent read-only inspection could materially help and delegation is authorized in the active environment, read `references/read-only-subagents.md` before delegating. Delegation is optional and never changes stage gates or ownership. Otherwise perform the review as a separate read-only pass in the main agent.

After a feature retrospective, read `references/knowledge-promotion.md` when it has an eligible process-improvement candidate. It controls the developer-approved promotion of reusable knowledge and workflow-version compatibility.

Developer instructions and existing project standards take priority. The active workflow authority is `docs/ai-workflow/workflow-reference.md`. The DOCX under `docs/ai-workflow/sources/` is historical material only; it never overrides TroughGame rules.

## TroughGame integration

- Choose the real owner: reusable infrastructure in Template, reusable gameplay
  in GameCore, current scene-specific behavior in TechnicalPrototype, startup in
  Preloader, application lifetime in Project. Prototype is legacy.
- Keep `Shared` .NET-only, flat `ProjectCore.<Context>` namespaces, existing
  assemblies, and the documented folder/suffix taxonomy.
- Follow the awaited initialization flow. Installers register; features bind
  through their protected helpers and resolve only after binding completes.
- Never move `Assets/ProjectCore/Resources/ProjectContext.prefab` or add `.asmdef`.
- Read the installed `unity:unity-cli` skill for live Editor interaction; use
  status and explicit project targeting before Unity-managed asset operations.

## Start or resume a feature

For a feature sourced from Notion, read
`docs/notion/feature-integration.md` and dispatch through the
`notion-project-coordination` skill. A separate `notion-feature-intake` worker
returns source snapshots and proposed requirements/AC. Consume its values in
your own requirements contract, then apply the existing approval gates. Never
call Notion MCP or read/write the worker journal yourself.

1. Determine the feature identifier from the request; use a stable lowercase kebab-case name.
2. Load `.ai/features/<feature>/state.yaml` if it exists. If not, create the feature directory from `.ai/features/_template/`, replace the feature placeholder, and remove illustrative decision records that do not represent a real pending decision.
3. Read the paired `requirements.md`, `decisions.md`, `research.md`, `knowledge-promotion.md` and current subtask artifact when they exist. Keep YAML factual and compact; store long-form requirements, research, architecture, decisions and promotion records in their dedicated artifacts.
4. Reconcile Git context before resuming a write stage. Do not write, update state or attempt recovery while a mismatch is unresolved.
5. Run the current stage. Do not restart a completed stage without a recorded reason.

## State rules

- Record approvals, decisions, questions, stage results and evidence immediately after they occur.
- Advance state only after the required result exists and the required approval is explicit.
- Use `waiting_for_approval`, `blocked`, `in_progress`, `failed` and `complete` accurately; never use a positive status as a substitute for evidence.
- A state record is not approval evidence by itself. Record who approved, what was approved and the related revision or decision.
- Never treat silence as approval.
- Do not change global project rules, this skill or workflow references from a feature retrospective without a separate developer approval.
- Create subtasks only from a developer-approved initial plan that names them, or from an explicitly requested plan change. Do not split, rename, reorder or remove an existing subtask otherwise. A review correction or implementation defect remains an iteration of the current subtask unless the developer explicitly changes the plan.
- Record an optional correction loop when developer feedback, review changes or a discovered bug interrupts the main flow. Complete its relevant checks, then wait for the developer to explicitly confirm whether the main flow may resume; do not create a new subtask or resume automatically.

## Feature state and Git

Follow `references/state-and-git.md` for state artifacts, reconciliation, checkpointing, automatic commit and evidence cleanup. A review acceptance alone does not commit work: the relevant required Play Mode result must pass first.

Notion workers own independent `.ai/notion/operations/` journals. Do not share
`state.yaml` or add remote synchronization fields to it. Send accepted facts,
source observations and approvals as immutable values; keep your local approved
requirements snapshot independently. Before implementation/resume, ask the
intake worker to compare the current Notion sources with the supplied baseline.
Material requirement changes follow the existing revision/approval gate.

At meaningful accepted transitions, dispatch `publish_progress` or
`update_documentation`. After explicit final readiness acceptance dispatch
`finalize_feature` with actual AC/check/Play Mode evidence, exact accepted tasks,
commit references and the intended existing feature business status. Remote
updates are done by the documentation worker and verified in its receipt.
Report local completion and publication separately if a remote operation fails;
retry via the worker without redoing accepted implementation or changing gates.

## Stage routing

| Stage | Produce | Stop condition |
| --- | --- | --- |
| `requirements` | Requirements, Acceptance Criteria and only blocking questions. | Wait for requirements approval. |
| `research_decomposition` | Relevant project context and a scoped, verifiable subtask backlog. | Wait for plan and scope approval. |
| `subtask_architecture` | Local technical design, exact scope and validation plan for the current subtask, plus a concrete before/after execution summary in the developer response. | Wait for architecture approval. |
| `implementation` | Minimal approved diff, Unity integration, relevant automated validation and a compact review handoff. | If successful, continue to `ai_audit`; stop immediately on ambiguity or scope expansion. |
| `ai_audit` | Independent risk-focused review of the complete relevant code and Unity asset diff. | Correct `P0`/`P1` findings before developer diff review; otherwise continue to it. |
| `developer_review` | Record accept/change decision. | On requested changes, return to `implementation`; on acceptance, wait for Play Mode verification. |
| `play_mode_verification` | Reproducible manual verification scenario or record the developer's result. | On failure, diagnose and loop through the smallest approved fix; on success, continue to plan update. |
| `plan_update` | Fact-based future-backlog update, or an explicit unchanged result. | Wait for plan-change approval only when the plan changes; otherwise advance to the next subtask or final verification. |
| `final_verification` | Requirement and AC traceability with real evidence and `passed`, `failed` or `not tested` status, recorded and reported to the developer. | Continue to `workflow_audit`; do not claim final readiness yet. |
| `workflow_audit` | Executed workflow audit, stored result and a developer-visible summary of findings or no-change result. | Wait for the developer's final readiness decision. |
| `retrospective` | At most three evidence-based process-improvement proposals, or no-change result. | Route eligible proposals to `knowledge_promotion`; otherwise complete the workflow. |
| `knowledge_promotion` | Developer-approved permanent knowledge change, or a recorded rejection/not-applicable result. | Apply only the approved candidate, validate, commit separately and complete the workflow. |

## Approval and uncertainty

- Stop before product or architecture decisions, scope expansion, a required human gate or any behavior not specified by approved requirements.
- Ask the smallest concrete question that unblocks the work. Explain the decision's effect, but do not invent a default behavior.
- Finish all authorized read-only analysis and preparation before asking an approval question.
- Mechanical operations within approved scope do not require a new approval.
- Use the `DEVELOPER DECISION REQUIRED`, `BLOCKED` and `SCOPE EXPANSION REQUIRED` response conventions when applicable.

## Research Brief (before decomposition)

Before proposing a feature plan, follow `references/project-research.md`, record the approved Research Brief in `research.md`, and link it from state. Do not submit the plan until relevant integration points and material unknowns are identified.

## Subtask architecture (before implementation)

Before requesting approval, follow `references/subtask-architecture.md`, store the plan in `subtasks/FEAT-XX.md` and link it from state. Do not implement until the approved artifact exposes the code/API/Unity surface and validation path. Use `references/stage-deliverables.md` for the developer-facing approval summary.

## Lifecycle Preflight (when relevant)

When applicable, follow `references/unity-lifecycle-safety.md` and include its map in the subtask artifact. For a relevant networked or initialization-order change, Play Mode covers affected peer roles and one relevant lifecycle transition before automatic commit.

## Implementation and validation

- Work only inside the approved current-subtask scope. Do not perform unrelated refactors.
- Follow `AGENTS.md` for Unity CLI and live Editor interaction. Direct C# source changes are permitted; Unity-managed scenes, prefabs and assets require the prescribed Editor preflight.
- Run `Tools/Architecture/Validate-Architecture.ps1 -Mode Strict`, `dotnet build ./ProjectCore.Runtime.csproj --no-restore -v:minimal`, and `git diff --check` when relevant. Review warnings and record missing generated-project prerequisites honestly.
- Run `Tools/AI/validate-unity.ps1` after production-code changes and fixes. It resolves the Unity CLI from `-UnityCliPath`, `TROUGHGAME_UNITY_CLI_PATH`, PATH, then the standard Windows CLI install location. The value must be the Unity CLI executable, not `Editor/Unity.exe`. It validates through the connected Editor's Pipeline, targeting this repository explicitly. If it reports unavailable or incomplete validation, record the result; continue independent authorized preparation, but do not claim runtime readiness or bypass required validation. Do not launch, close or substitute an Editor silently. Select the narrowest applicable mode and record its JSON result path and outcome.
- Do not ask the developer to run a check the agent can run. Do ask for manual Play Mode evidence when it is required and cannot be reproduced reliably by the agent.
- Do not report implementation or a feature complete while applicable checks fail or required evidence is absent.

For a subtask with a Lifecycle Preflight, implement and audit against the approved dependency map. If the real owner, initialization order or readiness contract differs from the map, stop for architecture revision before moving the first calculation or view update to another lifecycle point.

## Workflow audit and retrospective

Follow the workflow-audit, reporting and retrospective sequence in `docs/ai-workflow/workflow-reference.md`. Do not bypass its closure gate.

## Feature completion

Do not mark the feature complete until the completion criteria in `docs/ai-workflow/workflow-reference.md` are satisfied.
