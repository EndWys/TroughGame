---
name: unity-feature-development
description: Run TroughGame's workflow for a new feature from approved requirements or an explicitly requested continuation of its recorded workflow. Do not use for standalone bugs, research, or ordinary existing-feature changes.
---

# Unity Feature Development

## Purpose and Scope

The agent owns state, research, implementation, and available checks within
approved scope. The developer accepts product decisions, architecture, Play Mode
results, and final readiness. New states use `1.4-tg.1`.
Use for a new feature from agreed requirements or an explicit recorded continuation.
Standalone bugs, research, reviews, tooling setup, and existing-feature improvements
do not create feature state. Reopen a completed workflow only when explicitly requested.

## Documents and Rules

Read `AGENTS.md`, `docs/ai-workflow/DOCUMENTATION_STANDARD.md`, and
`docs/ai-workflow/workflow-reference.md`. Use `docs/project-overview.md` and
`docs/project-map.md` for context. Policy owners are `docs/ARCHITECTURE_RULES.md`,
`docs/CONVENTIONS.md`, and `docs/SCENE_HIERARCHY_STANDARD.md`.
Developer instructions and existing project standards take precedence.

| When to Read | Reference |
| --- | --- |
| Start/resume, write, commit, or cleanup | [state-and-git.md](references/state-and-git.md) |
| Prepare requirements | [requirements-contract.md](references/requirements-contract.md) |
| Research and decomposition | [project-research.md](references/project-research.md) |
| Local architecture | [subtask-architecture.md](references/subtask-architecture.md) |
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

Notion uses a separate worker through `notion-project-coordination`; see
`docs/notion/feature-integration.md`. The worker returns sources and proposed
requirements/acceptance criteria (AC); the main agent obtains approval and owns
its own contract. Do not call MCP directly, read/edit worker journals, or add Notion
sync fields to feature YAML. Refresh source observations before resume/implementation;
material changes require an approved revision.

## Start and Resume

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
| `play_mode_verification` | Scenario and actual result | Failure → diagnosis; success → plan update |
| `plan_update` | Justified delta or unchanged plan | Plan changes need approval |
| `final_verification` | Every requirement/AC mapped to actual checks | Workflow audit before final readiness |
| `workflow_audit` | Executed audit and presented result | Developer final readiness decision |
| `retrospective` | Up to three supported improvements or none | Eligible candidates → knowledge promotion |
| `knowledge_promotion` | Approved rule, rejection, or not applicable | Exact change, separate commit, completion |

Do not create/split/rename/reorder subtasks without the approved initial plan or
an explicitly requested approved plan delta. A correction remains the same subtask.
An actual defect in implementation/checks/audit/review/Play Mode invokes `unity-bugfix`
embedded in the feature: current FEAT/interruption, `bugs/<bug-id>/case.md`, one
host state and checkpoint. Diagnostic and fix gates are separate; after review/retest,
explicitly return to the host flow.

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

At meaningful accepted gates pass immutable facts for `publish_progress`/
`update_documentation`. After final acceptance, pass `finalize_feature` with exact
tasks, AC/checks/Play Mode, commits, limitations, and the actual business status.
Report development and Notion outcomes separately on publication failure; retry
through the worker without replaying implementation. Complete only by workflow-reference criteria.
