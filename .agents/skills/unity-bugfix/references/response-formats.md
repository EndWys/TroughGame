# Bugfix Response Forms

## Purpose

Follow the common response standard. All instructions/forms here are English;
render developer-facing prose/headings/table labels in the developer's language.
Preserve identifiers/statuses. Present applicable content in chat; the case retains
full records, not a substitute for approval. Stops cite/link the exact rule briefly.
Do not re-approve unchanged plans.

## Bug Analysis

Show BUG-ID, conclusion (confirmed / leading hypothesis / insufficient evidence /
not reproduced), and next step (fix or diagnosis).

| Expected | Actual | Expectation Authority / Fact |
| --- | --- | --- |
| Verifiable contract | Developer-reported or agent-observed | Task/spec/decision and exact log/run |

| Conditions / Steps | Version / Role / Scene | Actual Outcome |
| --- | --- | --- |
| Minimal reproducer | Actual commit/build/peer/object | Failure or attempt count without reproduction |

| Location | Evidence | Causal Conclusion / Gap |
| --- | --- | --- |
| Absolute file link, Type.Member, asset/object | Relevant code/log/profile excerpt | Mechanism or hypothesis |

A confirmed cause exposes trigger → state/owner → mechanism → failure. Verify line
numbers. Name exact Unity asset/object/component; mark unavailable evidence.

## Diagnostic Plan

Show BUG-ID/revision, requested decision, missing discriminating data, exact actions.

| Hypothesis | Likelihood / Confidence | For / Against | Distinguishing Check | Conditional Fix Complexity |
| --- | --- | --- | --- | --- |
| H-ID/mechanism | High/medium/low/unknown; basis | Facts/gaps | Confirming/weakening result | Small/medium/large/unknown; reason |

| Probe | Exact Changes / Actions | Data / Distinguishing Outcomes | Cost / Impact | Cleanup / Exit |
| --- | --- | --- | --- | --- |
| EXP-ID → H-ID | File/member, log fields, capture/test steps, role | Outcome A/B and meaning | Effort/overhead | Remove probe, run limit, exit criterion |

Name permitted files/objects/conditions and excluded effects. Diagnostic approval
does not permit a production fix. Percentages require evidence/request; incomplete
hypotheses need not sum to 100%. Separate investigation cost from fix complexity.
Profiler specifies metric/conditions; logs specify fields/correlation; tests specify
observable failure.

## Iteration Result

Show BUG-ID/iteration, actual conclusion, next authorized probe/minimal plan delta/
fix proposal.

| Probe / Approved Revision | Actual Run / Evidence | Observation | Hypothesis Impact |
| --- | --- | --- | --- |
| EXP/APR | Command/capture/run | Value/event order | Supports / refutes / does not distinguish; revised estimate |

New probes require visible delta/approval; existing authorization persists. Forced
manual capture includes exact steps/data; do not delegate available agent checks to the developer.

## Fix Proposal

Show BUG-ID/revision, proven cause/evidence, before/after, recommended option/scope.

| Option | Mechanism / Compatibility | Complexity / Risks | Check |
| --- | --- | --- | --- |
| Recommended | Correction and reason | Supported tradeoff | Original/regression |

| File / Object / Member | Exact Change | Cause / Preserved Behavior | Check |
| --- | --- | --- | --- |
| Verified location | Guard/order/subscription/config | Removed mechanism | Observable result |

Options table applies only to real alternatives; one fix still has exact validation.
API shows signatures/callers; lifecycle shows owner/readiness/cleanup; serialization
shows object/property/old/new/reference. Name removed/accepted retained probes.
Scope/design expansion needs host approval or a standalone revision. Then request
selection/approval/changes.

## Implementation and Review

Show BUG-ID/revision, every relevant changed file/object, and actual results,
including the original scenario.

| File / Unity Object | Actual Change | Behavior / Reason |
| --- | --- | --- |
| Every affected element | Fix/regression/probe cleanup | Concrete effect |

| Check | Actual Result / Evidence | Coverage / Limit |
| --- | --- | --- |
| Original reproduction | Passed / failed / not tested | Conditions/role |
| Regression | Fails before / passes after, or comparison not run | Contract |
| Compile/tests/architecture | Command/outcome | Applicable scope |

Audit shows reviewed diff and P0/P1/P2 findings or none. Required findings are fixed
before acceptance. Expose failures/missing checks. Explain absence of useful automated
regression and retain a manual scenario. Ask a concrete accept/change question;
the diff is not accepted beforehand.

## Retest and Delivery

Retest shows BUG-ID and final-acceptance basis:

| Scenario / Steps | Expected | Actual Result / Executor / Evidence |
| --- | --- | --- |
| Original failure | Correct observable behavior | Agent/owner with attribution |
| Affected adjacent/peer/lifecycle case | Preserved behavior | Passed / failed / not tested |

Expose gaps/impact. Manual runs specify preconditions/steps/expectation and
Console/Profiler/log data on failure, with role/executor. Embedded handoffs name
the covered host gate. Failure returns to analysis; acceptance leads to delivery.
The delivery response summarizes cause, corrected behavior, actual commit/checkpoint,
checks, and separate Notion outcome. Embedded results name the interruption and
request an explicit return decision if absent. Commit/publication does not authorize
the next subtask.
