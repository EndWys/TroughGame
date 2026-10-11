# Разбор System и Service

Дата: 2026-10-11. Анализ текущей рабочей копии, включая уже согласованные
изменения нейминга. Первоначальный разбор содержал предложения без изменений
production-кода и нормативных определений. Статус согласованных пунктов
указан ниже; пункт 4 отклонён пользователем. Исходные имена в разделе
анализа описывают состояние до реализации соответствующего пункта.

## Реализация первого пункта

После подтверждения пользователя уточнены определения System и Service в
[`CONVENTIONS.md`](CONVENTIONS.md#system-and-service), синхронизирована таблица
Managers в ARCHITECTURE_RULES и добавлена ссылка на единые критерии выбора.
Закреплены существующие примеры, пересечение ролей и признаки, которые сами
по себе не определяют суффикс. Семантический выбор роли остаётся задачей
code review; новых правил автоматической классификации не добавлено.

Production-код, контракты, DI-регистрации и ресурсы в этом пункте не изменены.
Реализация отдельно согласованных пунктов 2–3 описана ниже. Решение по
пункту 4 зафиксировано ниже; согласованный разбор завершён.

Проверки первого пункта: Strict — 533 скрипта, 167 ресурсов, 0 ошибок и
предупреждений; staged/unstaged git diff --check прошли. Сборка и Unity-тесты
повторно не запускались: этот пункт изменяет только документацию.

## Реализация второго пункта

После подтверждения пользователя выполнено переименование:

- `IGameEntityComponentAccessor` →
  [`IGameEntityComponentService`](../Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Abstract/IGameEntityComponentService.cs).
- `GameEntityComponentAccessorService` →
  [`GameEntityComponentService`](../Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Managers/Services/GameEntityComponentService.cs).
- Обновлены DI-регистрация NetworkEntitiesFeature, зависимости MovementSystem,
  DamageableSystem, DamageSourceSystem и EnemyTargetingService; тестовая
  реализация называется TestGameEntityComponentService.

Два файла переименованы через Unity AssetDatabase. GUID интерфейса
`1f41f9a4f5f64eb492f21ef4afbe9d65` и реализации
`9b11b7115e874779bf63e83ca37df1a9` сохранены. Сигнатуры операций, алгоритмы,
generic-ограничения и snapshot-семантика ApplyToAll сохранены. Обновление
исходников ограничено согласованной заменой идентификаторов в восьми файлах.

Проверки второго пункта: Strict — 533 скрипта, 167 ресурсов, 0 ошибок и
предупреждений; Runtime build — 0 ошибок, 8 прежних предупреждений MSB3277.
Все 134 собственных Editor-теста и 6 Play-тестов прошли без пропусков,
включая EnemyTargetingServiceTests и проверки запуска приложения.
Проверены соответствие восьми файлов запланированной замене имён, оба GUID,
отсутствие старых идентификаторов в C# и старых путей в ссылках документации.
Сериализованные ассеты не изменены; staged/unstaged diff --check прошли.

Первое ожидание Unity recompile в CLI истекло через 90 секунд. Последующий
recompile_status подтвердил completed, compilationFailed=false и пустой
список ошибок; тесты запускались после этого подтверждения.

## Реализация третьего пункта

После подтверждения пользователя выполнено переименование:

- `DamageableSystem` / `IDamageableSystem` → `DamageableEntityService` /
  `IDamageableEntityService`.
- `DamageSourceSystem` / `IDamageSourceSystem` → `DamageSourceEntityService` /
  `IDamageSourceEntityService`.
- Реализации перенесены из Combat/Scripts/Managers/Systems в Managers/Services;
  обновлены DI-регистрации CombatFeature и проверки разрешения контрактов в
  ApplicationStartupPlayModeTests.

Переименование выполнено через Unity AssetDatabase. Четыре script GUID и GUID
папки сохранены; содержимое всех пяти .meta совпадает с исходным побайтно.
Изменения шести исходников ограничены заменой двух имён и их интерфейсов.
Сигнатуры операций, предметные ограничения Base*Component и делегирование
общему GameEntityComponentService сохранены. Расчёт и применение урона
не изменены. Сериализованные ассеты не затронуты.

Проверки третьего пункта: Unity compilation — completed, без ошибок;
Strict — 533 скрипта, 167 ресурсов, 0 ошибок и предупреждений;
Runtime build — 0 ошибок, 8 прежних предупреждений MSB3277.
Все 134 собственных Editor-теста и 6 Play-тестов прошли без пропусков,
включая проверки новых DI-контрактов при запуске. Побайтовая проверка
шести исходников и пяти .meta, отсутствие старых идентификаторов в C#,
проверка ссылок документации и staged/unstaged git diff --check прошли.

## Решение по четвёртому пункту

Пользователь отклонил удаление: MovementSystem пока сохраняется.
IMovementSystem, MovementFeature, регистрация в GameCoreFeatureGroup и
проверка разрешения контракта в startup-тесте остаются. Код и ресурсы
Movement в рамках этого пункта не изменены. Предложение ниже сохранено
как результат анализа, а не как запланированная задача.

## Вывод и границы анализа

Проверены публичные API всех 8 классов System и 15 классов Service в Managers,
реализации спорных классов, DI-регистрации и потребители component-query API.
Это анализ ответственности и именования, а не полный аудит ошибок runtime.

Пять System действительно представляют runtime-подсистемы. Три класса
GameCore с этим суффиксом только делегируют операции component-query сервису.
Особенно выделяется MovementSystem: его API функционально повторяет общий
API без предметного ограничения и не используется gameplay-кодом репозитория.

У сервисов не обнаружено оснований для массового переименования в System.
Однако IGameEntityComponentAccessor содержит ApplyToAll(Action<TComponent>):
его явный поведенческий API шире закреплённой роли Accessor. Это несоответствие
имени контракту, а не доказанный сбой выполнения.

## Предлагаемое различие ролей

| Роль | Основной смысл | Пример |
| --- | --- | --- |
| System | Владелец работы runtime-подсистемы: поддерживает её активные объекты/состояние, правила их взаимодействия, переходы или жизненный цикл. Обработка может запускаться событиями и явными вызовами. | ScreenNavigationSystem поддерживает историю, выбирает переходы и управляет жизненным циклом экранов. |
| Service | Предоставляет определённую возможность через операции/запросы: расчёт, выбор, поиск, сохранение, диагностику или интеграцию. Может иметь состояние и ресурсы, необходимые для этой возможности. | EnemySteeringService рассчитывает результат steering; LocalSaveService предоставляет операции сохранения через storage и serializer. |

Это проектный ориентир для выбора имён, а не непересекающаяся классификация.
Объект может и предоставлять возможность, и поддерживать состояние. В таких
случаях выбираем имя, лучше объясняющее его основной API потребителям;
существующее понятное имя сохраняем без отдельной причины для миграции.

Не использовать как критерий различия:

- Наличие состояния, коллекций, Initialize, Dispose, async или событий.
- Вызов каждый кадр/сетевой тик. MovementSimulationService вызывается из
  NetworkTick, но выполняет операцию Simulate для переданного тела.
- DI lifetime: обе роли следуют общим правилам Managers; суффикс не выбирает
  контейнер, lifecycle или способ регистрации.
- Наличие Unity API. LevelCollisionService выполняет Physics2D-запросы;
  запрет внешних API относится к Shared, а не ко всем Services.
- Размер класса и слово «система» в описании фичи.

System не означает Unity ECS и не требует нового scheduler. Service не
означает stateless/pure C#. System/Service не задают направление зависимостей:
его определяют ownership, публичные контракты и архитектурные правила.
Правило отсутствия прямой зависимости Services от View сохраняется;
SceneFlowService взаимодействует с представлением через Presenter-контракт.

## Все текущие System

| Контекст / класс | Фактическая ответственность | Предложение и ожидаемый результат |
| --- | --- | --- |
| Template / ScreenNavigationSystem | Definitions, история экранов, текущий экран, переходы Show/Hide/Close, защита от параллельных переходов. | Сохранить. Документировать System как владельца подсистемы, работающей также по явным командам. |
| Template / PopupSystem | Активные popup entries, ответы, отмена, блокировка ввода, завершение и очистка окон. | Сохранить: владеет состоянием и жизненным циклом popup-подсистемы. |
| Template / LoadingScreenSystem | Текущий loading screen, его состояния, Show/Hide, отмена при Dispose, блокировка ввода. | Сохранить: один активный объект тоже может составлять подсистему. |
| Template / DebugConsoleSystem | Буферы логов и команд, подписки, выполнение через CheatService, уведомления Controller. | Сохранить: поддерживает runtime-модель консоли. Сам по себе System не обязан владеть View. |
| Template / DebugVisualizationSystem | Создание, запуск и остановка runner, registry, управление каналами и HUD, подключение backend к facade. | Сохранить: владеет работой подсистемы через делегирование runner. Делегирование частей работы допустимо. |
| GameCore / DamageableSystem | Пять forwarding-методов для поиска, сортировки и обработки BaseDamageableComponent. | Если сохраняем предметный API, переименовать в DamageableEntityService. Название перестанет обещать правила расчёта/применения урона, которых здесь нет. |
| GameCore / DamageSourceSystem | Аналогичные forwarding-методы для BaseDamageSourceComponent. | Если сохраняем предметный API, переименовать в DamageSourceEntityService. Название отражает работу с выбранной группой компонентов. |
| GameCore / MovementSystem | Те же пять generic-методов общего component API с единственным ограничением class. | Рекомендовано убрать избыточную прослойку вместо переименования. Movement-специфическая ответственность отсутствует; вводить фиктивный marker interface ради имени не нужно. |

### Что именно не так с тремя GameCore System

[MovementSystem](../Assets/ProjectCore/Contexts/GameCore/Features/Movement/Scripts/Managers/Systems/MovementSystem.cs)
имеет, например, такую операцию:

```csharp
public void ApplyToAll<TMovementComponent>(Action<TMovementComponent> action)
    where TMovementComponent : class
{
    _gameEntityComponentAccessor.ApplyToAll(action);
}
```

Имя generic-параметра не ограничивает его назначение. Подходит любой class или
интерфейс: контракт не требует movement body/state/processor. Класс не запускает
simulation и не задаёт movement-правила; они находятся в state machines,
processors и MovementSimulationService.

Поиск IMovementSystem/MovementSystem нашёл только объявления, регистрацию в
MovementFeature и Resolve-assert в ApplicationStartupPlayModeTests. Реальных
gameplay-вызовов этого контракта в исходниках репозитория нет. Переименование
в MovementService оставило бы те же необоснованную предметную специализацию
и дополнительную прослойку.

Если согласовано удаление: убрать MovementSystem и IMovementSystem с их .meta,
MovementFeature с .meta (его единственная операция — эта регистрация),
AddFeature<MovementFeature>() из GameCoreFeatureGroup и соответствующий assert
из startup-теста. Сохранить Movement как владельца существующих configs,
states и processors, их namespaces, пути и GUID. Поведение этих типов не
зависит от регистрации IMovementSystem. Перед реализацией отдельно согласовать
такое отсутствие lifecycle entry у пассивного набора movement-основ, чтобы
не оставлять пустую Feature только для сохранения имени.

Исходный DamageableSystem (теперь
[DamageableEntityService](../Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Managers/Services/DamageableEntityService.cs))
и
DamageSourceSystem (теперь
[DamageSourceEntityService](../Assets/ProjectCore/Contexts/GameCore/Features/Combat/Scripts/Managers/Services/DamageSourceEntityService.cs))
действительно ограничивают API соответствующими Base*Component. Это
предметные фасады над общей реализацией, но правила damage им не принадлежат.
ApplyDamage принадлежит BaseDamageableComponent, а не DamageableSystem.

Потребителей этих двух контрактов вне объявления, DI и startup-теста тоже
не найдено. Поэтому rename имеет умеренную пользу для читабельности, без
runtime-выигрыша; сохранять их как задел на будущее не является необходимостью.
Удаление фасадов допустимо как альтернатива, если предметный API не нужен,
но вместе с проверкой роли CombatFeature. В рамках текущего нейминга
наименее затрагивающий структуру вариант — два точечных rename.

Repository хуже объясняет полный API с Action по компонентам: эти классы
не владеют коллекцией или её хранением. Accessor не подходит полному API,
потому что callback может выполнить ApplyDamage и другое изменение состояния.
Механическое новое имя System не добавляет отсутствующие gameplay-правила.

## Все текущие Service

| Контекст / класс | Фактическая возможность | Решение |
| --- | --- | --- |
| GameCore / MovementSimulationService | Simulate одного переданного тела с учётом collision strategy. | Сохранить. Вызов из NetworkTick не делает сервис владельцем общей simulation-подсистемы. |
| GameCore / LevelCollisionService | Разрешение displacement, collision queries, obstacle probe; переиспользуемые Physics2D-буферы. | Сохранить. Strategy и Probe — его контракты; Service — роль DI-реализации. |
| GameCore / GameEntityComponentAccessorService | Поиск компонентов через registry, сортировка, выбор первого и ApplyToAll по snapshot компонентов. | Предложено GameEntityComponentService и IGameEntityComponentService; см. ниже. |
| TechnicalPrototype / EnemyTargetingService | Доступность, позиция и выбор ближайшей цели. | Сохранить: конкретная возможность target selection. |
| TechnicalPrototype / EnemySteeringService | Расчёт направления и approach position для текущего enemy; LastCalculation для наблюдения. | Сохранить: конкретная операция, наличие последнего результата допустимо. |
| Template / AppTimeService | Выбор time provider и расчёт временных интервалов. | Сохранить: возможность времени, а не владелец игрового clock loop. |
| Template / CommandLineService | Получение/разбор аргументов и обработка cancel request сервера. | Сохранить: инфраструктурная возможность; события и Dispose не требуют System. |
| Template / CheatService | Регистрация, разбор и выполнение команд через handlers/converters. | Сохранить. Имя Service понятно потребителям ExecuteAsync; совмещение Registry-контракта не требует rename. |
| Template / LocalSaveService | Сохранение/загрузка/миграция через storage и serializer, сериализация доступа к операциям. | Сохранить: прикладная возможность с собственными ресурсами. |
| Template / SceneFlowService | Переход между сценами, typed settings, scene lifecycle, Presenter-контракты. | Сохранить: устоявшийся API LoadAsync. Имеет черты подсистемы, поэтому System был бы допустим, но полезного изменения ответственности от rename нет. |
| Template / LocalConfigService | Типизированные configs текущего контейнера с fallback к parent. | Сохранить: загрузка каталога и политика разрешения, а не только выдача единственного значения. |
| Template / LogService | Подписка на Unity logs и доставка handlers. | Сохранить: диагностическая возможность. |
| Template / DebugLoggerService | Отправка диагностических сообщений в Unity Debug. | Сохранить: DI-сервис выполняет Logger-контракт; не переносить в Other только из-за SDK-вызовов. |
| Template / FeatureDebugLoggerService<TFeature> | Добавление категории и передача сообщений общему Logger. | Сохранить: DI-реализация Logger; имеющаяся open-generic регистрация AsCached остаётся документированным случаем. |
| Template / DebugToolsService | Политики доступности/включения инструментов и command-line overrides. | Сохранить: управление политикой; наличие набора флагов и события не требует System. |

### Accessor с операцией изменения

Исходный IGameEntityComponentAccessor (теперь
[IGameEntityComponentService](../Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Abstract/IGameEntityComponentService.cs))
помимо запросов содержит ApplyToAll(Action<TComponent>). Реализация получает
snapshot ссылок на компоненты и вызывает action для каждого. Например,
переданный callback может вызвать damageable.ApplyDamage(...).

Это явная операция выполнения действий, которая расходится с закреплённым
описанием Accessor как read API. Речь не о возможности технически изменить
объект из Predicate/selector или через возвращённую ссылку: для запросов
следует ожидать callback без side effects. Здесь Action прямо входит в API.

Рекомендованный минимальный вариант: IGameEntityComponentService +
GameEntityComponentService. Сохранить все методы и их поведение, включая
snapshot ApplyToAll. Обновить поля, параметры, DI, EnemyTargetingService и
его test double. Реализация остаётся в Managers/Services.

Query/Mutator split тоже возможен, но добавит интерфейс без установленной
потребности. Удаление ApplyToAll только ради удобного суффикса изменит API.
Для текущего этапа лучше корректно назвать имеющуюся возможность.

## Предлагаемый порядок решений

| № | Изменение | Обоснование / ожидаемый результат | Объём и границы |
| --- | --- | --- | --- |
| 1 | Уточнить System/Service в CONVENTIONS, синхронизировать ARCHITECTURE_RULES, добавить критерии и существующие примеры. | Нынешнее «ongoing runtime rules» недостаточно объясняет пять обоснованных System. После уточнения можно выбирать роль без ошибочных критериев «есть Tick/есть состояние/есть Unity». | Малый; только нормативная документация. Не вводить автоматический семантический классификатор. |
| 2 | Реализовано: IGameEntityComponentAccessor / GameEntityComponentAccessorService → IGameEntityComponentService / GameEntityComponentService. | Активный контракт поиска целей больше не обещает read-only роль при наличии ApplyToAll. | API и поведение сохранены, .meta/GUID сохранены, потребители и test double обновлены. |
| 3 | Реализовано: DamageableSystem / DamageSourceSystem → DamageableEntityService / DamageSourceEntityService и соответствующие интерфейсы. | Имя отражает предметный поиск/обход компонентов, а не отсутствующие правила урона. | Unity-safe moves в Managers/Services, DI и startup assert обновлены; поведение и .meta/GUID сохранены. Польза умеренная: gameplay-потребителей пока нет. |
| 4 | Отклонено пользователем: MovementSystem пока сохраняется. | Предложение удалить дублирующий generic API рассмотрено; принято решение оставить текущую структуру. | IMovementSystem, MovementFeature, регистрация и startup assert сохранены; изменений по этому пункту нет. |

Срочность этих изменений архитектурная, а не критическая runtime. Разбор не
установил, что суффиксы вызывают сбой игры. Начать с документации, затем
согласовывать каждый кодовый шаг отдельно в принятом интерактивном порядке.

При code changes: Strict-валидация, runtime build, соответствующие Editor
tests и ApplicationStartupPlayModeTests; rename assets через Unity с
сохранением metadata. Для удаления Movement — повторный поиск потребителей
и serialized references. Анализ не меняет traversal, allocation, damage или
movement algorithms; такие изменения требуют отдельного обоснования и тестов.
