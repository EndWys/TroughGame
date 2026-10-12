# Unity Lifecycle Safety Reference

## Purpose

Use this reference only for a feature subtask that changes or relies on Unity/Fusion lifecycle code, runtime UI initialization, event-driven initialization, pooling, RPCs, coroutines or `UniTask`.

The purpose is to make readiness dependencies explicit before implementation. Lifecycle method names communicate object lifetime; they do not, by themselves, prove that a project service, configuration, model or child view is initialized.

## Required lifecycle dependency map

Include this map in the local architecture plan whenever this reference applies. Add only the rows relevant to the subtask.

The map belongs in the approved `subtasks/FEAT-XX.md` artifact, not only in a chat response.

| Trigger and owner | State guaranteed by evidence | Dependencies used | Dependencies deferred or forbidden | Safe next action | Invalidation / cleanup |
| --- | --- | --- | --- | --- | --- |
| `<entry point>` | `<contract-backed state>` | `<service/model/view>` | `<unproven dependency>` | `<ready signal or method>` | `<unsubscribe/cancel/reset/guard>` |

Evidence means a documented contract or the demonstrated initialization path in the project. Call order inferred from a name, current play-through or a nearby component is not sufficient evidence.

## Readiness rules

For TroughGame, trace `ApplicationEntryPoint -> ApplicationInitializationFlow ->
context initializer -> FeatureInitializationFlow`, and gameplay entry through
`ISceneFlowService -> IGameSceneLifecycle`. Installers register only; feature
initialization is explicitly awaited after all bindings. Scene tokens expire
after `ExitAsync` and before unloading. Local Unity callbacks and Fusion
callbacks must not become alternate application/feature entry points. Consult
`docs/ARCHITECTURE_RULES.md` for the authoritative contracts.

- `Awake`, `OnEnable`, `Start`, Fusion `Spawned`/`Despawned`, RPC handlers and project-specific signals have different guarantees. Record the guarantee actually established by the owning code.
- Do not read a service configuration, model data or child view state in an entry point unless its initialization is proven complete there.
- Put initial calculations and first UI updates behind an explicit owner-defined ready point when the relevant data or view is initialized later.
- A project-specific ready signal is safe only for the state named by its contract. It does not implicitly make unrelated dependencies ready.
- When a view may be pooled or a network object may despawn and respawn, define reset behavior for derived UI state and event subscriptions.
- For delayed `UniTask`, coroutine and callback work, define whether it remains valid after disable, despawn or reuse. Cancel it or guard it with the current object/session identity when it can become stale.
- Verify both the initial state and at least one transition. A correct update after a later event does not prove that the baseline state was initialized safely.

## Architecture output

After the map, state the initialization chain in execution order. For example:

```text
<owner ready signal>
  -> <model/config ready>
  -> <view initialization>
  -> <first derived calculation>
  -> <subscriptions for later updates>
```

Name the concrete owners, methods and signals. If the chain cannot be proven with read-only research, list it as a blocking unknown and request a technical decision or a narrowly scoped spike.

## Review and Play Mode checks

Review the final diff against the approved map:

- each changed entry point uses only its proven dependencies;
- readiness subscriptions and cleanup are symmetric;
- delayed work cannot update an invalid or reused object;
- baseline and transition updates share a consistent source of truth.

For networked or initialization-order changes, the Play Mode case covers the peer roles that execute the changed path (host and client when applicable) and one affected spawn, ready-state, despawn or reuse transition. Record Console output and the observed result.
