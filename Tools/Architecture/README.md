# Architecture Validator

`Validate-Architecture.ps1` checks TroughGame source layout and architectural
conventions without loading Unity or requiring external dependencies.

## Usage

Report all migration violations without failing the command:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -Mode Report
```

Write a machine-readable report:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -Mode Report `
  -SummaryOnly `
  -OutputPath Logs\ArchitectureValidation.json
```

Fail when an Error remains:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -Mode Strict `
  -SummaryOnly
```

Validate the checked-in positive regression fixture:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Validate-Architecture.ps1 `
  -ProjectRoot .\Tools\Architecture\Fixtures\Valid `
  -Mode Strict
```

The valid fixture must report `0 errors, 0 warnings`.

## Diagnostic Families

| Prefix | Area |
| --- | --- |
| `PATH` | Source ownership and legacy roots |
| `NS` | Context and Domain namespaces |
| `DOMAIN` | Forbidden Domain dependencies |
| `FEATURE` | Modules, Implementations, Infrastructure, and Bridges ownership |
| `SCRIPT` | Closed feature `Scripts` taxonomy |
| `CONTEXT` | Shared context `Scripts/<Category>` |
| `SUFFIX` | Folder and type suffix correspondence |
| `TYPE` | File/type correspondence and top-level type count |
| `ABSTRACT`, `ENUM`, `STATIC` | Special type category rules |
| `MANAGER`, `VIEW`, `DI` | DI-managed and MonoBehaviour placement |
| `NAME` | Known spelling and legacy suffix rules |
| `INIT` | Initialization callback and `async void` checks |
| `TEST` | Flat Editor and Play test placement |

Errors are deterministic convention violations and fail Strict mode. Warnings
are heuristic findings that require manual review.

`TEST001` requires every test fixture to live directly in `Tests/Editor` or
`Tests/Play`. Other environment names and nested test-category folders are
invalid.

## Initial Migration Baseline

Baseline captured on 2026-07-22 against 131 first-party C# files:

```text
267 errors
4 warnings
```

| Rule | Count | Meaning at baseline |
| --- | ---: | --- |
| `NS001` | 107 | Legacy or incorrect namespaces |
| `FEATURE001` | 102 | Features do not yet have a Modules, Implementations, Infrastructure, or Bridges owner |
| `NAME004` | 19 | Legacy `Changer` suffixes |
| `PATH001` | 14 | Domain remains under legacy `Assets/Domain` |
| `PATH002` | 10 | Project-owned scripts remain outside Contexts or ProjectCore/Domain |
| `NAME001` | 9 | `Proyotype` or `Enteties` spelling in paths |
| `SUFFIX001` | 3 | Context script folder/suffix mismatch |
| `TYPE003` | 2 | More than one top-level type in one file |
| `INIT002` | 1 | `async void Start` in the legacy feature installer |
| `CONTEXT002` | 2 | Undocumented shared context category |
| `INIT001` | 2 | Lifecycle callbacks appear to start initialization chains |

The baseline is historical and is not an allowlist. Report mode must decrease
as migration proceeds; new diagnostics must not be accepted merely because the
project is not strict yet.

Context-owned scripts use the documented non-feature categories such as
`Abstract`, `Enums`, `Extensions`, `DataHolders`, `Other`, and `Views`. Outside
`Init`, these scripts must not participate in DI. A complete capability,
stateful service, or lifecycle participant remains feature-owned.

## Static Analysis Limits

- MonoBehaviour inheritance is resolved from project-owned types and the known
  Unity/Fusion roots. An external custom base type may require manual review.
- Manager `AsSingle`, open-generic `AsCached`, and BaseFeature binding discovery is heuristic and
  produces a Warning when the binding cannot be found statically.
- Lifecycle callback inspection intentionally produces Warnings because local
  component setup may be valid.
- The validator does not replace Unity compilation, Test Runner, scene loading,
  prefab validation, or architecture review.
