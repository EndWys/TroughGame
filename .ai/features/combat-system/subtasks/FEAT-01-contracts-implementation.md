# FEAT-01 Common Contracts — Implementation Record

## Scope and Status

Implemented the main-owned common contracts from accepted architecture revision 7
(APR-003), as explicitly requested by the developer. This is an implementation
slice inside FEAT-01, not acceptance/completion of FEAT-01 or a new subtask.
Package A/B mechanics and actor/Unity integration have not started.

## Production Inventory

- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Enums/CombatActionPhase.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Enums/CombatActionKind.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Enums/CombatShapeType.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Abstract/ICombatActionStateAccessor.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Abstract/ICombatActionSnapshotAccessor.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Abstract/ICombatActionAccessor.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Abstract/ICombatActionMutator.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Abstract/ICombatHurtbox.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Abstract/ICombatTargetQuery.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Models/CombatActionStateModel.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Data/CombatActionTimingData.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Data/CombatActionSnapshotData.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Data/CombatShapeData.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Data/CombatContactData.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Settings/CombatShapeSettings.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Configs/CombatActionProfileConfig.cs`: new contract/data/config type, matching architecture revision 7.
- `Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/DataHolders/Configs/CombatAttackConfig.cs`: new contract/data/config type, matching architecture revision 7.

Changed `Assets/ProjectCore/Contexts/GameCore/Features/Input/Scripts/DataHolders/Models/InputBufferStateModel.cs`: private packed command/lock byte and separate TickTimer; existing ushort/bool member access preserved through computed properties. IDs above 127 throw before mutation. Existing descriptor, controller and interface source files are unchanged.

Extended `Assets/ProjectCore/Contexts/GameCore/Tests/Editor/InputBufferControllerTests.cs`: exhaustive supported ID/lock combinations, invalid-ID preservation, copy/clear/reset regression. These preliminary tests are main-owned work in the file Package A will extend; its future baseline must include them.

Unity imported script/folder metadata. No config asset instances or actor bindings are required for the passive contracts. Profile validation and geometry behavior remain Package A/B responsibilities; passive data constructors do not implement those policies.

## Frozen Contract Manifest

`FEAT-01-contracts.json` records 21 source SHA-256 values: 17 new files, packed model,
and three reused unchanged input contracts/implementation files. Contract revision 1
implements architecture revision 7; these are different version counters.
Future workers must use this exact baseline and return contract changes to main.

## Source Refresh

Read-only isolated worker refreshed Combat and phases/hitbox sources without material
drift. Request: `combat-contracts-source-refresh-20261015`; request SHA-256:
`88d5a6bcd815c8457a5e5c91d2787e1c88d27134f4af769b668dec0ec8c6328a`.
Combat version `2026-10-15T16:47:29.805Z`, normalized source SHA-256
`b32d1d2e0eb1b9aaf1eac231cb4f222f356ea0f65daf049540460242bf0871fb`.
Phases task version `2026-10-12T17:30:47.137Z`, normalized source SHA-256
`9655745b3a5e223c918acac0cab99dfe4881c0aea0e690533e214a9f696172b0`.
No publication or task closure performed; completeness/unknown-block metadata unavailable.

## Executed Validation

| Check | Actual result | Evidence / limitation |
| --- | --- | --- |
| Connected Editor compilation | Passed; no compilation errors | `.ai/evidence/combat-system/contracts-validation/20261015-195355-487-0f2feb0a-Compile/result.json` |
| Connected Editor input-buffer tests | 7 passed, 0 failed/skipped/inconclusive | `.ai/evidence/combat-system/contracts-validation/20261015-195538-763-861c790e-EditMode/result.json` |
| Runtime dotnet build | Passed; 0 errors, 10 warnings in dependency projects/assembly version conflicts | `dotnet build ./ProjectCore.Runtime.csproj --no-restore -v:minimal`; generated project includes all 17 new contract types |
| Strict architecture | Failed on same 4 unrelated Enemy PNG ASSET003 names; 550 scripts, 171 resources, 0 warnings | No new contract diagnostics; baseline remains unresolved |
| Whitespace | `git diff --check` passed | New contract source also checked for trailing whitespace |
| Fusion woven layout | Buffer 8 bytes / 2 words; action model 20 bytes / 5 words | `.ai/evidence/combat-system/contracts-layout.json`; prior live buffer size was 12 bytes. No bandwidth measurement |
| Network reconciliation / gameplay Play Mode | Not run | Required after actor/mechanics integration; packing unit tests do not prove reconciliation |

Initial layout eval attempts timed out around domain reload; final read-only eval
with 20-second operation timeout succeeded. No Editor restart or fallback used.

## Scoped Code Audit

Separately inspected all 17 new files, packed-model diff and test diff against the
approved inventory/signatures/serialized shapes. Checked input/gameplay ID separation,
immutability, interface inheritance, no duplicate state ownership, lock-bit preservation,
invalid-ID atomicity, Fusion struct storage, passive configs and current caller compatibility.
No actionable P0/P1/P2 finding in this slice. This is not the final combined FEAT-01
audit, developer review or Play Mode acceptance.

## Next Action

Prepare isolated A/B baselines from the frozen manifest, then develop the approved
packages and main integration. Keep FEAT-01 at implementation until the combined
changes are validated and audited. No commit is authorized by this slice alone.
