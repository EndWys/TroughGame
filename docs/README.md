# Документация TroughGame

Документация содержит действующие стандарты и руководства возможностей.
При изменении правила обновляем документ-владелец; остальные документы
ссылаются на него.

## Действующие стандарты

| Тема | Документ-владелец |
| --- | --- |
| Contexts, Features, владение, зависимости, DI и инициализация | [ARCHITECTURE_RULES.md](ARCHITECTURE_RULES.md) |
| Папки внутри владельца, категории, суффиксы, нейминг, стиль и имена ресурсов | [CONVENTIONS.md](CONVENTIONS.md) |
| Авторская иерархия игровых сцен | [SCENE_HIERARCHY_STANDARD.md](SCENE_HIERARCHY_STANDARD.md) |
| Команды валидатора, диагностические коды, fixtures и пределы анализа | [Tools/Architecture/README.md](../Tools/Architecture/README.md) |
| Инструкции работы агента и обязательные проверки | [AGENTS.md](../AGENTS.md) |

Root README остаётся кратким обзором. Его краткие напоминания и AGENTS
ссылаются на стандарты; полные таблицы правил там не дублируются.

## Руководства возможностей

- [Network Entities](features/NETWORK_ENTITIES.md): uint ID, протокол, lifetime
  и component-query API.
- [Local Config](features/LOCAL_CONFIG.md): каталоги, lookup и владение config.
- [Logging](features/LOGGING.md): feature-категории, logger lifetime и handlers.
- README внутри Template Features: [Cheat](../Assets/ProjectCore/Contexts/Template/Features/Cheat/README.md),
  [Command Line](../Assets/ProjectCore/Contexts/Template/Features/CommandLine/README.md),
  [Debug Tools](../Assets/ProjectCore/Contexts/Template/Features/DebugTools/README.md),
  [Debug Console](../Assets/ProjectCore/Contexts/Template/Features/DebugConsole/README.md),
  [Debug Visualization](../Assets/ProjectCore/Contexts/Template/Features/DebugVisualization/README.md),
  [Loading Screen](../Assets/ProjectCore/Contexts/Template/Features/LoadingScreen/README.md),
  [Local Save](../Assets/ProjectCore/Contexts/Template/Features/LocalSave/README.md),
  [Popup](../Assets/ProjectCore/Contexts/Template/Features/Popup/README.md),
  [Screen Navigation](../Assets/ProjectCore/Contexts/Template/Features/ScreenNavigation/README.md),
  [Scene Flow](../Assets/ProjectCore/Contexts/Template/Features/SceneFlow/README.md)
  и [окно Scene Definition](../Assets/ProjectCore/Contexts/Template/Features/SceneFlow/SCENE_DEFINITION_WINDOW.md).
