---
name: unity-feature-development
description: Start TroughGame's feature-development workflow only on a direct developer command to use this skill, then continue that feature through its recorded flow. Never select implicitly from a generic feature request or another skill.
---

# Unity Feature Development

## Purpose and Scope

The agent owns state, research, implementation, and available checks within
approved scope. The developer accepts product decisions, architecture, Play Mode
results, and final readiness. New states use `1.7-tg.1`.
Start a new feature workflow only when the developer explicitly invokes this skill
by name or commands its use. A generic request to develop a feature, a specification,
a task URL or another skill's recommendation does not activate it or create state.
After that initial invocation, continue development and later authorized resumptions
of the same feature through this flow without requiring another skill invocation.
Preserve its accepted approvals, current stage and existing authorization across turns.
Do not automatically adopt unrelated work or restart completed stages.
Standalone bugs, research, reviews, tooling setup, and existing-feature improvements
do not create feature state. Reopen a completed workflow only when explicitly requested.

## Documents and Rules

Read `AGENTS.md` and `docs/ai-workflow/workflow-reference.md`. Use `docs/project-overview.md` and
`docs/project-map.md` for context. Policy owners are `docs/ARCHITECTURE_RULES.md`,
`docs/CONVENTIONS.md`, and `docs/SCENE_HIERARCHY_STANDARD.md`.
Developer instructions and existing project standards take precedence.

| When to Read | Reference |
| --- | --- |
| Start/resume, write, commit, or cleanup | [state-and-git.md](references/state-and-git.md) |
| Prepare requirements | [requirements-contract.md](references/requirements-contract.md) |
| Research and decomposition | [project-research.md](references/project-research.md) |
| Local architecture | [subtask-architecture.md](references/subtask-architecture.md) |
| Runtime UI screen, popup, widget or UI-related defect | [unity-ui-toolkit](../unity-ui-toolkit/SKILL.md) |
| Lifecycle, UI, pooling, RPC, coroutine, UniTask | [unity-lifecycle-safety.md](references/unity-lifecycle-safety.md) |
| Code audit | [ai-code-audit.md](references/ai-code-audit.md) |
| Stage handoff and developer response | [stage-deliverables.md](references/stage-deliverables.md), [response-style.md](references/response-style.md) |
| Authorized independent inspection | [read-only-subagents.md](references/read-only-subagents.md) |
| Promote lessons after retrospective | [knowledge-promotion.md](references/knowledge-promotion.md) |

## Ownership and Integration

Choose the actual owner: Template for infrastructure, GameCore for shared gameplay,
TechnicalPrototype for the current scene, Preloader for startup, Project for application
lifetime. Prototype remains historical. Keep Shared .NET-only, flat namespaces
`ProjectCore.<Context>`, existing assemblies, folder categories, and suffixes.
Installers register dependencies. Features use protected DI helpers and resolve
dependencies after registrations finish; initialization is awaited through the unified
flow. Preserve `Assets/ProjectCore/Resources/ProjectContext.prefab`; add no `.asmdef`.
For Editor operations use `unity:unity-cli`, a status preflight, and explicit project targeting.

Notion has exactly two explicit dispatch points; see
`docs/notion/feature-integration.md`. At requirements intake from selected Notion
sources explicitly load `../notion-feature-requirements/SKILL.md` and dispatch its
isolated worker. At `notion_finalization` explicitly load
`../notion-feature-finalization/SKILL.md`. Never invoke/load Notion skills on routine
resume, subtasks, patches, bugs or intermediate decisions. Reuse the local contract.
The main agent owns state and never calls Notion MCP. The new timing limits apply
to active features without rewriting versions, replaying work or revoking gates.

## Start and Resume

For runtime UI, use `unity-ui-toolkit` in this agent and the same FEAT/state.
Its hierarchy/resources/presentation proposal extends the local architecture approval;
new UI visual prototype acceptance occurs inside implementation before complete
logic. It does not replace final diff review or Play Mode. Preserve existing host
approval/state shapes; see its workflow integration for subject/revision records.
Older active states require an explicit compatibility decision before adding the
new prototype gate. Do not silently update their workflow version.

1. Choose a stable `feature-kebab-case`.
2. Load `.ai/features/<feature>/state.yaml`; if absent, use `.ai/features/_template/`,
   replace the ID, and remove fictional decision examples.
3. Read requirements, decisions, research, knowledge candidates, and the current
   subtask. YAML is a compact index; artifacts hold details.
4. Reconcile Git before writing. Unresolved overlap/mismatch blocks writes and
   recovery. Do not repeat a completed stage without a recorded reason.
5. Continue the current stage; record facts, approvals, and checks promptly.

## Stages and Gates

| Stage | Deliverable | Transition / Stop |
| --- | --- | --- |
| `requirements` | Requirements, AC, blocking questions | Requirements approval |
| `research_decomposition` | Source-backed context and verifiable plan | Plan/scope approval |
| `subtask_architecture` | Exact files/API/objects, before/after, checks | Architecture approval |
| `implementation` | Minimal approved changes and Unity integration | Audit on success; stop for scope change/uncertainty |
| `ai_audit` | Separate inspection of all relevant changes | Fix P0/P1 before developer review |
| `developer_review` | Full diff acceptance or specific feedback | Corrections → implementation; acceptance → Play Mode |
| `play_mode_verification` | Scenario and actual result | Failure → current-subtask correction; success → plan update |
| `plan_update` | Justified delta or unchanged plan | Plan changes need approval |
| `final_verification` | Every requirement/AC mapped to actual checks | Workflow audit before final readiness |
| `workflow_audit` | Executed audit and presented result | Developer final readiness decision |
| `notion_finalization` | Whole-feature task/feature statuses and concise accepted specification deltas | Only after full acceptance; verified synchronization or explicit not applicable |
| `retrospective` | Final process revision; up to three supported improvements or none | Optional local knowledge promotion with its existing approval/check/commit rules, then complete |

Do not create/split/rename/reorder subtasks without the approved initial plan or
an explicitly requested approved plan delta. A correction remains the same subtask.
Defects in implementation/checks/audit/review/Play Mode remain corrections in the
current FEAT and approved scope, using existing audit/review/verification gates.
Do not automatically load/invoke `unity-bugfix`, create bug workflow state, import
its gates or ask to activate it. Material scope/design uncertainty uses host gates.
Only a direct developer command starts the manual bugfix process; if requested,
reuse the current artifact/case, one host state and corrected checkpoint. Existing
manual authorization persists; ordinary defects alone never establish it.

`waiting_for_approval`, `blocked`, `in_progress`, `failed`, and `complete` must
match facts. An approval record is not proof: identify the human, exact subject/
revision, and decision source. Silence is not approval; existing authorization
persists. Complete authorized preparation and present the proposal before asking.
Explain a stop with the exact rule's link and short quotation.

## Validation and Delivery

Run applicable AGENTS checks: strict architecture validation, `dotnet build`,
`git diff --check`. After runtime edits use `Tools/AI/validate-unity.ps1`.
CLI resolution: `-UnityCliPath`, `TROUGHGAME_UNITY_CLI_PATH`, PATH, standard
Windows Unity CLI installation. This is the CLI path, not Editor.
The helper targets a connected project Editor. Report unavailable/incomplete results;
they do not prove readiness. Do not silently start/replace an Editor. Execute available
checks yourself; request manual Play Mode only where necessary.
Compare lifecycle with the approved dependency map. Changed owner/order/readiness
requires architecture revision. After accepted review, required passed Play Mode,
and applicable checks, create one accepted checkpoint per state-and-git.

Keep all intermediate gates, bugs, decisions, audits and tests local. After all
subtasks, final verification, presented workflow audit and developer final readiness
acceptance, enter `notion_finalization`. Pass exact task/feature targets and accepted
specification/plan deltas to its worker. No Notion targets means not applicable,
without a worker. Report verified/pending publication separately; retry only this
stage on failure. Then perform `retrospective`, including optional local knowledge
promotion, and complete by workflow-reference criteria. Do not publish retrospectives.
