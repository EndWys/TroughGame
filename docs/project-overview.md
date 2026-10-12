# TroughGame: project overview

TroughGame is a multiplayer game about dungeons, currently developed through
the TechnicalPrototype gameplay context. See the short [root overview](../README.md).
Product documents are under `docs/Project docs/`; read the document selected by
the developer for each feature. Their presence alone does not approve a feature
or resolve a product decision.

Verified during workflow adaptation on 2026-10-12:

| Concern | Existing technology / source |
| --- | --- |
| Editor | Unity `6000.3.14f1`, `ProjectSettings/ProjectVersion.txt` |
| DI | Zenject, context installers under `Assets/ProjectCore/Contexts/` |
| Networking | Photon Fusion, `GameCore/Features/NetworkEntities` |
| Async | UniTask, `Packages/manifest.json` |
| Rendering / input | URP and Input System, `Packages/manifest.json` |
| Editor automation | `com.unity.pipeline` `0.7.0-exp.1`; installed Unity CLI |
| Tests | Unity Test Framework; existing Editor and Play assemblies in Template |
| Static verification | `Tools/Architecture/Validate-Architecture.ps1 -Mode Strict` |

Application lifetime: `ProjectContext -> Scene_Preloader -> gameplay scene`.
`ApplicationEntryPoint` is the sole startup callback; subsequent scene loads use
`ISceneFlowService` and the awaited `IGameSceneLifecycle` contract.

The [architecture rules](ARCHITECTURE_RULES.md),
[conventions](CONVENTIONS.md), and [scene standard](SCENE_HIERARCHY_STANDARD.md)
own project policy. This overview is a discovery aid, not a second rulebook.
