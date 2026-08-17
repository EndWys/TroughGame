# TroughGame

TroughGame is a multiplayer friends-slope game about dungeons.

## Project overview

- Unity `6000.3.14f1`
- Zenject dependency injection
- Photon Fusion networking
- UniTask asynchronous runtime
- `Scene_Preloader` is the single application entry point
- Project-owned code is organized by Contexts and Features

The runtime flow is:

```text
ProjectContext -> Preloader -> Gameplay Scene
```

`ProjectContext` persists for the entire application lifetime. Preloader is a
temporary startup context. Gameplay scenes own their scene-scoped context and
are initialized through the common application flow.

## Development rules

- Use DI for services, systems, factories, and other functional dependencies.
- Keep Features independent; connect them through Modules, Template Features,
  or Bridge Features where appropriate.
- Keep Domain independent of external APIs except .NET and Unity APIs.
- Use `Domain` or `ProjectCore.{Context}` namespaces.
- Keep application startup under the single `ApplicationEntryPoint` flow.
- Preserve the project folder taxonomy, suffix conventions, and context
  lifetimes when adding or moving code.

Architecture ownership, dependency direction, DI, and lifecycle rules are
documented in
[`docs/ARCHITECTURE_RULES.md`](docs/ARCHITECTURE_RULES.md).

Detailed coding, member-order, naming, suffix, resource, and folder conventions
are documented separately in
[`docs/CONVENTIONS.md`](docs/CONVENTIONS.md).

Migration stages and their status are documented in
[`docs/ARCHITECTURE_MIGRATION.md`](docs/ARCHITECTURE_MIGRATION.md).

The standard authored hierarchy for gameplay scenes is documented in
[`docs/SCENE_HIERARCHY_STANDARD.md`](docs/SCENE_HIERARCHY_STANDARD.md).

The static architecture validator is documented in
[`Tools/Architecture/README.md`](Tools/Architecture/README.md).
