---
name: unity-feature-development-v2
description: Coordinate the new TroughGame feature workflow only when explicitly invoked by the developer. Route one stage at a time to its skill and require developer permission for stage transitions. Never start implicitly or substitute the legacy workflow.
---

# Feature Workflow Coordinator V2

## Entry and Scope

Start only on a direct developer command to use this new workflow. Developing this
skill pack is not a command to start a game feature. Do not migrate existing features
or load `unity-feature-development` and its references as a fallback.
After the initial invocation, continue the same feature without repeated invocation;
permission to enter each next stage is still required.

This skill owns routing, the compact `state.yaml`, optional `decisions.md` and
`process-history.md`, not requirements, design, implementation or stage-specific
response formats. Stage skills run in the same agent; loading a skill does not
authorize spawning another agent.

## Stage Route

The following names reserve the new pack's stage skills. Until a named skill has
been created and accepted, that stage is unavailable. Do not invent its instructions,
run its work in the coordinator, or create a placeholder result.

| Stage | Skill | Result needed before requesting transition |
| --- | --- | --- |
| `requirements` | `unity-feature-requirements-v2` | Agreed behavior, scope and acceptance criteria |
| `planning` | `unity-feature-planning-v2` | Project research and an agreed ordered subtask plan |
| `architecture` | `unity-feature-architecture-v2` | Agreed technical solution for the current subtask |
| `implementation` | `unity-feature-implementation-v2` | Implemented subtask and actual technical check results |
| `review` | `unity-feature-review-v2` | Diff findings resolved and developer acceptance of the diff |
| `verification` | `unity-feature-verification-v2` | Observable playtest cases presented and their behavior accepted by the developer |
| `acceptance` | `unity-feature-acceptance-v2` | Whole-feature requirements coverage and developer readiness acceptance |
| `notion_finalization` | Existing `notion-feature-finalization` | Verified final synchronization or not applicable |
| `retrospective` | `unity-feature-retrospective-v2` | Brief process observations and proposals, or none |

Repeat architecture → implementation → review → verification for each planned
subtask. After all subtasks, proceed to acceptance → notion_finalization →
retrospective → complete. Notion finalization remains the penultimate stage.
Plan changes require an explicit developer decision; do not add a plan-update stage.

## Dispatch and Transitions

1. Identify the feature and current stage from the developer's instruction and
   existing local records. For a genuinely new feature, copy the
   [V2 state template](templates/state.yaml) to `.ai/features/<feature>/state.yaml`,
   set its ID and record the initial authorization. On resume, read existing state
   and only the artifacts needed for that stage; do not recopy or replay accepted work.
2. Confirm permission to enter the stage. Initial workflow invocation permits the
   requirements stage. Later permission must identify the next stage or clearly
   authorize continuing along the stated route. Acceptance alone is not permission
   to continue unless the developer also authorizes continuation. Preserve already
   granted permission; do not ask for it again.
3. Load only the selected skill at `.agents/skills/<skill-name>/SKILL.md`. Pass the
   feature, current subtask when applicable, relevant approved inputs and scope.
   Missing skill or mandatory input: report the exact gap and stop dependent work.
4. Let that skill produce its defined result and perform its own required checks.
   Do not duplicate its instructions, response or report in the coordinator.
5. Record the material result and developer decision in state. At the handoff, name
   the proposed next stage and wait for permission unless already authorized.
   Passing checks, silence and an agent's recommendation are not developer permission.

Feedback requesting a correction authorizes that specified return within existing
scope. A failed check alone does not authorize changing stage or scope: report the
failure and proposed return. Keep the same subtask; do not start `unity-bugfix`,
create a new task, or replace the accepted plan.

## Files and Project Rules

Follow AGENTS and the applicable architecture, naming, scene and UI standards.
Stage skills define their own concise result formats. Read technical lifecycle
guidance only where relevant; do not load all standards/references on every handoff.

Use only the agreed feature files under `.ai/features/<feature>/`: `state.yaml`,
`requirements.md`, `feature-plan.md`, optional `decisions.md`/`process-history.md`, and
`subtasks/<subtask>/architecture.md`. Requirements, planning and architecture skills
own their respective documents; the coordinator alone writes state, decisions and
process history from stage results and actual developer instructions.
Do not copy full artifacts into state or create extra reports, journals, templates
or scripts. Tool evidence belongs in
ignored `.ai/evidence/`. Preserve existing records and unrelated Git changes.
Workflow CHANGELOG records changes to the workflow, not feature progress.
Existing scoped commit/check requirements remain; loading this skill grants no
additional commit, push, merge or release permission.

## State and Decisions

Follow the compact format and field rules in the [state template](templates/state.yaml).
Keep stage/subtask, accepted outcomes, brief check results and next action current.
Add only approved plan subtasks; preserve their IDs. Store review dispositions and
developer playtest observations when needed to resume and establish final coverage.
For accepted documents, reference their acceptance basis instead of duplicating it.
Record permission for the next action separately from acceptance of the last result.
Do not overwrite prior valid permission or infer it from silence. Store process
events only in `process-history.md`; other stage skills do not routinely load it.

Create `decisions.md` from the [decision template](templates/decisions.md) only for a
significant accepted product/design decision. Keep choice, reason, impact and human
confirmation; no routine approval ledger or unaccepted options. Append stable `DEC-*`
records, marking an old record superseded when a new accepted decision replaces it.
The affected stage document retains the full solution. Update records only when
the underlying result/decision changes, not after every command or message.

## Process History

Create `.ai/features/<feature>/process-history.md` from the
[process history template](templates/process-history.md) only at the first significant
process event. Follow its entry format and update rules. Record developer-reported instruction/response problems, avoidable
returns or repeated corrections, consequential tool blockers and agreed process
adjustments. Do not log routine commands, code edits, successful checks, normal
approval waits or approvals already recorded elsewhere.

The coordinator is the sole writer. Retrospective reads this history; other stage
skills do not maintain or routinely load it. Do not reread the entire history at
every handoff. Keep it local, outside Notion publication. This is a sparse factual
source for process review, not a transcript or an additional approval gate.

## Existing Specializations

At requirements, pass selected sources and any existing Notion packet to
`unity-feature-requirements-v2`. That stage alone owns conditional intake through
`notion-feature-requirements`; the coordinator does not dispatch a duplicate read.
At finalization only, use existing `notion-feature-finalization` after whole-feature
acceptance and permission to enter that stage. These operations retain their
isolated-worker/MCP rules. No Notion invocation at other points.
`unity-ui-toolkit` may supply relevant UI guidance inside the current stage; it
does not create a parallel workflow or authorize a transition.

Complete only after the planned subtasks, whole-feature acceptance, finalization
or its explicit not-applicable result, and retrospective are finished. Preserve
accepted development on publication failure; retry only the pending finalization.
