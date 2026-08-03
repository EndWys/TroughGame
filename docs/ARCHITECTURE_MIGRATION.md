# TroughGame Architecture Migration

This document contains the migration stages and their completion status.
Permanent architecture and convention rules live in
`docs/ARCHITECTURE_RULES.md` and `docs/CONVENTIONS.md`. The root `README.md`
remains a short project overview.

## Current status

The migration is complete. Future changes must preserve the documented
namespace, Feature taxonomy, DI, lifetime, and single-entry-point rules.

## Stages

### Stage 7 — Unified Application Entry Point

Status: completed.

- `Scene_Preloader` is the first Build Settings scene.
- `ApplicationEntryPoint.Start()` is the only application startup trigger.
- Startup runs Project initialization, Preloader initialization, then loads the
  first gameplay scene through `ApplicationFlowCoordinator`.
- `ProjectContext` owns persistent application flow; Preloader is temporary.
- Game scenes resolve `IGameSceneInitializer` and report ready only after their
  feature flow completes.

### Stage 8 — Initialization Infrastructure

Status: completed.

Generic Feature and context initialization infrastructure is located in
`Assets/ProjectCore/Contexts/Template/Scripts/Initialization`.
`ApplicationInitializationFeature` is owned by Preloader. `ApplicationFlow`
remains a Template Feature with Project as its runtime owner.

### Stage 9 — Feature Service Initialization

Status: completed after audit.

Features use the mandatory `IBaseFeature.InstallBindings(DiContainer)` and
`IBaseFeature.InitializeAsync(CancellationToken)` lifecycle. Services that
require explicit initialization are resolved as their concrete implementation
and invoked from the owning Feature without expanding their consumer-facing
interface.

### Stage 10 — Runtime and Test Assemblies

Status: completed.

Production code uses `ProjectCore.Runtime`. Editor and Play tests use separate
test assemblies under `Assets/ProjectCore/Contexts/Template/Tests`.

### Stage 11 — Startup and Navigation Tests

Status: completed and verified.

Edit Mode and Play Mode tests pass. Play Mode coverage verifies Preloader to
Prototype startup, Preloader entry-point cleanup, ProjectContext persistence,
and SceneContext recreation during navigation.

Cancellation and initialization-failure fixtures remain optional extensions
because current production scenes do not provide controllable failure stages.

### Stage 12 — Final Stabilization Audit

Status: completed.

- Runtime, Editor test, and Play test assemblies compile successfully.
- Edit Mode and Play Mode tests pass.
- Unity scenes and DI lifetime behavior were manually verified.
- Namespace, naming, folder taxonomy, and Feature placement were audited.
- Strict architecture validation completed with `0 errors, 0 warnings`.

## Migration source rules

`D:\Repositories\BG-Games-Platform` is the reference implementation when
TroughGame lacks required infrastructure. Copy only the smallest required set
of scripts and assets, then adapt namespaces, ownership, DI, folder taxonomy,
startup flow, and context lifetime to TroughGame rules. Do not copy project
assembly definitions, credentials, generated files, or local settings.

Always preserve Unity `.meta` files, validate prefabs after renames, and do not
edit generated `.csproj` or `.sln` files manually.
