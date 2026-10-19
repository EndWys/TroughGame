---
name: unity-refactoring
description: Investigate a TroughGame refactoring problem or assess the developer's proposed plan and agree a concrete solution. Run only when the developer explicitly invokes this skill or starts its refactoring process. Never start automatically from code smells, review findings or feature/UI work.
---

# Manual Refactoring

## Invocation and Scope

Start only when the developer names `$unity-refactoring`, requests this skill, or
explicitly starts its refactoring process. A problem report, proposed plan, review
finding or generic improvement request alone does not invoke it. Continue the same
manually authorized process across feedback/resume without repeated invocation.
Feature, UI and bugfix workflows must not automatically load or dispatch this skill.

The primary result is an agreed refactoring solution. Proposal acceptance alone
does not authorize implementation. Preserve the intended observable behavior and
identify any requested behavior change separately for an explicit developer decision.

## Task

1. Establish the developer's problem, desired improvement, affected area and behavior
   or contracts that must remain unchanged. Treat the explanation as the starting
   point; distinguish the reported symptom, a suggested cause and confirmed evidence.
2. Inspect the relevant implementation, callers, dependencies and current Git changes.
   Include Unity assets, initialization or serialization boundaries when affected.
   Follow AGENTS and applicable project standards. Keep research focused on the
   problem and its consequences rather than surveying unrelated systems.
   If the developer supplied a plan, assess its feasibility, benefit, compatibility
   and missing details through focused inspection. Reuse sound decisions; explain
   concrete faults or necessary corrections instead of automatically endorsing it
   or restarting a broad investigation.
3. Ask focused questions about consequential information inspection cannot establish.
   Continue useful independent research while answers are pending. Resolve uncertain
   goals, ownership, compatibility and tradeoffs before presenting a solution as ready.
   If evidence needs a diagnostic experiment, explain its scope and distinguishing
   outcomes; obtain permission for changes or probes not already authorized.
   Research is sufficient when the mechanism is supported, scope/invariants are clear,
   the proposal is feasible and no unresolved choice blocks its implementation.
4. Choose proportionate transformations using the local [technique guide](references/techniques.md).
   Prefer established technique names and explain their application to this code.
   Several techniques may form one solution. A custom transformation is allowed;
   describe it plainly and do not invent a canonical technique name.
   Name a design pattern only when it helps explain the resulting structure and
   solves a concrete need. Explain its benefit; no separate pattern is a valid result.
5. Present the concrete proposal below and refine it from developer feedback.
   Keep the same problem and proposal rather than creating numbered report files.
   Readiness requires the research criteria above; agreement requires the developer's
   actual decision. Do not infer either from silence or from naming a technique.

## Response Standard

Use the developer's language. For a ready proposal, present these items in order:

1. **Problem.** State the actual mechanism, evidence and development difficulty it
   creates. Name relevant entities or methods; link locations where useful.
2. **Goal and preserved behavior.** Explain the intended structural improvement and
   the observable results, public contracts or data compatibility to preserve.
3. **Techniques and pattern.** Name the selected techniques, where they apply and
   why they fit. Identify any custom transformation. Name and justify the design
   pattern if used, otherwise state that none is needed. When the developer supplied
   a plan, explain whether it fits and which corrections or details are necessary.
4. **Concrete solutions.** Use a numbered list of distinct changes in execution
   order, not a table or continuous block of prose. Give each item a short action
   title and explain the entities/methods/responsibilities/dependencies being changed,
   current → proposed organization or logic, and how it resolves the problem.
   Include a key call chain when it changes and a focused code/pseudocode fragment
   when it clarifies the transformation. Label proposed code and APIs. Keep this
   detail under the relevant numbered item. Explain dependencies on earlier items;
   split substantial work into coherent parts suitable for developer review.
   Do not add a separate execution-order section or invent detail for trivial edits.
5. **Result verification.** State concrete signs that the refactoring goal is met
   and how preservation of behavior will be checked. Select relevant inspections,
   tests, scenarios or measurements; do not prescribe every check for every change.
   Describe planned checks as planned, not as executed results.
6. **Developer decision.** Request acceptance/corrections to the exact solution and
   identify remaining choices if any. Distinguish accepting the proposal from
   authorizing implementation; preserve an implementation instruction already given.

When evidence or answers are still missing, report the established facts and focused
questions/next investigation instead of filling a complete proposal with assumptions.

## Output and Further Work

Deliver the proposal in chat. No state YAML, report, template, journal or script is
required. Create a working document only if the developer requests one; update that
same document on feedback. Temporary tool evidence belongs in ignored `.ai/evidence/`.
Do not invoke feature/bugfix workflows, Notion or subagents as part of this process.
When manually invoked within an active feature, preserve its scope and host gates;
return the proposal to that host without creating another FEAT or workflow state.

If the developer separately authorizes implementation, apply the agreed numbered
changes within scope, preserve unrelated work and run the agreed applicable checks.
Follow project Unity integration rules where relevant. A material change to the
solution returns to the developer before that edit. Report actual changes, achieved
improvement and verification results/limits; obtain the applicable diff/retest
acceptance. This skill grants no automatic commit, push, merge or release permission.
