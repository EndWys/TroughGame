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
class in Components must produce `VIEW001` even when another context has a valid
MonoBehaviour with the same name. Other cases cover local-name precedence,
imports, qualified names, aliases, generic arity, multiline generic bases,
ambiguous imports, framework namesakes, Managers, context-owned Components, and cycles.
Generated fixtures are created in a temporary directory and removed after the run.

Run naming-rule regression checks:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass `
  -File .\Tools\Architecture\Test-NamingValidation.ps1
```

The naming runner checks the same `Editor`, `EditorWindow`, and
`AttributeDrawer` suffixes for context and feature tooling. It also verifies
that top-level abstract classes require `Base` outside Abstract, while the
four documented Template generic definition families and Shared primitives
retain their naming scope. Negative fixtures check the exception's owner,
generic arity, and base type. Each case checks exact diagnostics and exit codes.

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
| `NAME` | Known spelling, interface/base prefixes, and legacy suffix rules |
| `INIT` | Initialization callback and `async void` checks |
| `TEST` | Flat Editor and Play test placement |
| `ASSET` | Own resource naming, ownership, placement, metadata and authoring |

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
`Abstract`, `Enums`, `Extensions`, `DataHolders`, `Other`, and `Components`. Outside
`Init`, these scripts must not participate in DI. A complete capability,
stateful service, or lifecycle participant remains feature-owned.

## Static Analysis Limits

- `Other/ViewModels` and `Other/Presenters` accept concrete `*ViewModel` and
  `*Presenter` presentation objects respectively under
  context/feature ownership. Suffix checks do not prove MVVM responsibilities,
  plain C# inheritance, per-view lifetime or approval of an MVP choice; review these against
  [UI_TOOLKIT_STANDARD.md](../../docs/UI_TOOLKIT_STANDARD.md). ViewModels are not
  passive Models or container-lifetime Managers; the same applies to Presenters.

- The naming vocabulary is defined in `docs/CONVENTIONS.md`. Contract and
  implementation suffixes need not match, subject interfaces may omit a role
  suffix, and nested implementation types do not follow top-level category
  suffix rules. Contract responsibility, word order, generic parameter meaning,
  injection method names, and `Empty`/`Default`/`NoOp` semantics require code
  review; a clean validator report does not certify these naming decisions.
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

## Resource validation

Run `powershell -NoProfile -ExecutionPolicy Bypass -File
Tools/Architecture/Test-ResourceValidation.ps1` from the repository root.
The runner checks valid resource folders, negative naming/ownership/metadata cases,
the ProjectContext and third-party exceptions, and SceneDefinition placement.

Own non-C# resources under ProjectCore are checked; Plugins and ThirdParty are
excluded. C# fixture metadata is not required by this resource stage. Existing
SDK roots outside ProjectCore are not relocated or treated as project-owned content.

Common UI resources use root `Assets/ProjectCore/Assets` semantic folders from
[CONVENTIONS](../../docs/CONVENTIONS.md#common-ui-resource-layout). Font source
assets (`.ttf`, `.otf`) require `Font_` naming and `Assets/Fonts` placement.
Resource fixtures cover the shared root layout and invalid names/folders.
Cross-context UI references and common-resource consumers still require reference
inspection/review; this path validator does not enforce the dependency graph.

| Rule | Check |
| --- | --- |
| ASSET001 | Matching resource .meta exists |
| ASSET002 | Project/context/feature Assets root, with ProjectContext exception |
| ASSET003 | Prefix and name pattern for the resource extension |
| ASSET004 | Semantic folder; SceneDefinition exception requires its matching scene |
| ASSET005 | Config owner matches its containing context |
| ASSET006 | Empty folders, warning only |
| ASSET007 | CreateAssetMenu context spelling matches the script context |

`VIEW001` and `VIEW002` retain their IDs for compatibility but now validate
`Scripts/Components`. Ordinary `*Component` scripts live in that root; role
folders retain their suffixes and MonoBehaviour/VisualElement checks.
The validator cannot infer semantic ownership from arbitrary serialized references;
resource types and their configured instances may have different owners.
