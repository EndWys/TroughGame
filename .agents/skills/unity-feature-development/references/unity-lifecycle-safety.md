# Unity Lifecycle Safety

## Purpose

Read for Unity/Fusion callbacks, runtime UI, event-driven initialization, pooling,
RPC, coroutine, and UniTask changes. A method name does not prove service/config/
model/child View readiness. Show readiness dependencies before implementation.

## Dependency Map

Include the map in approved `subtasks/FEAT-XX.md` and the visible architecture
handoff; include only applicable rows.

| Trigger / Owner | Guaranteed State | Dependencies Used | Deferred / Forbidden | Safe Action | Invalidation / Cleanup |
| --- | --- | --- | --- | --- | --- |
| Actual entry | Contract/proven chain | Service/model/View | Unproven readiness | Ready signal/method | Unsubscribe/cancel/reset/guard |

Trace `ApplicationEntryPoint → ApplicationInitializationFlow` → context initializer
→ `FeatureInitializationFlow`, and gameplay entry `ISceneFlowService → IGameSceneLifecycle`.
Installers register only; initialization is awaited after bindings. Scene token
expires after `ExitAsync`, before unload. Unity/Fusion callbacks do not replace
the unified startup flow. Policy owner: `docs/ARCHITECTURE_RULES.md`.

## Readiness Rules

- `Awake`/`OnEnable`/`Start`, `Spawned`/`Despawned`, RPC, and project signals have
  different guarantees; name the actual guarantee and owner.
- Do not read config/model/child View before their initialization completes.
  First derived computation/UI update follows an explicit owner-ready point.
- A ready signal guarantees its named state, not every dependency.
- Pooling/respawn require defined state/subscription reset.
- Delayed task/coroutine/callback after disable/despawn/reuse cancels or checks
  current object/session identity.
- Verify initial state and at least one transition. A late successful update
  does not prove safe baseline initialization.

Show real owners/methods/signals in sequence:
owner ready → config/model ready → View initialization → first calculation →
subsequent subscriptions. If read-only research cannot prove the chain, raise
a blocking technical question or bounded experiment. Names/incidental order are not proof.

## Verification

Compare the final diff with the map: dependencies ready, subscription/cleanup
symmetry, delayed work cannot update invalid/reused objects, baseline and
transitions share a source of truth. For network/init-order changes, Play Mode
covers executing peer roles, host/client where applicable, and a spawn/ready/
despawn/reuse transition. Record actual results and Console evidence.
