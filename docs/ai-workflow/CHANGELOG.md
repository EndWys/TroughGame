# Workflow Version History

## Manual Refactoring — 2026-10-19

- Added explicit-only `unity-refactoring` for problem investigation or assessment of
  a developer-provided plan, ending in an agreed numbered refactoring proposal.
- Technique selection uses a local guide; implementation requires explicit permission.
  No automatic feature/UI/bugfix invocation or additional workflow state is introduced.

## V2 Activation — 2026-10-19

- The accepted `unity-feature-development-v2` pack is now the only feature workflow.
  Its skills, stage order, response formats and templates remain unchanged.
- Removed the legacy aggregator/references, shared feature templates, workflow
  description, porting report and original DOCX source.
- Removed the legacy state audit/evidence-cleanup scripts and their regression cases.
  Connected-Editor validation and its success/failure checks remain.
- Updated documentation, UI and Notion references to the accepted V2 entry and paths.

The entries below are historical changes, not instructions for the active workflow.

## 1.7-tg.1 / Manual Bugfix 2.0 — 2026-10-17

- Feature workflow entry is manual-only: explicit `unity-feature-development`
  invocation starts it; the same feature then continues through the recorded flow
  without repeated invocation. Generic requests and UI skills cannot auto-start it.
  `unity-ui-toolkit` retains automatic selection for relevant UI work. No existing
  feature state or accepted gate is migrated by this invocation-policy update.
- Disabled implicit `unity-bugfix` invocation. Only a direct developer command
  naming the skill or starting/resuming its workflow activates it.
- Removed automatic feature/UI/audit/failed-test handoffs, bug-state creation and
  imported bugfix gates. Ordinary corrections use the current subtask and host gates.
- Replaced multi-reference bugfix instructions with one short process: analyze,
  agree diagnostics/fix, implement, verify/review/accept, deliver. No mandatory
  tables, separate audit reports or new bug YAML. Existing records remain intact.
- Explicit manual authorization persists across turns. A fix-and-continue command
  covers return to the host. Existing checks, acceptance and scoped commit limits remain.
- Manual-only timing applies immediately to active work without state migration;
  earlier automatic invocation alone is not authorization. New feature templates
  and audit tools use 1.7-tg.1; no active feature/bug state is rewritten.

## 1.6-tg.1 — 2026-10-17

- Replaced three broad Notion skills with two explicit-only skills: feature
  requirements intake and whole-feature finalization. Removed coordinator,
  intermediate publication/refresh/bugfix operations and mandatory journal tooling.
- Added `notion_finalization` after whole-feature final readiness acceptance,
  immediately before final retrospective/process revision. Knowledge promotion
  remains an optional local retrospective substep with unchanged approval/check/commit rules.
- Finalization updates exact tasks/feature statuses and brief accepted specification/
  plan deltas only. No workflow journals, audits, tests or bug iterations in Notion.
- The developer explicitly applies the timing restriction to active features now;
  preserve their state/version, history and accepted gates. No replay or auto-migration.
- New templates/audit tools use 1.6-tg.1. No live Notion or Unity changes are part of
  this tooling update; historical receipts remain available.

## 1.5-tg.1 — 2026-10-14

- Added unity-ui-toolkit for runtime screen/popup/widget design, resources,
  responsive layout, MVVM, data/input binding and dynamic collections.
- Runtime UI uses MVVM by default; MVP requires a concrete benefit and acceptance
  through the existing architecture gate. Each presentation unit has one state/
  action owner, with common DI/navigation/lifecycle and verification rules.
- Per-view Other/ViewModels and Other/Presenters naming and validation support.
- Root common UI owner is `Assets/ProjectCore/Assets`, grouped by resource type;
  context-private UI is not reused directly across concrete contexts. Defined
  search/promotion/change rules and font naming/folder checks.
- Stage-specific UI response forms describe hierarchy, architecture/resources,
  exact changes, evidence and next actions in the developer's request language.
- UI specialization reuses host state/approvals/checkpoint; Notion stays isolated.
  Consolidated UI architecture/resource approval extends the existing host gate.
- New UI adds visual prototype acceptance before complete logic. This acceptance
  does not replace final diff review/Play Mode. Targeted bugs keep v1.0 bugfix gates.

New feature states use 1.5-tg.1. Older active states require an explicit gate
compatibility decision before adopting the prototype step; no automatic migration.

## 1.4-tg.1 — 2026-10-12

- Added unity-bugfix v1.0: causal analysis, likelihood/complexity estimates, approved
  diagnostic probes, and exact fix options.
- Standard response forms and separate diagnostic/fix/review/retest acceptance gates.
  Failed retest returns to analysis; existing authorization persists.
- Embedded bugs reuse host FEAT/interruption/state/checkpoint; standalone bugs have
  an independent template.
- Notion adds `prepare_bug_context`/`finalize_bugfix` for exact accepted bug tasks/
  documents without completing the whole feature.
- Documentation follows a common structure. All agent instructions, including
  workflow references, discovery maps, and template instructions, use English.
  Responses follow the developer's request language; Notion content follows its
  live project standard. Editorial changes do not alter gates or migrate state.
- One-off installation reports/historical sources are outside the active rule set.

New feature states use 1.4-tg.1. Introducing embedded diagnostic gates into an older
active feature requires an explicit decision. See [bugfix-reference.md](bugfix-reference.md).

## 1.3-tg.1 — 2026-10-12

- Added separate Notion coordination/intake/publication skills.
- Bound actual task/feature/document/idea IDs and schemas; use existing Notion
  documentation standard v1.2.
- Immutable requests/results and private journals without shared development state.
- Source refresh, accepted-fact publication, and verified closure preserve approval semantics.
- Offline helper/tests check retry identity, bounds, and partial outcomes; they do
  not prove remote writes.

Active 1.2-tg.1 states are not migrated automatically; introducing Notion sources/
publication requires an explicit decision.

## 1.2-tg.1 — 2026-10-12

Initial adaptation of the supplied workflow v1.2 for TroughGame.

- Preserved stages/gates, developer Play Mode, requirements traceability, audit,
  retrospective, and knowledge promotion.
- Selected automatic accepted-subtask commits after review and passed Play Mode;
  this does not authorize tooling setup commits.
- Uses ARCHITECTURE_RULES/CONVENTIONS, scene standard, actual project map,
  Tools/AI, and TROUGHGAME_UNITY_CLI_PATH.
- Historical DOCX never granted authority; active rules are maintained in Markdown.
- Connected-Editor validation accounts for rejected commands, string/object responses,
  explicit project targeting, and inconclusive tests. Cleanup checks bounds and dry-run.
- Audit is structural; subagent inspection is optional with authorized delegation.

Discuss changes to gate/state semantics explicitly; editorial changes require no migration.
