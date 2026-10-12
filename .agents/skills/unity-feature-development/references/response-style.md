# Unity Feature Development Response Style

## Purpose and priority

This reference makes workflow responses easy for a developer to scan, approve and validate. It controls presentation only; workflow stages and approval semantics belong to `docs/ai-workflow/workflow-reference.md`.

The developer's explicit request for response format takes priority over this reference. Use the smallest structure that makes the current result, evidence and required action clear.

## Core response model

Every response should answer, in this order when applicable:

1. What is the current outcome or required developer action?
2. What concrete facts or evidence support it?
3. What happens next, and who must act?

Lead with the answer. Do not open with a greeting, task restatement or chronological narration. Use short sections, lists and tables to separate independent information; use prose only where it explains a relationship or consequence better than a structured form.

## Attention signals

Reserve high-visibility formatting for information that changes what the developer needs to do.

| Situation | Presentation |
| --- | --- |
| A decision is needed to continue | `## DEVELOPER DECISION REQUIRED`, followed immediately by `**Question:** ...` |
| Work cannot continue | `## BLOCKED`, followed by the missing input/evidence and why it blocks progress |
| Approved scope must expand | `## SCOPE EXPANSION REQUIRED`, followed by the exact additional file, system or Unity object |
| Actionable risk | Start the item with `**WARNING:**` |
| Required manual step | Start the item with `**Developer action:**` |
| Unknown or unavailable evidence | Start the item with `**Unknown:**` or `**Evidence missing:**` |
| Status | Use **Passed**, **Failed**, or **Not tested** in the relevant lead sentence or table cell |

Do not use all caps, bold text or warnings decoratively. Do not use more than one attention heading in a response unless there are genuinely independent blockers.

## Structure and density

- One bullet represents one fact, decision, risk, question or action.
- Name concrete files, types, methods, Unity objects, commands, tests and evidence paths where that makes a claim reviewable.
- Use a table when the reader must compare repeated fields: options, scope, subtasks, findings, requirements or validation status.
- Do not use a table for a one-line result, a single ordered procedure or explanatory prose.
- Omit empty sections, repeated context and inapplicable template fields.
- Keep routine success reports brief. Add detail for an approval gate, blocker, risk, scope change or failed validation.
- Clearly label facts, inferences and proposals. Never present an assumption or unavailable evidence as confirmed.

## Questions and gates

Never hide a required question at the end of a long analysis. Put it first and make it answerable without reconstructing the full context.

For a decision with meaningful alternatives, use a compact table:

```markdown
## DEVELOPER DECISION REQUIRED

**Question:** Approve the proposed subtask architecture?

| Option | Effect |
| --- | --- |
| Approve | Implementation starts within the listed scope. |
| Request changes | The architecture is revised before any project changes. |

### Basis

- `Assets/ProjectCore/Contexts/TechnicalPrototype/Scripts/Init/TechnicalPrototypeContextInstaller.cs` is the registration point when that context owns the feature.
- The proposed change remains inside approved subtask `DR-03`.
```

- Ask only questions that materially affect behavior, architecture, scope, verification or authorization.
- Number independent questions as `Q1`, `Q2`; state dependencies between them.
- If there is no valid alternative, state the exact approval or missing information required instead of inventing options.
- Never treat silence, agreement with an unrelated point or a generic positive response as approval.

## Stage-aware content

Choose content based on the current workflow stage. Do not force a stage-specific heading or a complete template when a smaller form is clearer.

| Current stage | Include what the developer needs to assess |
| --- | --- |
| Requirements | Read `requirements-contract.md` for required facts. Do not discuss implementation design; a blocking question identifies the authoritative source already checked. |
| Research and decomposition | Read `project-research.md` for required facts. Do not dump search logs, create plan items before approval, or show empty concern rows. |
| Local architecture | Read `stage-deliverables.md` for required facts. Use the approval-question layout and concise execution chain. |
| Implementation | Read `stage-deliverables.md` for required facts. Use a material-area review table and a compact validation result. |
| AI audit | Read `ai-code-audit.md` for required facts. Use its findings table or explicit no-actionable-findings result. |
| Developer review handoff | Diff areas to review, validation already completed and the explicit accept/change decision. |
| Play Mode handoff | Reproducible preconditions, steps, expected result and failure evidence to return. |
| Debug | Use the Bug diagnosis handoff in `stage-deliverables.md`: expected versus actual result, concrete location/evidence, falsifiable conclusion and smallest correction or targeted diagnostic. |
| Plan update | Use the Plan change handoff in `stage-deliverables.md` only after an explicit developer request. Otherwise state `plan unchanged` briefly. |
| Final verification | Use the Final verification handoff in `stage-deliverables.md`. Keep requirement traceability and known gaps visible. |
| Workflow audit | Use the Workflow audit handoff in `stage-deliverables.md`. Keep process evidence and actionable findings separate from code review. |
| Retrospective | Use the Retrospective handoff in `stage-deliverables.md`. Show at most three evidence-based reusable improvements or an explicit no-actionable-improvements result. |
| Knowledge promotion | Use the Knowledge promotion handoff in `stage-deliverables.md`. Keep factual evidence, one canonical target and the explicit approve/reject/change decision visible. |

## Useful response patterns

### Small successful implementation

```markdown
## IMPLEMENTATION COMPLETE

- **Result:** `DailyRewardProgressService` now rejects a second claim on the same local date.
- **Changed:** `Assets/.../DailyRewardProgressService.cs` and its focused test.
- **Validation:** **Passed** — `<actual command>` (exit code `0`).
- **Next:** AI code audit.
```

### Architecture approval handoff

```markdown
## DEVELOPER DECISION REQUIRED

**Question:** Approve the local architecture for `FEAT-02`?

### Behavior change

- **Before:** `AIGoalSelector.Select(...)` receives goals in discovery order and selects the first valid one.
- **After:** `AIScoreCalculator.CalculateGoalScore(...)` assigns each valid goal a score; `AIGoalSelector.Select(...)` chooses the highest-scoring goal before `BotActionExecutor.Execute(...)` performs it.

### Execution chain

`BotTurnSystem.HandleTurn()` → `AIScoreCalculator.CalculateGoalScore(...)` → `AIGoalSelector.Select(...)` → `BotActionExecutor.Execute(...)` → bot action in the match.

The complete file/API inventory and validation plan are in `subtasks/FEAT-02.md`.
```

Use actual discovered names only; this example does not authorize inventing similar types.

### Scope expansion

Use the Scope expansion or architecture revision handoff in `stage-deliverables.md`. Present the discovered evidence and smallest requested delta before asking for approval.

```markdown
## SCOPE EXPANSION REQUIRED

**Requested addition:** `Assets/.../ProjectContext.prefab`.

| Approved scope | Additional scope | Why required | If deferred |
| --- | --- | --- | --- |
| Daily Rewards feature files | Project DI registration | The approved service cannot be resolved otherwise | The subtask cannot initialize |

**Decision:** Approve expansion / revise architecture / defer integration.
```

### AI code audit

```markdown
## AI CODE AUDIT

| Priority | Location | Finding | Consequence | Recommended correction |
| --- | --- | --- | --- | --- |
| P1 | `RewardView.OnEnable` | Signal subscription is not removed | Reopened view receives duplicate updates | Unsubscribe in `OnDisable` |
```

If there are no findings, state `**Result:** No actionable findings.` and identify the reviewed diff/scope.

### Play Mode verification

```markdown
## PLAY MODE VERIFICATION REQUIRED

1. Start through `Scene_Preloader` with the scenario's required player state.
2. Open Daily Rewards and press **Claim** twice.
3. **Expected:** one reward is granted; the second press is unavailable; Console has no new errors.
4. **Developer action on failure:** return steps, expected/actual result, Console log and a screenshot of the reward state.
```

## Evidence and status

- State an exact command only when it was run; include its result when relevant.
- Use clickable absolute paths for local file links in Codex; repository-relative paths remain suitable inside workflow artifacts.
- Identify the actual test, Play Mode scenario, log, Inspector result or state snapshot that supports a claim.
- `Not run`, `not available`, `Not tested` and `Failed` are distinct statuses; use the precise one.
- Do not claim behavior passed solely because implementation code exists.
- Do not paste complete logs or diffs. Quote the smallest relevant excerpt and point to its source.

## Avoid

- Long prose that mixes status, questions and risks.
- Repeating the same changed files, requirements or validation in several sections.
- “Looks good”, “should work” or “done” without concrete evidence.
- Boilerplate headings and empty tables.
- Style-only audit findings that do not affect correctness, safety or maintainability.
- Decorative bold, all caps, warnings or nested lists.
