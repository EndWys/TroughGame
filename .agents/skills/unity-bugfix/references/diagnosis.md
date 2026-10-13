# Evidence and Diagnostic Loop

## Purpose

Separate a confirmed cause from a leading hypothesis and collect observations
that actually discriminate competing failure mechanisms.

## Intake and Causal Analysis

Collect available applicable facts; explicitly mark absent version/build/logs.
Expected behavior has a task/specification/decision/developer statement as authority.
Actual behavior is reported or agent-observed: identify the observer and whether
the original scenario ran. For intermittent bugs record frequency, trigger, timing,
and peer role; one successful run does not prove absence.
Trace trigger → owner → state/lifetime → failure. Inspect relevant callers, DI/init,
config/serialized references. Compare code/log/asset evidence and a known-good version
when available. The first exception or expensive Profiler sample need not be causal.
Use existing diagnostics before adding instrumentation.

| Conclusion | Required Basis |
| --- | --- |
| Confirmed cause | Proven mechanism explains conditions/expectation; exact location/causal chain. Failing checks or deterministic static proof are acceptable with honest limits |
| Leading hypothesis | Supported but missing link/alternative remains; production fix not yet authorized |
| Insufficient evidence | Observations do not discriminate; minimal distinguishing probe needed |
| Not reproduced | No original failure in recorded attempts; not equivalent to fixed |

## Hypotheses, Likelihood, Complexity

Show materially distinct hypotheses, evidence for/against, distinguishing observation,
likelihood, and confidence in the estimate. Default to high/medium/low or not estimable.
Numeric ranges require a request or repeated observations, labeled as subjective
estimate or measurement. Overlapping/incomplete causes need not sum to 100%; likelihood
does not prove a hypothesis.
Conditional fix complexity is small/medium/large/unknown based on owners/contracts/
assets/checks. Separate investigation cost; name what could change the estimate.
Do not promise unsupported hours or discard undisproved alternatives.

## Diagnostic Plan and Approval

Order probes by discriminating power, cost, and impact. Each identifies question/H-ID,
exact member/object, method/steps, data, distinguishing outcomes, conditions/peer roles,
effort, overhead, cleanup, and exit criterion. Choose applicable tools:

- Correlated logs: order/identity/state/lifetime/config/owner/role/time; use project logging contracts.
- Profiler: capture interval/scene/role/markers, measurement limits, CPU/GC/physics/network
  hypothesis. Verify capture capability first; source inspection is not measurement.
- Narrow test: controlled inputs and violated observable contract. A proven diagnostic
  test may become a regression test.
- Inspector/prefab/scene: exact field/object, connected Editor, preserve GUID/`.meta`
  and agreed save scope.
- Reproduction matrix: minimal role/state/lifecycle comparisons.

Read-only source/existing-log analysis may continue before approval. New probe runs,
instrumentation, and experiments await approval unless exactly authorized already.
Do not re-approve accepted verification scenarios. For unavailable tools report limits
and exact manual capture steps. Execute available authorized checks yourself; never
start a fallback Editor silently.
After each iteration update facts/hypotheses/likelihood: confirmed → fix proposal;
unresolved → minimal justified probe delta. Exhausted approved attempts/budget or
required input stops investigation. Remove owned temporary probes at their accepted
expiry; explain retained diagnostics in the final diff.

## Fix Options

Present minimal coherent correction and real alternatives. Compare owners/files,
behavior, compatibility, complexity, risks, and checks; recommend from evidence.
A single sound fix needs no invented options. Redesign outside bug scope remains a
separate proposal; agreed expected behavior is preserved.
