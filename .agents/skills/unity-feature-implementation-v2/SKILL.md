---
name: unity-feature-implementation-v2
description: Implement one agreed TroughGame subtask from its accepted architecture and report actual changes and technical checks. Run only when explicitly dispatched at the new workflow's implementation stage or directly requested. Do not redesign, start review/acceptance or create workflow reports.
---

# Feature Implementation V2

## Entry and Inputs

Run at `implementation` with permission to enter this stage, or on a direct developer
request to use this skill. Inputs are the feature ID, selected `FEAT-*`, accepted
`requirements.md`, `feature-plan.md`, this subtask's accepted `architecture.md` and
relevant developer decisions. Missing accepted scope/design or unresolved material
choices block dependent edits; identify the gap instead of guessing a solution.

## Task

1. Inspect the relevant current implementation and Git changes before editing.
   Identify the work belonging to this subtask and preserve unrelated/staged user
   changes. Resolve overlapping edits that cannot safely be distinguished before
   changing them. Do not regenerate the plan or architecture on entry.
2. **Code implementation:** implement the agreed logic, data and contracts, including
   DI registrations, initialization and lifecycle handling where required. Follow
   AGENTS, architecture rules and conventions, and applicable technical standards.
   Across code, assets and UI, routine details within the accepted design need no
   additional approval. Do not add neighboring features or refactor unrelated systems.
   Changes to agreed ownership, contracts, behavior or scope must return to the
   coordinator for a developer decision before the affected edit.
3. **Assets and Unity integration:** create or update the agreed configuration assets,
   prefabs and scene objects. Set required components, hierarchy, serialized values
   and references so the code is connected to the actual project. Follow the accepted
   placement and scene standards; omit work the subtask does not require. Use the
   installed Unity CLI skill for Editor operations with explicit project targeting.
   Preserve asset GUIDs and `.meta` files. Do not start a fallback Editor. Missing
   Editor access blocks dependent asset operations, not independent authorized code work.
4. **UI integration:** when the subtask includes runtime UI, use the project skill
   [unity-ui-toolkit](../unity-ui-toolkit/SKILL.md) for its UI implementation and
   required UI-specific checks/approvals. Keep that work within the accepted
   architecture and current subtask; the coordinator still owns workflow transitions.
5. Run the applicable technical checks below. Fix failures caused by this change
   within the accepted solution and repeat affected checks. A failed assumption or
   material design change must be reported rather than hidden behind a speculative
   patch. Ordinary corrections remain in this subtask; do not invoke `unity-bugfix`.
6. Inspect the resulting diff against the accepted architecture and task boundary.
   Remove owned temporary diagnostics or generated files that are not part of the
   agreed result. Report the actual implementation and remaining issues in chat.
   This scope check is not the separate review stage or developer diff acceptance.

## Technical Checks

Run relevant AGENTS checks: strict architecture validation, the project runtime
`dotnet build`, and `git diff --check`. After runtime/source/asset changes, use
`Tools/AI/validate-unity.ps1` against the connected project Editor for compilation
and tests relevant to the changed behavior. Choose EditMode for logic without a
gameplay scenario and PlayMode for Unity runtime behavior; use filters to limit scope.
Add regression tests where they meaningfully protect the change.

Keep actual check outcomes available to the coordinator for its compact state.
In the developer response, report failures or missing required checks that prevent
completion; omit routine successful check summaries and reminders about later stages.
Automated tests do not replace the developer's required Play Mode/retest acceptance.

## Response Standard

Use the developer's language and the following section order. Describe the actual
implementation rather than copying the architectural proposal. The developer must
understand the result from the response; file links provide access to its details.

1. **Implementation brief.** In a short paragraph, identify the subtask, the new or
   changed functionality and how it works at a high level. Focus on the delivered
   behavior. Leave entity inventories and technical explanations to later sections.
   Do not include build/test statuses or reminders about later verification here.
2. **Important implementation details.** Explain the main mechanisms in short,
   named paragraphs or bullets. For each, describe what was added or changed, what
   triggers it, how it processes data/state and what result it produces. Explain
   consequential interactions, conditions and dependencies. This section develops
   the brief into an understandable account of the logic; it is not a file list.
3. **Call chains.** Give each distinct key runtime flow a short descriptive name,
   followed by an ordered list from trigger/entry point to result. Use this format
   for each item: `Object/Class.Method()` — its concrete role/action at that point.
   Name real methods from the implemented code; distinguish callbacks/events or
   framework transitions when the next step is not a direct method call. Keep the
   important flow visible without enumerating every internal helper.
4. **Objects and scripts.** Use a table with columns: entity name, file reference,
   state (`new`, `changed`, `reused`, or `removed` where applicable), role/change,
   and usage. Explain what a new entity is, what it owns or does, what changed in
   an existing entity and where/how it participates in this implementation. Include
   every created/changed script and relevant object; include reused entities that
   explain integration. Split scripts and Unity objects into separate tables when
   both are present. Use clickable file links and an object name for scene/prefab
   instances. Do not duplicate asset inventories from the next section.
5. **Unity integration.** For each new/changed prefab or scene hierarchy, show a
   compact indented tree of relevant GameObjects and attached components. Mark new
   objects and added/changed components; include unchanged parents only as context.
   Explain important serialized assignments as owner.component.field → target,
   so the developer can see how the implementation is connected. Then provide two
   separate asset tables, one for created assets and one for changed assets, using
   columns: asset name/link, asset type, purpose and where it is used. Include
   prefabs/configuration/UI resources as applicable; omit empty tables. Describe
   only actual modifications, not a complete dump of an untouched scene or prefab.
6. **Key implemented code.** Select the fragments that best demonstrate the core
   behavior or a consequential implementation decision. For each, provide a short
   title, clickable source file link with the actual line, a focused code block and
   a plain explanation of what the fragment does and why it matters. Quote current
   implemented code, not pseudocode or the earlier design sketch. Include enough
   context to understand the mechanism without pasting complete classes or the diff.
7. **Blocking or remaining issues.** Include this section only for unresolved errors,
   incomplete integration, unavailable mandatory checks or design decisions that
   prevent completing the stage. State each issue's impact and needed action, with
   command/error details when useful. Do not include routine successful checks or
   notices that later workflow stages have not yet taken place.

Omit inapplicable sections and empty tables. Keep each fact in its relevant section;
use the same entity names across explanations, chains, tables and code fragments.

Return the result to the coordinator without prescribing or starting another stage.

## Output and Completion

The output is the scoped code/asset/test diff and actual technical check results.
No implementation MD template or report is needed. Do not edit requirements, plan,
architecture, decisions, state or other subtasks; report necessary revisions to the
coordinator. Tool-generated temporary evidence belongs in ignored `.ai/evidence/`.
No Notion calls, subagents, commits, push, merge or release belong to this stage.

Report implementation ready only when the approved integration is complete and
applicable required technical checks pass. Otherwise return what is implemented,
the exact failed/unavailable work and its impact, without claiming completion.
The coordinator records the outcome and obtains permission for any stage transition.
An implementation handoff alone does not accept the diff, pass verification or
complete the subtask/feature.
