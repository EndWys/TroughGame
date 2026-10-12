# Notion and feature development

The user approved separate Notion subagents and separate state. Use
`notion-project-coordination` in the development agent and the two worker skills
only in dedicated subagents. Read-only code-review delegation remains a separate
mechanism; the Notion worker may perform scoped remote writes through this
contract without permission to change feature state or implementation.

| Development point | Operation and immutable values supplied |
| --- | --- |
| New feature from a Notion task/page or "next ready task" | prepare_feature_context: exact IDs or verified selection rule |
| Requirements approval | Developer stores returned source snapshot in requirements.md; approved contract can be published as values |
| Resume / before implementation of a source-dependent subtask | refresh_source_context: prior source observations; inspect material changes before using stale requirements |
| Accepted plan, architecture, review result, blocker or Play Mode result | publish_progress: approved delta, affected task IDs, actual evidence and desired mapped status |
| Accepted facts alter documentation | update_documentation: concrete delta, canonical target and evidence |
| Explicit final readiness acceptance | finalize_feature: full accepted scope, required AC/checks/Play Mode, exact tasks, business feature status, commits and limitations |
| Retrospective promotes a reusable rule | update_documentation only for the exact separately approved rule and canonical Notion target |

Routine updates happen at meaningful transitions, not for every tool call. A
review-ready task can be `Review`; a task becomes `Done` only after its required
acceptance. Feature business status is selected from verified existing values.
Publish documentation/results before feature completion properties. Include
unexpected accepted facts and explicitly deferred work in the delivery summary.

The original requirements/plan/local-architecture/review/Play Mode/final-readiness
gates remain unchanged. Notion `Ready`, a search result, or an intake packet is
not an approval. The developer agent prepares a reviewable gate result and
records the human decision in its own state; the worker consumes its authorized
value later. Existing authorization is not requested again unnecessarily.

The developer may create its own source snapshot and approved contract under
`.ai/features/`, using values returned in the packet. It never gives the worker
that directory as input. No Notion retry status or journal fields are added to
feature state; workers never advance feature stages. Journals are independently
owned and ignored by Git. Their operation IDs are for message correlation, not
shared mutable variables.

If publication fails, preserve completed development facts. Report local
completion and Notion publication separately; do not claim documentation/task
statuses are synchronized. Resume publication with the same operation ID via
the dispatcher and the worker's private receipt. A fresh session may ask a
worker to reconcile outstanding operations for the specified feature; only
that worker reads the private journal. Do not replay implementation to repair
a documentation failure. Required source access/approval gaps still block the
dependent development stage; independent preparation can proceed.

For a standalone documentation request, dispatch directly without creating a
feature folder or state. If subagents are unavailable, report that restriction;
the main agent never silently takes over Notion MCP actions.

Current canonical ownership: Notion contains product requirements, plans and
documentation; repository `ARCHITECTURE_RULES.md`, `CONVENTIONS.md` and
`SCENE_HIERARCHY_STANDARD.md` retain their current authority for code/assets.
Project Notion documentation standards govern page presentation. A conflict
affecting behavior/policy needs an explicit decision; migration to Notion-owned
architecture standards is not implicit in this integration.
