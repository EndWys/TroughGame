# Feature Development Workflow

## Purpose and Scope

Version **1.7-tg.1**. Start only on the developer's explicit invocation of
`unity-feature-development`. A generic feature request or another skill cannot
activate this workflow. After the initial invocation, continue the same feature and
authorized resumptions through its recorded flow without repeated skill invocation.
Standalone bugs, research, reviews, tooling setup,
and ordinary existing-code improvements do not create feature state.
The user selected staged approvals and automatic local accepted-subtask commits after
required passed Play Mode. Developer instructions, AGENTS, and architecture/code/scene
standards take precedence.

## Inputs and Roles

The developer defines the product contract, accepts semantic gates/manual Play Mode,
and decides merge/QA/release. The agent researches, implements within accepted scope,
runs available checks, and reports actual limits. Inputs: approved specification/
answers/decisions, current code/Unity context, state, approved scope. A separate worker
fetches Notion sources. A vertical subtask has an observable verifiable outcome;
acceptance criteria (AC) prove behavior while audit separately finds concrete risks.

## General Rules

- Change accepted files/systems/objects only. Scope expansion and material changes
  to owner/contract/lifecycle within scope require revision.
- Do not invent product/architecture decisions or perform unrelated refactoring.
  Mechanical implementation of the accepted design requires no extra question.
- Every subtask passes code/Unity audit, full developer diff review, and required
  Play Mode before its accepted commit.
- Report unavailable, failed, and untested checks honestly. Do not complete a stage/
  feature with a failing mandatory check.
- Architecture shows before/after and execution chain. Implementation handoff accounts
  for every changed production file/asset. Artifacts do not replace visible proposals.
- Discuss final readiness after requirement coverage and presented workflow audit.
  Never hide unavailable audit or attention findings.

## Stages and Deliverables

Runtime UI uses [unity-ui-toolkit](../../.agents/skills/unity-ui-toolkit/SKILL.md)
inside the same host FEAT/state. The UI hierarchy/resources/presentation plan extends the
architecture gate; new UI visual prototype acceptance precedes complete logic
within implementation. It never accepts the complete subtask diff. Final review,
Play Mode and checkpoint rules remain. MVVM is the default; the existing architecture
gate can accept a justified MVP choice with exact responsibilities and synchronization.
No extra gate is added. See its
[integration contract](../../.agents/skills/unity-ui-toolkit/references/workflow-integration.md).
For older active versions, agree gate compatibility explicitly before adopting
the new prototype step; do not silently migrate state.

| Stage | Deliverable / Decision |
| --- | --- |
| Requirements | Goal, source-backed RQ/AC, scope/decisions; behavior approval |
| Research/plan | Actual integration, initial FEAT/order/scope/checks; approval before creating subtasks |
| Architecture | Exact files/API/data/lifecycle, Unity/DI, risks/checks; approval before implementation |
| Implementation | Minimal accepted diff, bindings/hierarchy/config, checks, reviewable handoff |
| Code audit | All relevant final changes and P0/P1/P2 risks; required correction before review |
| Review | Accept the whole diff or request changes; not a substitute for Play Mode |
| Play Mode | Original/affected scenario, roles/transitions; failure → current-subtask correction |
| Plan update | Facts affecting future work; unchanged plan unless explicitly changed |
| Final verification | Every RQ/AC linked to implementation/evidence/status; failure/gap → existing FEAT |
| Workflow audit | Command, result/report, findings; present before final decision |
| Final readiness | Developer readiness or further-work decision |
| Notion finalization | After full acceptance: exact task/feature statuses and short accepted specification/plan deltas; penultimate stage |
| Retrospective | Final process revision; up to three supported proposals or none, including optional local knowledge promotion with its existing approvals/checks/separate commit |

Repeat architecture → implementation → audit → review → Play Mode/correction → plan
for subtasks. Then final verification → workflow audit → developer decision →
Notion finalization → retrospective (including optional knowledge promotion) → complete.

## Requirements and Initial Plan

The contract defines behavior/validation, not implementation design. Re-read the
current authoritative source before asking; gaps/conflicts do not become guesses.
Show the full contract, save `requirements.md`, and obtain approval before research/
decomposition.
Research by responsibility/data/integration, not one feature name. Inspect mechanisms,
implementations/callers/contract variants, assets/installers/tests. Negative findings
state search bounds. Plan observable outcomes, reuse/extension, dependencies, ownership,
and narrow checks; each material unknown gets an experiment/decision. Do not divide
only by DTO/interface artifacts or hide scope overlap.
Before approval subtasks are proposals. Afterwards add/remove/split/rename/reorder
only through an explicitly requested approved delta. Bug/review corrections remain
in the same subtask. Requirements changes use the contract, not disguised plan updates.

## Architecture, Implementation, Audit

Architecture includes outcome/scope, exact new/changed/reused files/objects, cross-type
contracts/signatures/serialized shapes, owners/callers/compatibility, execution chain,
Unity/DI/references, relevant patterns, risks/decisions, checks. The lifecycle map is
mandatory when applicable. Do not defer material unknown boundaries to implementation.
After approval change a minimal coherent set, integrate actual Unity references/
installers/config, and run applicable checks. Follow unified initialization/DI;
preserve `.meta`, assemblies, and the ProjectContext exception. Drive the connected
Editor through CLI with explicit project targeting; edit source directly and assets
according to Editor rules. Record unavailable checks; do not silently start a fallback Editor.
Audit separately inspects state/null/errors, ownership/DI, lifecycle/subscriptions/
async/resources, Fusion authority/serialization, bindings, CPU/GC, scope/design
compliance. P0/P1 require correction/new applicable checks/audit. P2 are visible
recommendations, not refactoring authorization. Findings identify exact location,
evidence, consequence, and minimal change.

## Interruptions and Bugs

Feedback/defects create no FEAT. Correct them in the current subtask and approved
scope, retaining existing audit, review and verification gates. Scope/design changes
use the host decision process. Record the reason and relevant evidence in the
current artifact; do not create a bug workflow or additional approval sequence.
Never automatically invoke/load `unity-bugfix`, even for failed tests or Play Mode.
Only a direct developer command naming the skill or starting/resuming its workflow
activates it; do not solicit activation as a prerequisite to ordinary corrections.
If manually requested, reuse the current artifact/case, state and checkpoint.
An explicit fix-and-continue command covers return; preserve accepted history.

## Play Mode and Final Verification

The developer runs the accepted scenario, returning steps/expected/actual/logs on
failure. Network/init-order cases cover executing host/client roles and at least one
spawn/ready/despawn/reuse transition. Diagnose and correct failures within the
current host scope; the bugfix skill requires a separate direct developer command. The final journey includes applicable edge cases, Inspector/assets, and no
new Console errors.
For every accepted RQ/AC record actual implementation, method/scenario/evidence,
and `passed`, `failed`, or `not tested`. Code presence is not a passed check.
Save `final-verification.md` and present it before audit/readiness. Failed/untested
mandatory criteria return to existing FEAT or missing-data requests.
Run `Tools/AI/workflow-audit.ps1 -Feature <feature-kebab-case>`; retain outcome/paths/
state and present findings or explicit absence. The helper checks structure, not
code/requirements/approval correctness. Unavailable/`attention_required` is visible
to the developer and does not authorize automatic workflow policy changes.

## Retrospective and Completion

After acceptance use iteration/scope/review/check/audit facts. Propose up to three
reusable improvements with observation, evidence, exact change/owner/effect.
One-off workarounds or feature mechanics do not become rules without a decision.
Knowledge promotion requires an eligible candidate, canonical target, duplicate/
conflict check, approval, validation, separate commit. Record rejection/no candidates.
New state uses the current CHANGELOG version. Gate/state semantic changes do not
silently migrate active features; translation/editorial edits need no migration.
Before final readiness, requirements/plan/architectures are approved, subtasks passed
review/Play Mode, checks passed or unavailable limits are explicit, all RQ/AC coverage
and audit are retained/presented, limitations are visible, and the developer considers
the final diff. An untested mandatory RQ/AC blocks readiness. Notion finalization
follows this decision and precedes the final retrospective
(with optional local knowledge promotion), then `complete`. These are not prerequisites
of readiness itself. Finalization requires verified effects or explicit not applicable.

## Notion, State, Delegation

Follow [Notion integration](../notion/feature-integration.md): explicitly load
`notion-feature-requirements` only for selected Notion sources at requirements intake,
and `notion-feature-finalization` only after full acceptance before retrospective.
The main agent dispatches isolated workers and owns contract/state/Git; no main-agent
MCP calls, routine source refresh or intermediate publication. Bugfixes and subtasks
never trigger Notion. Keep accepted decisions locally until finalization.
This timing supersedes old active-feature hooks without replaying or migrating state.
Publication failure preserves development; retry finalization without implementation.
Optional research/code-review subagents require authorized delegation. The main agent
verifies findings/owns gates/state; without subagents it performs a separate review.

## Related Documents

- [Feature skill and topic references](../../.agents/skills/unity-feature-development/SKILL.md).
- [Bug diagnosis](bugfix-reference.md).
- [Notion integration](../notion/feature-integration.md).
