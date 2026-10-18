# UI Verification and Audit

## Purpose

Gather visual and functional evidence appropriate to the approved UI change.

## Execution

1. Inspect the complete relevant diff for ownership, API/lifetime, binding modes,
   service authority, subscriptions, async cancellation, recycling and asset refs.
   Use the host's audit severity/required-finding rules. Independent inspection
   does not imply a subagent; delegation requires existing authorization.
   Compare the actual implementation with the accepted MVVM/MVP choice. Confirm
   one state/action owner per presentation unit, domain authority and owned cleanup.
   For MVP check its View contract and explicit update/event flow; native binding
   tests apply only where actual bindings exist.
2. Run relevant project checks: strict architecture validation, C# build when
   applicable, `git diff --check`; use `Tools/AI/validate-unity.ps1` for connected
   Editor compilation/tests after runtime edits. Use installed CLI preflight,
   explicit project target and required caller/skill flags. Do not start a fallback
   Editor silently or hand-edit serialized scenes/assets to bypass Editor tooling.
3. Execute available render/runtime inspection in the actual host. Inspect Console
   and binding errors; verify catalog/definition/UXML/USS/PanelSettings references.
   Check resource ownership: no direct UI reference to another concrete context,
   and no root CommonUI dependency on private game-feature assets. For shared
   resource changes inspect actual consumers and verify their affected behavior.
   Source parsing, compilation or a UI Builder preview alone does not prove input,
   navigation or runtime binding works.
4. Verify the approved resolution/content matrix and capture actual renders with
   screen/panel size, scale policy, data and revision. Compare to the approved
   source; include concrete differences and accepted deviations.
5. Exercise changed commands/drafts/validation, busy/error/retry, input/focus,
   modal blocker, close/reopen, hidden history and applicable scene transitions.
   Collection changes include recycling and late-result checks.
6. For defects retest the original exact case and adjacent affected behavior. A
   regression test must demonstrate the violated contract; prefer failing before
   and passing after. Add no tests that simply mirror layout implementation.

Execute available checks yourself and request only genuinely necessary manual
actions. An unavailable Editor/capture/profiler is a named limitation, not a pass.
Keep temporary captures/logs in ignored `.ai/evidence/`, with durable scenario and
result references in the host artifact. Do not add one-off installation reports.

## Completion and Return

Visual prototype acceptance permits subsequent functional work; it never substitutes
for final diff review or required Play Mode/retest acceptance. Failed verification
returns to correction at the same host task; no automatic bugfix-skill invocation. Material design/scope
changes return to the applicable approval before dependent edits.

Return exact changed paths, observed checks, accepted decisions and remaining work
to the host. Preserve .meta and unrelated staged changes. This skill grants no
standalone automatic commit/push or Notion invocation. Keep UI-stage results local;
distinguish owner acceptance from checks actually executed.
