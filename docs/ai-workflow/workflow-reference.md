# Unity Feature Development Workflow

## Purpose and status

This document is the operational Markdown reference for the TroughGame Unity feature-development workflow. It replaces manual copying of prompt templates with a shared process that a feature-development skill can execute.

It applies only to a new feature started from approved requirements, or an explicitly requested continuation of that feature's recorded workflow. Standalone bug fixes, project research, code reviews, and modifications or improvements to an existing feature are outside this workflow until they receive their own approved process.

Current version: **1.3-tg.1**, see [CHANGELOG.md](CHANGELOG.md).
The developer selected the original staged approvals and automatic local
subtask commits after accepted review and successful Play Mode verification
during adaptation on 2026-10-12.

Developer instructions and TroughGame's `AGENTS.md`, `docs/ARCHITECTURE_RULES.md`,
`docs/CONVENTIONS.md`, and scene hierarchy standard remain authoritative for
project policy. This document owns workflow semantics, not architecture or
style. The [source DOCX](sources/AI_Unity_Development_Workflow_Base_v1.2.docx)
is historical comparison material and has no authority in this project.

## Operating model

The developer starts one eligible feature workflow. The agent proceeds through the current stage, records the result and stops only at a semantic approval gate, ambiguity, scope conflict, or a required developer-run Play Mode verification.

The developer remains responsible for product decisions, approvals, Play Mode validation and the final decision to merge, send to QA, or release. The agent is responsible for research, bounded implementation, automated validation that it can run, evidence collection, and explicit reporting of uncertainty.

## Terms

| Term | Meaning |
| --- | --- |
| Vertical slice | The smallest end-to-end scenario that passes through the required system layers and produces an observable result. |
| Technical spike | A short experiment that resolves a technical unknown before the main implementation. |
| Subtask scope | The approved boundaries of a subtask: allowed systems, files and Unity objects. Any expansion needs developer approval. |
| AI code audit | An independent review of the diff for Unity/C# risks: lifecycle, subscriptions, async/cancellation, serialization, resources and allocations. |
| Acceptance Criterion (AC) | An observable, testable condition that proves expected feature behavior. |
| Regression test | A test that reproduces a discovered defect and prevents it from returning. |

## Non-negotiable rules

- Approved requirements and explicit developer decisions are authoritative.
- Do not invent gameplay behavior, edge-case handling, architecture decisions, or evidence that has not been supplied or observed.
- Work only within the approved scope. Stop before modifying an out-of-scope file, system, Unity object, serialized reference, prefab, scene, ScriptableObject, or composition root.
- Do not perform unrelated refactors.
- Each completed subtask receives an independent AI code audit, a developer review of the complete relevant code and Unity asset diff, and the required Play Mode result before its accepted implementation is committed.
- Run relevant automated validation after every production-code implementation or fix when the project provides a runnable check. If it cannot be run, report the missing check explicitly.
- Do not treat silence as approval.
- Do not report a stage or feature as complete while applicable validation is failing.

For every local-architecture approval, the developer-facing response summarizes the real before/after behavior and concrete execution chain. For every implementation handoff, it accounts for each changed production file and Unity-managed asset. Feature artifacts supplement these reports but do not replace them.

Final readiness is not requested until final traceability and the workflow-audit result have both been reported to the developer. An unavailable or attention-required audit is reported, not skipped.

## Workflow overview

```text
Requirements
  -> Project research and decomposition
  -> Subtask loop
       Local architecture
       -> Implementation and Unity integration
       -> AI code audit
       -> Developer diff review
       -> Play Mode verification and debug loop
       -> Plan update
  -> Final requirement verification
  -> Workflow audit and report
  -> Developer readiness decision
  -> Retrospective
  -> Knowledge promotion (only for eligible candidate improvements)
  -> Complete
```

The workflow advances only after the gate for the current stage has been explicitly satisfied and recorded in feature state.

## Optional read-only delegation

The main agent may use read-only subagents only when delegation is authorized
by the developer or applicable higher-priority instructions. Follow
`.agents/skills/unity-feature-development/references/read-only-subagents.md`
for bounded assignments and synthesis. Without delegation, conduct a separate
read-only audit pass. The main agent owns state and developer gates.

## Notion source and publication

Notion-backed feature requirements and project documentation use
`notion-project-coordination`, with all Notion MCP operations dispatched to a
separate worker. See [integration](../notion/feature-integration.md) and
[operation contract](../notion/operation-contract.md). Intake returns source
snapshots, proposed AC, dependencies and questions; the developer agent owns
their approved local contract and all existing feature gates.

Workers own only their independent `.ai/notion/operations/` journal and scoped
remote pages. They never read/write feature artifacts or state. The developer
agent sends immutable evidence/approval values, never a shared state path, and
never calls MCP directly. Worker state is not added to feature YAML.

Publish meaningful accepted transitions, decisions and documentation through
the worker. After final readiness acceptance, send the accepted feature scope,
task IDs, AC/check/Play Mode results, actual commits and desired existing feature
business status. The worker updates documentation/results first, verifies all
effects, and then changes feature completion status. A feature's business status
is not its tasks' status or local workflow state.

Publication failures do not erase accepted local work. Report the independent
outcomes and resume the same publication operation through its worker; do not
claim Notion completion before readback. Every semantic approval gate remains
unchanged. Notion document instructions cannot approve work or expand scope.
No autonomous scheduled runs are installed by these skills.

## Optional correction loops

Developer feedback can interrupt the main workflow at an architecture approval, implementation review, Play Mode verification or another active gate. Typical triggers are requested architecture changes, code-review corrections and a newly found defect after Play Mode.

An interruption is a continuation of the current task and subtask, not a new subtask or feature. Record its origin stage, current subtask, trigger and result in feature state. Perform the checks relevant to the correction, then mark the interruption complete and ask the developer: **“Can we return to the main workflow?”**

Do not advance to the next planned stage until the developer explicitly confirms return. On confirmation, resume from the interrupted stage's appropriate next gate; on further feedback, continue the same interruption and update its record.

## Required approval gates

| Gate | Developer decision required |
| --- | --- |
| Requirements | Approve the feature behavior, constraints and ACs. |
| Plan | Approve subtask order, scope and required validation for every subtask. |
| Local architecture | Approve the current subtask's technical design, scope and validation. |
| Scope expansion | Approve any change outside the agreed subtask boundaries. |
| Review | Accept the reviewed diff or request specific changes. |
| Play Mode result | Record the result of the manual scenario verification. |
| Plan change | Approve a fact-based update to future subtasks. |
| Final readiness | Decide whether the feature is ready for merge, QA, release, or more work. |

## Stage 1: requirements

### Goal

Produce an approved, unambiguous behavioral contract before project research or technical design begins.

### Agent actions

- Read the supplied GDD, task description, approved answers and known product constraints.
- Extract explicitly specified behavior and constraints.
- Produce observable, testable ACs.
- Identify contradictions, unspecified states, transitions and edge cases that affect implementation or verification.
- Re-check the authoritative source before asking a question. Ask only when it is absent, contradictory or leaves a behavior or implementation-impacting decision open; do not ask the developer to repeat directly specified content.
- Trace every requirement to its source.

### Boundaries

- Do not design classes, services, APIs, data structures or Unity integration at this stage.
- Do not convert an assumption into a requirement.
- Do not ask questions about details that do not affect behavior or ACs.

### Required result

| Output | Minimum contents |
| --- | --- |
| Requirements | ID, behavioral statement, source. |
| Acceptance Criteria | ID, observable test condition. |
| Questions | ID, unknown, authoritative source checked, why the answer is blocking. |
| Approved assumptions | Only explicitly confirmed assumptions. |

Stop for requirements approval if a blocking question exists or the behavioral contract has changed.

Record the full approved contract in `.ai/features/<feature>/requirements.md`, link it from feature state and present the same applicable content in the developer-facing approval response. The contract contains no implementation design.

## Stage 2: project research and decomposition

### Goal

Find the relevant existing project context and form an approved backlog of small, verifiable subtasks.

### Agent actions

- Perform read-only research based on responsibilities and data, not only the feature name.
- Check relevant concerns when applicable: time, persistence, backend/API, synchronization, rewards/inventory, UI/navigation, configuration, localization, DI, scenes/prefabs, serialized references and test utilities.
- Find existing features, services, interfaces, components, scripts and partial implementations that can be reused or extended.
- Identify technical unknowns that need a spike before implementation.
- When lifecycle-sensitive work is likely, trace the initialization owner and readiness path for the relevant services, configurations, models and Views; do not infer readiness from a Unity or Fusion callback name.
- Produce a compact Research Brief that maps each relevant requirement or AC to its source, affected concern, concrete existing implementation (or bounded negative finding), reuse/extension decision and planning implication.
- Build subtasks in dependency order.

Research by responsibility, data and integration boundary rather than feature name alone. Inspect only applicable concerns: gameplay, synchronization, peer authority, UI, localization, configuration, DI, persistence, scenes/prefabs and validation. Do not approve a plan based on unverified claims that an existing system does or does not exist.

Record the approved Research Brief and initial feature plan in `.ai/features/<feature>/research.md` and its compact status/reference in state so the workflow can resume without relying on chat history. Present the same applicable content in the developer-facing approval response; do not replace it with raw search logs or a link to the artifact.

### Subtask contract

Every proposed subtask must state:

- one concrete, verifiable result;
- dependencies and ordering constraints;
- allowed scope: systems, files and Unity objects that may change;
- explicitly out-of-scope areas;
- the required verification method.
- the Research Brief finding that establishes its reuse point, ownership boundary or dependency.
- expected public-contract and Unity-integration surface when it is already known from research.

Do not split work merely by code artifact such as a DTO or interface. Do not use overlapping scope unless the dependency is explicit and the developer has approved it.

The listed subtasks are proposals until the developer approves the initial feature plan. Do not create them in state, begin local architecture or modify project files before that approval.

### Subtask creation and correction rule

Create initial subtasks only from the developer-approved feature plan that names them. After that approval, create, split, rename, reorder or remove a subtask only when the developer explicitly requests a plan change. A bug found in Play Mode or a correction requested during review remains a fix iteration of the current subtask; it does not create a new subtask. If the correction exceeds approved scope, request a scope or architecture revision for the same subtask, not a new backlog item. After the correction repeats its relevant validation and review gates, wait for the developer's renewed decision before advancing.

Stop for plan and scope approval before designing the first subtask.

## Subtask loop

Repeat this entire loop for one approved subtask at a time.

### Stage 3: local architecture

#### Goal

Define the smallest technical solution that meets the current subtask contract.

#### Agent actions

- Identify exact files, types, methods and public API changes.
- Describe data ownership, dependencies, lifecycle, subscriptions, cancellation and disposal.
- Identify serialized changes, Unity object integration, configuration and DI registration when applicable.
- For Unity/Fusion lifecycle code, runtime UI initialization, event-driven initialization, pooling, RPCs, coroutines or `UniTask`, include a lifecycle dependency map: entry point and owner, proven-ready state, dependencies used, dependencies deferred, safe calculation/update point, and cleanup or invalidation path. Include the resulting initialization chain in execution order.
- Show the execution chain from trigger to domain change and observable result, naming concrete owners, methods, events and Views.
- Name a pattern, principle or extension point only when it materially influences the design; explain its role rather than adding an empty placeholder.
- Define the validation and evidence required for the subtask.
- Surface any technical ambiguity, prerequisite or required scope expansion.

#### Required result

| Area | Required detail |
| --- | --- |
| Scope and outcome | Observable result, included systems and meaningful explicit exclusions. |
| Change inventory | Every expected file/type/asset; its `existing / reuse`, `extend`, `new` or `new asset` status; responsibility, API/data surface and dependencies. |
| Data, state and lifecycle | New or changed APIs, fields, events, signals or serialized references; ownership, callers/implementations, state transitions, compatibility and cleanup impact. |
| Execution chain | Concrete `Type.Member(...)` trigger → owner → domain mutation → event/model update → View/observable result, plus initialization prerequisites. |
| Unity integration | Exact prefabs, scenes, GameObjects, components, ScriptableObjects, serialized fields, installers, localization entries or assets affected. |
| Lifecycle safety | Required only for lifecycle-sensitive work: dependency map, initialization chain, delayed-work and pooling/despawn handling. |
| Design choices | Patterns, principles or extension points only when they influence the implementation. |
| Validation | Automated and Play Mode checks, with expected evidence. |
| Risks and scope conflicts | Only material unknowns, developer decisions and actual scope conflicts; no empty placeholder section. |

Stop for local-architecture approval before modifying the project.

Record each approved local architecture in `.ai/features/<feature>/subtasks/FEAT-XX.md`, including its lifecycle map when applicable, and link it from the matching state subtask.

The developer-facing architecture response contains the same applicable plan content, not merely a link to the artifact.

When a concrete discovery changes the approved scope, request a scope expansion. When it changes the real owner, contract, lifecycle or dependency inside approved scope, request an architecture revision. In both cases, show only the evidence-based delta, update the same subtask after approval and do not create a new subtask.

### Stage 4: implementation and Unity integration

#### Goal

Implement the approved subtask as the smallest coherent diff.

#### Agent actions

- Modify only approved scope and follow project conventions.
- Keep the change focused; do not refactor adjacent code without approval.
- Run relevant automated validation after production-code changes and fixes.
- For Unity Editor integration, read the target object first; make only required changes; then re-read it, save it and check the Console.
- If a safe tool operation is unavailable, report the precise manual integration step required from the developer.

Before AI code audit, provide a compact review handoff: material changed areas, concise behavior change, review focus, automated validation and actual limits. Group mechanical companion files by reviewable responsibility, but surface public contracts, Unity-managed assets, configuration changes and known limits separately. The complete diff remains the source for line-level review.

#### Unity asset checks

The developer verifies manual or tool-assisted asset changes in the Inspector, including prefab overrides, serialized references and the Git diff of changed assets.

Stop immediately if the approved scope must expand.

### Stage 5: independent AI code audit

#### Goal

Review the full relevant code and Unity asset diff independently of the implementation pass.

#### Review focus

- state consistency, null handling and error paths;
- Unity lifecycle readiness, initialization order and subscription/unsubscription symmetry;
- async flow, cancellation, disposal and resource release;
- serialization, inspector references, prefabs and ScriptableObjects;
- unnecessary allocations and performance risks where relevant;
- unintended or out-of-scope changes;
- whether implementation and validation evidence match the approved local architecture.

Each finding includes its concrete location, risk type, criticality, evidence, consequence and smallest recommended correction. `P0` and `P1` findings return to implementation; `P2` is a visible developer-review recommendation. Style-only comments should be omitted unless they conceal a correctness or maintenance risk. When no actionable findings exist, report the reviewed scope and relevant analysis areas rather than an empty checklist.

### Stage 6: developer review

The developer reviews the complete relevant code and Unity asset diff for correctness, readability, API quality, error handling and unrelated changes. The developer accepts the diff or returns specific required changes. Review acceptance authorizes the future automatic commit; it does not commit before the required Play Mode result.

Any accepted fixes return to implementation, then relevant automated validation and AI code audit are repeated.

### Stage 7: Play Mode verification and debug loop

#### Developer input

The developer verifies the approved subtask scenario in Play Mode. For a defect, provide the reproduction steps, expected and actual result, logs and available context.

For a subtask that changes networked or initialization-order behavior, the scenario covers every affected peer role (host/client when both execute the changed code) and at least one affected spawn, ready-state, despawn or pooled-reuse transition. This is required before the automatic commit.

#### Agent response

- Analyze the report, logs and relevant code.
- State a confirmed cause, falsifiable root-cause hypothesis or evidence-insufficient result, naming concrete location and supporting evidence.
- Propose the smallest correction within the current subtask scope, with its repeated validation and Play Mode scenario.
- Request a specific additional Play Mode run, log or test if evidence is insufficient.

A reported defect and its correction remain an iteration of the current subtask. Request a scope expansion only when the correction actually exceeds approved scope; do not create a new subtask.

Repeat diagnosis, correction and verification until the defect is resolved or a product requirement gap requires a developer decision. A defect found here should receive a regression test when practical and appropriate to the project.

### Stage 8: plan update

After successful subtask verification, compare newly observed facts with the approved backlog.

Record facts that affect future work, but do not create, split, rename, reorder or remove subtasks unless the developer explicitly requests a plan change. Do not change requirements here. If there is no explicit plan-change request, record the plan as unchanged briefly.

When the developer requests a plan change, show only the evidence-based plan delta: fact, affected subtasks, proposed change, reason and delivery impact. Stop for approval before proceeding under the changed plan.

## Final feature verification

### Developer verification

The developer verifies the complete user journey and key edge cases in the target scene or build, plus affected Inspector state, Unity assets and the absence of new Console errors.

### Agent traceability check

For every approved requirement and AC, record:

| Field | Meaning |
| --- | --- |
| Requirement or AC ID | The approved contract item. |
| Implementation | The concrete implementation that addresses it. |
| Verification method | Automated test, Play Mode/build scenario, log, state snapshot or Inspector evidence. |
| Evidence | The actual result or link/path to it. |
| Status | `passed`, `failed`, or `not tested`. |

Code existing is not evidence that behavior passed. Do not hide failed or untested items and do not decide merge, QA or release readiness for the developer.

Record the full traceability in `.ai/features/<feature>/final-verification.md`, link it from state and report the result to the developer before proceeding to workflow audit.

If any approved requirement or AC is `failed` or `not tested`, return to the relevant existing subtask or request the missing evidence. Do not create a new subtask or request final readiness.

## Workflow audit and report

After final traceability and before asking for final readiness, run `Tools/AI/workflow-audit.ps1 -Feature <feature-kebab-case>`. Record its status and evidence paths in feature state, then report its command result, status and actionable process findings (or explicit no-actionable-findings result) to the developer. The audit does not repeat code review, requirements traceability or architecture. If it cannot run or reports `attention_required`, state that condition visibly; neither outcome authorizes skipping the gate or automatically changing workflow knowledge.

## Retrospective

After final readiness, collect only facts from the workflow: repeated iterations, decomposition or scope mistakes, scope expansions, review findings, checks that found defects, workflow-audit findings and steps that did not help.

Propose at most three small, repeatable process improvements. Each proposal must include the observed fact, conclusion, exact change, target location, expected effect and owner. Valid targets include `AGENTS.md`, a workflow reference, a skill, a checklist or a validation command. Report an explicit no-actionable-improvements result when there is no supported proposal.

Do not promote a one-off workaround, an unapproved assumption or a feature-specific product rule into a permanent project rule. The developer decides whether any proposed improvement becomes project policy.

## Knowledge promotion

After retrospective, evaluate its proposals using `.agents/skills/unity-feature-development/references/knowledge-promotion.md`. This is a post-feature controlled-learning gate, not a new feature stage or subtask: it turns only an approved reusable lesson into one canonical project rule.

Each candidate identifies a factual observation, evidence, proposed testable rule, applicability, canonical target and duplicate/conflict check. The developer explicitly approves, rejects or changes the candidate. On approval, the agent applies only that bounded change, runs target-appropriate validation, records its evidence and creates a separate commit. If no candidate is eligible, record that result and complete the workflow.

New feature states use the current workflow version from `CHANGELOG.md`. Do not silently migrate an existing state; a change to approval semantics or a required gate needs explicit developer migration approval.

## Response presentation

Response structure and formatting are defined by the feature skill's `references/response-style.md`. Use the smallest format that makes the current result, evidence and required developer action clear.

## Feature state requirements

The feature workflow records factual state in `.ai/features/<feature>/state.yaml`: feature name, workflow version, current stage, status, approved requirements and plan revisions, requirements/Research Brief/subtask artifact references, current subtask, subtask statuses, pending questions, decisions, approvals and validation evidence.

Long-form approved decisions belong in `.ai/features/<feature>/decisions.md`; research and local architecture belong in their dedicated feature artifacts. State must be reconciled with the actual Git diff and validation results when a session resumes; it is never trusted blindly after an interrupted write operation.

## Completion criteria

A feature is ready for the developer's final readiness decision only when:

- requirements, plan and local architectures received required approvals;
- every implemented subtask completed its review and Play Mode loop;
- applicable automated validation is passing, or every missing check is explicitly recorded;
- final traceability marks all requirements and ACs as `passed`, `failed` or `not tested` with real evidence, is stored in `final-verification.md` and has been reported to the developer;
- the workflow audit has executed, been stored in feature state and its result has been reported to the developer;
- known gaps and limits are visible;
- the developer has reviewed the final relevant diff and makes the final decision.

After the final readiness decision, record retrospective and knowledge promotion
as completed or explicitly not applicable, then complete the workflow. They
must not be prerequisites for the earlier final readiness decision.
