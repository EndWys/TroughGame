# AGENTS.md

## Project Rules

- Follow `docs/CONVENTIONS.md` for naming, folder structure, resource naming, code style, and member ordering.
- Follow `docs/ARCHITECTURE_RULES.md` for contexts, features, DI, initialization flow, and dependency direction.
- Keep root `README.md` short. Detailed rules belong in `docs/`.
- Do not move `Assets/ProjectCore/Resources/ProjectContext.prefab`; it is the Zenject ProjectContext exception.
- Do not add `.asmdef` to `TroughGame`.

## Architecture Constraints

- `Domain` must stay independent from external APIs except Unity Engine, Unity Editor, and .NET.
- Namespaces must stay simplified: `ProjectCore.{Context}` or `Domain`.
- Features must be initialized through the unified initialization flow.
- Avoid initialization chains hidden in random `Awake`/`Start`; only explicit entry-point/bootstrap components are allowed.
- Use DI for dependencies between services, systems, factories, coordinators, and feature managers.
- Do not create a Feature for scripts that do not need DI, initialization, context bindings, or feature ownership.

## Before Finishing Changes

Run when relevant:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Architecture\Validate-Architecture.ps1 -Mode Strict
dotnet build .\ProjectCore.Runtime.csproj --no-restore -v:minimal
git diff --check
```

## Git

- Preserve Unity `.meta` files.
- Prefer Unity-safe moves/renames: keep `.meta` files with their assets.
- Do not revert user changes unless explicitly asked.
