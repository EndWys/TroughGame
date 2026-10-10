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

Run the inheritance/type-name regression checks:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Test-TypeResolution.ps1
```

The runner checks exact diagnostic rules, file paths, and Strict exit codes.
It includes the positive fixture and `Fixtures/TypeNamesInvalid`, where a plain
class in Views must produce `VIEW001` even when another context has a valid
MonoBehaviour with the same name. Other cases cover local-name precedence,
imports, qualified names, aliases, generic arity, multiline generic bases,
ambiguous imports, framework namesakes, Managers, context-owned Views, and cycles.
Generated fixtures are created in a temporary directory and removed after the run.

## Diagnostic Families

| Prefix | Area |
| --- | --- |
| `PATH` | Source ownership and legacy roots |
| `NS` | Context and Shared namespaces |
| `SHARED` | Forbidden Shared dependencies |
| `FEATURE` | Flat feature ownership; obsolete role folders are rejected |
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

`TYPE004` reports an ambiguous base type when more than one known type is
available through imports. Use a qualified type name or a `using` alias.

`TEST001` requires every test fixture to live directly in
`Contexts/<Context>/Tests/Editor` or `Contexts/<Context>/Tests/Play`.
Feature-local `Scripts/Tests`, other environment names, and nested test-category
folders are invalid.

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
| `PATH001` | 14 | Shared remains under legacy `Assets/Shared` |
| `PATH002` | 10 | Project-owned scripts remain outside Contexts or ProjectCore/Shared |
| `NAME001` | 9 | `Proyotype` or `Enteties` spelling in paths |
| `SUFFIX001` | 3 | Context script folder/suffix mismatch |
| `TYPE003` | 2 | More than one top-level type in one file |
| `INIT002` | 1 | `async void Start` in the legacy feature installer |
| `CONTEXT002` | 2 | Undocumented shared context category |
| `INIT001` | 2 | Lifecycle callbacks appear to start initialization chains |

The baseline is historical and is not an allowlist. Current strict validation
must remain at zero errors and warnings.

Context-owned scripts use the documented non-feature categories such as
`Abstract`, `Enums`, `Extensions`, `DataHolders`, `Other`, and `Views`. Outside
`Init`, these scripts must not participate in DI. A complete capability,
stateful service, or lifecycle participant remains feature-owned.

## Static Analysis Limits

- MonoBehaviour inheritance is resolved from project-owned types and the known
  roots `UnityEngine.MonoBehaviour`, `Fusion.NetworkBehaviour`, and
  `UnityEngine.UIElements.VisualElement`. Type identities include the namespace
  and generic arity; each inheritance link uses its declaring file's namespace,
  imports, and aliases. An external custom base type may require manual review.
- Type parsing remains lightweight and assumes one primary top-level type and
  one namespace per file, as required by project conventions. It is not a C#
  compiler and does not model every language construct, such as nested types,
  global using directives, or aliases to constructed generic types.
- Manager `AsSingle`, open-generic `AsCached`, and Feature binding discovery is
  heuristic and produces a Warning when the binding cannot be found statically.
- Lifecycle callback inspection intentionally produces Warnings because local
  component setup may be valid.
- Direct `BaseMonoBehaviourFeature` descendants must be registered through
  `AddFeatureFromComponent<TFeature>()`. Exact GameObject placement and
  serialized references still require Unity prefab or scene validation.
- The validator does not replace Unity compilation, Test Runner, scene loading,
  prefab validation, or architecture review.
