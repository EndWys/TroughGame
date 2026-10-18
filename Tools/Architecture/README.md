# Architecture Validator

On-demand reference for `Validate-Architecture.ps1`. Reading this README is not
required for every feature task; the validation requirements in AGENTS still apply.
Read only the section needed to run the tool or understand a diagnostic.
Validator regression tests are for maintaining the tool.

The script checks project source/resource structure without loading Unity.
Architecture and naming policy belong to [ARCHITECTURE_RULES](../../docs/ARCHITECTURE_RULES.md)
and [CONVENTIONS](../../docs/CONVENTIONS.md); runtime UI policy belongs to
[UI_TOOLKIT_STANDARD](../../docs/UI_TOOLKIT_STANDARD.md).

## Usage

Run from the repository root. Normal project verification:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Validate-Architecture.ps1 -Mode Strict
```

`Strict` exits with code `1` when Error diagnostics remain, otherwise `0`.
`Report` lists diagnostics without failing for those violations. Warnings require
review and do not themselves fail Strict mode. `-SummaryOnly` suppresses individual
diagnostics; omit it when investigating a failure.

For a JSON report when needed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Validate-Architecture.ps1 -Mode Report -OutputPath .ai/evidence/architecture/result.json
```

An alternate checkout or fixture can be selected with `-ProjectRoot`.

## Diagnostic Families

Use the reported rule, file, line and message to locate the issue. Consult the
owning project standard before changing code; a diagnostic is not authorization
for unrelated cleanup.

| Prefix | Area |
| --- | --- |
| `PATH`, `NS`, `SHARED` | Source ownership, namespaces and Shared dependencies |
| `FEATURE`, `SCRIPT`, `CONTEXT` | Feature/context placement and script categories |
| `SUFFIX`, `NAME`, `ABSTRACT`, `ENUM`, `STATIC` | Naming and type categories |
| `TYPE` | File/type correspondence and type resolution |
| `MANAGER`, `VIEW`, `DI` | Managed types, components and DI placement |
| `INIT` | Initialization callbacks and async startup |
| `TEST` | Editor/Play test placement |
| `ASSET` | Resource naming, ownership, placement and metadata |

`TYPE004` indicates ambiguous base-type resolution; inspect imports and qualified
names. `VIEW001`/`VIEW002` retain their original IDs but check `Scripts/Components`.
Other individual diagnostic messages describe the failed check.

## Analysis Limits

- Parsing is lightweight, not compiler-based. Nested types, global usings, aliases
  to constructed generic types and external custom bases may require manual review.
  Inheritance resolution uses project types and known MonoBehaviour/NetworkBehaviour/
  VisualElement roots, namespaces, imports, aliases and generic arity.
- DI binding discovery and initialization callback checks are heuristic; warnings
  need contextual review rather than automatic code changes.
- Naming/category checks do not prove semantic responsibility or architecture quality.
  ViewModel/Presenter suffixes do not establish MVVM/MVP correctness or per-view lifetime.
- Resource checks cover project-owned resources; Plugins/ThirdParty are excluded.
  Serialized references, cross-context consumers and semantic ownership require
  inspection; path checks do not establish the dependency graph.
- A clean report does not replace compilation, behavior tests, scene/prefab checks
  or code review.

## Validator Maintenance

Run the relevant regression runner when changing validator behavior or its fixtures,
not during ordinary feature development:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Test-TypeResolution.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Test-NamingValidation.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Test-ResourceValidation.ps1
```

These cover type/inheritance resolution, naming rules and resource validation,
including negative cases and expected diagnostics/exit codes. Generated fixtures
are temporary. The checked-in positive fixture can also be run directly:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Validate-Architecture.ps1 -ProjectRoot .\Tools\Architecture\Fixtures\Valid -Mode Strict
```

That fixture is expected to report zero errors and warnings. Fixture results describe
the validator's regression coverage, not the current game project's runtime readiness.
