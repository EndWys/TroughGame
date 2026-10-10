# Независимая проверка плана улучшений архитектуры

Дата: 2026-10-09. Проверенная ревизия: `bf7717e` и текущий рабочий каталог.
Объект проверки: `docs/ARCHITECTURE_IMPROVEMENTS_PLAN.md`, пункты L1–L7 и G1–G7.

## Заключение

**План полезен как список кандидатов, но его полный объём не подтверждён как
необходимая архитектурная миграция.** Наиболее убедительные изменения исправляют
конкретные ошибочные ветки и несогласованные правила. Создание новых processors,
coordinators и interfaces в большинстве случаев является вариантом реализации,
а не доказанной необходимостью.

Рекомендуемый ближайший объём: исправить пути существующих Player prefab tests;
сузить G7 до документов и воспроизведённого дефекта валидатора; исправить
финализацию SceneFlow и preflight Player spawn. Для ScreenNavigation ошибка
подтверждается кодом, но активное использование capability не найдено: исправить
перед подключением новых экранов либо включить отдельной небольшой задачей.
Для EnemySteering оправдано небольшое улучшение API и проверяемости расчёта;
оно не должно предшествовать исправлениям текущих активных механизмов только
ради архитектурного примера.

Результат проверки: **2 пункта оставить, 8 сузить/упростить, 3 отложить,
1 сначала подтвердить**. «Сузить» означает, что часть пользы подтверждена,
но более крупное предлагаемое решение пока избыточно.

## Метод и пределы вывода

- Исходный план не использовался как доказательство. Проверены реализации,
  реальные вызовы и bindings, существующие Editor/Play tests, документы и скрипт
  архитектурной проверки.
- Production-код и исходный план не изменялись. Создан только этот отчёт.
- Unity Editor/Play Mode, игровые сценарии, Fusion session и профилирование
  не запускались. Поэтому ниже нет утверждений о воспроизведённом игровом баге,
  измеренном ускорении или доказанной сетевой утечке.
- Независимо запущен текущий structural validator:
  `Validate-Architecture.ps1 -Mode Strict -SummaryOnly` → 517 файлов,
  0 ошибок, 0 предупреждений.
- Дополнительно выполнен эксперимент самого валидатора на временном fixture
  вне проекта. Он воспроизводит ложное отсутствие ошибки для одноимённых типов
  разных namespaces; детали приведены в G7.
- Сборка production-кода заново не запускалась: код не изменён. Зелёная сборка
  сама по себе не проверяет строковые AssetDatabase paths или отмену переходов.

Уровни доказательности: **код** — конкретная ветка ошибки/неизбежная операция;
**структура** — потенциальная польза сопровождению без найденного сбоя;
**гипотеза** — нужен runtime-тест, измерение или новый потребитель.

## Вердикты по всем пунктам

| ID | Вердикт | Подтверждение и практическая польза | Минимальный объём / приоритет |
| --- | --- | --- | --- |
| L1 | **Сузить/упростить** | Код смешивает world reads и математику; свойство хранит результат предыдущего вызова. Текущий caller соблюдает порядок. | Один результат Direction + ApproachPosition; небольшой вычислительный helper при появлении содержательных тестов. P2, без snapshot framework. |
| L2 | **Отложить** | Правила retarget/dash доступны для извлечения, но текущая связная orchestration сама по себе не дефект. Dash уже отдельный processor. | Вернуться при усложнении правил или необходимости независимо тестировать policy. P3. |
| L3 | **Сузить/упростить** | Временные списки и повторные обходы подтверждены. Стоимость для кадра не измерена. | Убрать материализацию у first/min, сохранить семантику дерева и snapshots. P2; spatial index и широкая миграция API отложены. |
| L4 | **Оставить** | При cancel Show новый view остаётся в hierarchy вне history; закрытие history может завершиться частично. | Исправить инварианты ошибок/отмены без нового navigation framework. P2 сейчас, до активного использования; P1 при пользовательском сбое. |
| L5 | **Оставить** | Флаг сбрасывается до async cleanup; исключение Hide перекрывает исходный результат. Capability используется startup. | Небольшая правка finally и обработка ошибок; phase-specific cancellation проверить отдельно. P1. |
| L6 | **Отложить** | Sampling — маленький связный участок controller, повторного потребителя и UI tests с fake metrics нет. | Provider при новых метриках, повторном использовании или полезных controller tests. P3. |
| L7 | **Отложить** | Silent fallback подтверждён, NTP участвует в startup; продуктового потребителя сетевого UTC не найдено. | Одна диагностика допустима; смена сетевой реализации после требования к startup/cancel. P3. |
| G1 | **Сузить/упростить** | Player проверяет root после Spawn без cleanup, Enemy — до. | Исправить Player preflight; общий механизм после реального совпадения политики. Исправление P1, обобщение P3. |
| G2 | **Сузить/упростить** | Player spawnpoints проверяются поздно; duration используется как делитель. Не все SO доступны scene feature при старте. | Валидировать конкретные обязательные настройки в ближайшей точке readiness. P2. |
| G3 | **Сузить/упростить** | Targeting получает существующий accessor и registry ради дополнительных read methods. | Дополнить accessor точечным чтением. Новые query/spawn interfaces — только при подтверждённом выигрыше. P2 локально. |
| G4 | **Сузить/упростить** | Ошибочные ветки L4/L5 есть; Popup/Loading/Console уже имеют ownership/cancel. | Контракты и тесты затронутых операций вместе с L4/L5. Отдельную общепроектную переработку исключить. P2. |
| G5 | **Сначала подтвердить** | Порядок register → Init и optional dependency есть; повреждение registry, pooling leak или rollback failure не доказаны. Camera уже освобождает регистрацию. | Сценарии partial Init failure и повторного spawn/despawn, затем конкретная правка. Текущие время/authority кратко документировать. |
| G6 | **Сузить/упростить** | Runtime Resolve factory есть; подписки и cleanup уже симметричны. | Получить factory при явной initialization. Полноценные coordinators отложить до усложнения ownership. P2 малой правки. |
| G7 | **Сузить/упростить** | Документы расходятся; потеря namespace в typeMap реально подавляет VIEW001. Полный dependency analyzer не требуется для этих ошибок. | Исправить документы и resolution одноимённых типов + fixtures. P2. Semantic dependency tooling — отдельное исследование. |

## Дополнительная находка: сначала восстановить существующую проверку prefab

[PlayerPrefabTests.cs:15](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Tests/Editor/PlayerPrefabTests.cs:15)
и [строка 19](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Tests/Editor/PlayerPrefabTests.cs:19)
содержат `Features/Implementations/Player/...`. Фактические assets лежат в
`Features/Player/...`. Независимый `Test-Path` вернул `False` для старого prefab
path и `True` для нового. Тест загружает prefab по этой константе и сразу
проверяет `Is.Not.Null`.

Это подтверждённая несогласованность после flattening фич. Unity-тест в этой
проверке не запускался, но корректность его пути уже опровергнута файловой системой.
Рекомендация: обновить две константы и запустить существующие prefab tests до
использования их как доказательства безопасности G1/G2/G5. Новый слой, сервис
или тестовая инфраструктура для этого не нужны.

## Доказательства, меньшие решения и условия приёмки

### L1. EnemySteering

[EnemySteeringService.cs:39](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Managers/Services/EnemySteeringService.cs:39)
читает Transform, получает список registry, вычисляет group count/index,
approach offset и separation. Математические helpers уже приватные static
methods; это не монолит без внутренних границ.
[ApproachPosition:23](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Managers/Services/EnemySteeringService.cs:23)
действительно является побочным результатом вызова.

Однако единственный активный caller
[ChasingState:103](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Views/States/EnemyBehaviourChasingState.cs:103)
сначала вызывает CalculateDirection, затем читает ApproachPosition в строках
113 и 122, без await между ними. Дефект порядка доступа здесь не найден.

Меньшее изменение: вернуть оба значения из одного метода. Вынести формулы в
processor стоит, если это позволяет написать самостоятельные сценарии группировки,
separation и coincident positions; второй injectable service или interface ему
не обязателен. Сбор входа не должен вводить копию всех врагов для каждого врага
каждый tick. Возврат связанных результатов даёт пользу API сразу; полная
подготовка снимка мира имеет заметно большую цену.

Приёмка: прежний group order/fallback сохранён, тесты проверяют поведение,
новая модель входа не увеличивает per-tick allocation. Оптимизация AI из одного
извлечения не следует.

### L2. Retarget/dash

[ReassessTarget:133](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Views/States/EnemyBehaviourChasingState.cs:133)
содержит связное правило: availability, преимущество кандидата, его смена,
elapsed time и reset. Размещение такого правила в state допустимо: это часть
поведения состояния, а не само по себе нарушение проекта.
[Dash processor:34](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Other/Processors/EnemyDashChaseAbilityProcessor.cs:34)
уже изолирует dash eligibility и геометрию; доступ к Unity/Fusion разрешён.
Название Processor не означает обязательную чистую функцию.

Польза извлечения — возможность фиксированных policy tests, а не найденный баг
или reuse. Цена — новый input/output contract, передача state и потенциальное
усложнение небольших правил. Сначала определить конкретные тесты/изменения
баланса, которые неудобны в текущей форме. Retarget и dash не обязаны мигрировать
вместе. Если L1 уже решает testability steering, дальнейшее извлечение не является
автоматическим следующим этапом.

### L3. Entity queries

[Accessor:44](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Managers/Services/GameEntityComponentAccessorService.cs:44)
создаёт IDs list для первого результата. Predicate overload registry дополнительно
[создаёт componentBuffer:195](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Managers/Registries/NetworkEntityRegistry.cs:195).
Nearest query [строка 108](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Managers/Services/GameEntityComponentAccessorService.cs:108)
материализует IDs и второй раз ищет компоненты; сортировки в этом методе нет.
AI действительно вызывает nearest query через
[EnemyTargetingService:53](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Managers/Services/EnemyTargetingService.cs:53).

Локальное устранение этих списков имеет доказуемый результат — исчезают конкретные
allocation sites. Выигрыш в FPS, лимит количества врагов и необходимость spatial
partitioning остаются неизвестными.

Важные ограничения, которые следует добавить к плану:

- Predicate search сейчас подходит, если **любой** компонент дерева сущности
  удовлетворяет predicate (`FillEntityComponents` → `Exists`). Подмена этого
  на predicate первого найденного компонента изменяет контракт.
- Min query сейчас использует первый подходящий компонент каждой сущности и
  сохраняет первого победителя при равных keys. Его семантика отличается от
  «минимум по всем компонентам дерева».
- Predicate/selector — callbacks. Для direct traversal нужно явно запретить
  mutation либо сохранить snapshot там, где она допустима. Short-circuit также
  меняет количество вызовов predicate; query predicates должны быть без side effects.
- [ApplyToAll:141](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Managers/Services/GameEntityComponentAccessorService.cs:141)
  собирает **components snapshot**, а не только IDs. Это позволяет вызвать action
  для уже полученных ссылок при изменении registry. Оставить этот контракт,
  отдельно уточнив, допустима ли работа с компонентом уже despawned объекта.

Цена умеренная при точечной правке; общий traversal API и migration всех фасадов
пока не нужны. Приёмка: nested components, несколько однотипных компонентов,
tie order, reentrancy/mutation и GC query отдельно от остальных allocation AI.

### L4. ScreenNavigation

[System:229](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/ScreenNavigation/Scripts/Managers/Systems/ScreenNavigationSystem.cs:229)
добавляет view в hierarchy, затем await Show, но history обновляет лишь на строке
241. Catch/finally не очищают новый view.
[BaseScreenView:42](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/ScreenNavigation/Scripts/Views/Screens/BaseScreenView.cs:42)
имеет cancellable Yield/Delay, поэтому для этого сценария не требуется выдумывать
неизвестного наследника. Повторный переход больше не защищён IsTransitioning,
а потерянный view не находится в history. Частичное закрытие history и GoBack
также требуют определённого fail-state.

**Проблема подтверждена кодом.** При этом поиск callers нашёл OpenRoot только в
[legacy PrototypeFeature:18](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Prototype/Features/Prototype/Scripts/Init/PrototypeFeature.cs:18).
Capability установлена в активном контексте, но активных вызовов открытия/назад
не найдено. Поэтому она не обосновывает срочную массовую переделку текущего UI.

Меньшее решение: учитывать ownership нового view сразу после создания;
убирать его на abort, обеспечивать согласованное текущее/history после
необратимого Close. Не обещать восстановить уже закрытый view с произвольным
OnClose. У fake view отдельно проверять Show/Hide/Close faults и каждый await;
Play Mode нужен для настоящей hierarchy/анимаций. Цена средняя, так как требуется
решение о поведении history, а не только добавление одного catch.

### L5. SceneFlow

[Finally:166](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/SceneFlow/Scripts/Managers/Services/SceneFlowService.cs:166)
снимает IsTransitioning до await Hide; новый Load может стартовать, пока старый
Hide ещё выполняется. Исключение из
[Hide:213](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/SceneFlow/Scripts/Managers/Services/SceneFlowService.cs:213)
перекрывает Result/исходное исключение из try/catch. Это подтверждённые свойства
публичной операции. [Startup caller:41](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Preloader/Features/ApplicationInitialization/Scripts/Managers/Flows/ApplicationInitializationFlow.cs:41)
реально использует сервис.

Текущая композиция даёт один presenter — SceneLoadingScreenAdapter. Поэтому
«один неисправный presenter оставляет остальные» сейчас является риском
расширяемого API, а не найденным сценарием нескольких действующих presenters.
Он недорого проверяется fake presenters и не требует новой архитектуры.

Меньшее решение: держать флаг до конца cleanup, гарантировать сброс даже при его
сбое, определить возвращаемый результат и диагностику primary/secondary errors.
Нужен тест повторного запроса из незавершившегося Hide.

Cancellation требует отдельного фазового теста. Нельзя просто сделать все catch
одинаковыми: отмена Show до Exit должна сохранить старую сцену. Caller token
[связан с scene lifetime:147](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/SceneFlow/Scripts/Managers/Services/SceneFlowService.cs:147),
но startup передаёт application token; подтверждённой преждевременной отмены
загруженной сцены нынешним caller нет. Полный rollback Unity load обещать нельзя.
Цена маленькой finally-правки низкая; полной смены lifetime semantics — средняя
и нуждается в integration tests.

### L6. Performance sampling

[RenderPerformance:450](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/DebugConsole/Scripts/Managers/Controllers/DebugConsoleController.cs:450)
формирует один существующий data object из нескольких Unity counters. Это примерно
один маленький метод, связанный с отображением performance tab. Controller
уже [проверяет tab и throttling:266](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/DebugConsole/Scripts/Managers/Controllers/DebugConsoleController.cs:266).

Новый provider удобен для fake metrics и второго HUD, но такие потребители/тесты
не найдены. Отделение не устраняет текущую ошибку и не даёт доказанного ускорения.
До появления ещё одной метрики с собственным lifecycle, Recorder или повторного
потребителя оставить существующий метод. При появлении такой потребности
выделение небольшое и оправданное; сейчас P2 завышен.

### L7. NTP fallback

[Provider:36](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/AppTime/Scripts/Managers/Providers/NtpTimeProvider.cs:36)
молча fallback-ится, а worker
[GetNetworkTime:50](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/AppTime/Scripts/Managers/Providers/NtpTimeProvider.cs:50)
не получает token. Socket timeout не задаёт отдельного timeout для DNS.
[AppTimeFeature:22](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/AppTime/Scripts/Init/AppTimeFeature.cs:22)
действительно ждёт NTP в startup. Поэтому потенциальная задержка относится к
реальному запуску, а не к полностью неиспользуемому классу.

Однако gameplay потребителей IAppTimeService не найдено; реальных сбоев startup,
размеров задержки и требования к точности времени не предъявлено. Одна
диагностическая запись/статус источника — дешёвая наблюдаемость, но это не
архитектурное основание менять транспорт. Возобновить при измеренной задержке
startup/cancel либо появлении механики, зависящей от UTC. Сетевой UTC клиента
сам по себе не должен считаться authoritative серверным временем.

### G1. Spawn guarantees

[Player factory:32](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Player/Scripts/Managers/Factories/PlayerNetworkEntityFactory.cs:32)
сначала Spawn, root type проверяет на строке 45; wrong root → throw без Despawn.
В callback ID присваивается только если ожидаемый компонент найден.
[Enemy factory:44](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Managers/Factories/EnemyNetworkEntityFactory.cs:44)
проверяет загруженный prefab до Spawn. При успешно спавнящемся неверном Player
prefab ошибка конфигурации может оставить объект вне результата factory.
Поведение Fusion при таком сценарии нужно проверить в работающей session;
локально отсутствие cleanup в factory доказано.

Меньшее достаточное решение — аналогичный typed prefab preflight у Player,
сохранить собственную input authority policy. Отдельный широкий generic helper
не необходим для устранения этой ветки. Существующий
[BaseNetworkEntityFactory:12](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Abstract/BaseNetworkEntityFactory.cs:12)
уже унифицирует typed payload dispatch.

Обобщение сделать позднее, если совпадает повторяемый механизм, его error
semantics и prefab modes. Разные Despawn guards сами по себе не доказывают баг:
NetworkEntitySpawner выбирает factory по EntityType, обычный caller не выбирает
произвольную factory. Wrong-type guard полезен как защита API, но это отдельная
гарантия от wrong-prefab cleanup. Приёмка — корректный spawn плюс неверный
зарегистрированный prefab; invalid ref может быть отвергнут SDK раньше и не
проверяет интересующую ветку.

### G2. Configuration validation

[PlayerSpawnComponent:53](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Player/Scripts/Views/Components/PlayerSpawnComponent.cs:53)
проверяет spawnpoints только при join. Это реальный поздний fail-point.
[DashMovementConfig:17](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/Movement/Scripts/DataHolders/Configs/DashMovementConfig.cs:17)
делит Distance на DurationSeconds; runtime precondition duration > 0 полезен.
Но наличие `[Min]` и отсутствие runtime validation не доказывает, что текущие
assets испорчены или что каждую настройку необходимо переписать.

Рекомендовать проверки Player spawnpoints/prefab и делителей/обязательных
references в затронутых prefab components. «Всё проверить в Feature.Initialize»
слишком широко: сетевой prefab создаётся позднее, per-entity configs/компоненты
могут быть недоступны feature при startup. Использовать ближайшую явную
точку readiness — scene init для scene-owned данных, entity init/preflight для
prefab-owned. Также
[EnemySpawnComponent:36](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Views/Components/EnemySpawnComponent.cs:36)
проверяет при init основной enemy prefab, а dash prefab — при конкретном spawn;
политика обязательного/опционального dash prefab должна быть решена явно.

Не вводить universal validation framework, не замораживать SO без нужды.
Приёмка — сообщение с asset/полем до первого его использования и корректное
failure/readiness поведение. Цена низкая локально, высокая у полной ревизии всех
configs; последняя не обоснована этим набором примеров.

### G3. Public contracts

[EnemyTargetingService:12](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Enemy/Scripts/Managers/Services/EnemyTargetingService.cs:12)
принимает accessor для search и registry для TryGetComponent. Конкретная польза
меньшей правки: добавить точечное чтение к существующему read accessor и убрать
вторую зависимость из targeting; текущие targeting tests смогут проверять
availability/position через один fake.

Steering читает список roots по типу, root contracts и Transform; общего accessor
может быть недостаточно. Не превращать его в полный дубликат registry только
ради единственного constructor. Можно передавать локальные steering inputs в L1.
Spawn interface имеет смысл при содержательных тестах spawn orchestration;
сейчас concrete NetworkEntitySpawner уже сам hides factories dispatch.

Документы уже требуют межфичевые public contracts; заново объявлять это улучшением
не нужно. Уточнить существующие read operations, а полный список feature internals,
новые interface families и строгий инструментальный запрет делать при реальной
проблеме изменения поставщика. Иначе цена навигации по forwarding types выше пользы.

### G4. Async ownership

Гарантии L4/L5 оправданы. Но
[LoadingScreenSystem.Dispose:152](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/LoadingScreen/Scripts/Managers/Systems/LoadingScreenSystem.cs:152),
[PopupSystem.Dispose:141](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/Popup/Scripts/Managers/Systems/PopupSystem.cs:141)
и [Console.Detach:82](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/DebugConsole/Scripts/Managers/Controllers/DebugConsoleController.cs:82)
уже реализуют ownership и cancellation. Их наличие не доказывает повторяющийся
дефект всех async-операций. Cleanup fallback без вторичного лога может быть
осознанным recovery; диагностику выбирать под конкретный failure.

Минимум — записать контракты отмены и cleanup при исправлении L4/L5, не создавать
самостоятельный многосервисный рефакторинг. Post-Detach completion Console —
полезный сценарий тестирования, но баг требует command, который игнорирует
cancellation; по одному доступу к полям после await он не воспроизведён.
Owner generation/check нужен, только если такой сценарий поддерживается.

Разделять SceneFlow request/scene token сейчас не требуется ради startup:
application lifetime уже передаётся корректно. Решить новый контракт прежде
чем появится caller с краткоживущим request CTS. Общая небольшая policy полезна,
универсальные wrappers и одинаковые tokens для всех capabilities не оправданы.

### G5. Entity lifecycle, authority, time

[Root.Spawned:61](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Abstract/BaseNetworkEntityRoot.cs:61)
регистрирует root до components Init, а
[optional registry:22](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/NetworkEntities/Scripts/Abstract/BaseNetworkEntityRoot.cs:22)
и invalid ID могут пропустить регистрацию. На уровне метода rollback после
Init exception отсутствует. Это основания для проверки readiness semantics,
но не доказательство того, что SDK оставляет объект зарегистрированным после
такого failure во всех modes.

[CameraTargetComponent:46](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/GameCore/Features/CameraTarget/Scripts/Views/Components/CameraTargetComponent.cs:46)
уже освобождает регистрацию через Despawned и OnDestroy. Общий teardown на
INetworkEntityComponent дублировал бы этот механизм без выявленного владельца,
которому не хватает cleanup. Выяснить, используются ли reuse/pooling и какие
компоненты имеют ресурсы вне Fusion callbacks; только после этого менять API.

Authority/tick/render правила уже видны в state machines; retarget rollback bug
не доказан. Кратко описать существующие правила полезно независимо от тестов,
но нельзя под этим пунктом перестраивать networked layout, менять register order
или добавлять IClock. Цена ошибки высокая: сетевой lifecycle общий для Player/Enemy.
Приёмка исследования — реальные Fusion spawn/despawn/partial-init tests в
поддерживаемом mode и конкретный воспроизводимый нарушенный инвариант.

### G6. Feature runtime responsibility

[DebugConsoleFeature:72](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/DebugConsole/Scripts/Init/DebugConsoleFeature.cs:72)
делает Resolve factory на каждом CreateConsole, включая event callback.
Меньшая правка — получить factory при Initialize и хранить вместе с уже
полученными system/controller dependencies. Это устраняет скрытый runtime lookup
без двух новых объектов lifetime.

Подписка и отписка у Console/Visualization уже парные:
[Console.OnDestroy:16](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/DebugConsole/Scripts/Init/DebugConsoleFeature.cs:16),
[Visualization.OnDestroy:17](D:/Repositories/TroughGame/Assets/ProjectCore/Contexts/Template/Features/DebugVisualization/Scripts/Init/DebugVisualizationFeature.cs:17).
Фичи небольшие; orchestration запуска и выключения собственной capability
согласуется с их ownership. Текущие Resolve происходят в composition/initialization
объекте, поэтому квалифицировать любое их присутствие нарушением DI неверно.

Coordinator позволит unit-test lifecycle без GameObject, но одновременно
потребует явно решить Zenject Dispose vs Unity OnDestroy order. Текущий cleanup
не доказан ошибочным. Вернуться к coordinator, когда появится сложная async
startup/shutdown последовательность, несколько владельцев или конкретные
проверки, для которых меньшая правка недостаточна. Не делать это стандартом Feature.

### G7. Documents and validator

Подтверждённые несогласованности:

- [ARCHITECTURE_RULES:292](D:/Repositories/TroughGame/docs/ARCHITECTURE_RULES.md:292)
  говорит о default runtime assembly при существующем ProjectCore.Runtime.asmdef.
- [Views:666](D:/Repositories/TroughGame/docs/ARCHITECTURE_RULES.md:666)
  требует MonoBehaviour, тогда как
  [CONVENTIONS:161](D:/Repositories/TroughGame/docs/CONVENTIONS.md:161)
  и настоящий BaseScreenView допускают VisualElement.
- [Get-TypeInfo:85](D:/Repositories/TroughGame/Tools/Architecture/Validate-Architecture.ps1:85)
  обрезает namespace у base type, а
  [typeMap:712](D:/Repositories/TroughGame/Tools/Architecture/Validate-Architecture.ps1:712)
  хранит primary types по короткому имени. Исправление только ключа без base
  resolution недостаточно.

Независимый эксперимент: во временном ProjectRoot созданы два
ReviewFixtureComponent, в GameCore — MonoBehaviour, в Prototype — plain class
под Views/Components, каждый в своём правильном namespace. Strict сообщил
**2 files, 0 errors, 0 warnings**. После удаления GameCore counterpart для того
же Prototype файла он сообщил **VIEW001, 1 error**. Это воспроизведённое
подавление ошибки инструмента. Эксперимент не утверждает, что одноимённые
текущие PlayerFeature уже породили конкретную неверную диагностику.

Минимум — исправить согласованность документов, namespace-aware base/type
resolution или явную диагностику неоднозначности, добавить positive/negative
fixtures. Можно сохранить простой инструмент, если он честно сообщает, что
не способен разрешить тип. Semantic dependency report — отдельная более
дорогая задача, не prerequisite этих исправлений.

Полноценный feature-internals analyzer требует решения G3 и корректной модели
using/aliases/generics/composition roots. Начинать его как большой P2 только
потому, что зелёная structural check не доказывает всю архитектуру, невыгодно.
Документация должна честно перечислять действующие проверки. Формулировку плана
«зелёная проверка означает проверку заявленных границ» сузить до проверяемых
инвариантов: ни статический инструмент, ни компиляция не доказывают всю архитектуру.

## Рекомендуемый минимальный порядок работ

1. **Восстановить проверку текущего состояния.** Исправить PlayerPrefabTests paths;
   запустить существующие relevant Editor tests. Согласовать две конкретные
   документальные ошибки; воспроизведённый typeMap defect закрыть fixtures.
2. **SceneFlow finally.** Сохранить transition lock до cleanup, не маскировать
   первичный исход, проверить Hide failure и concurrent second request. Existing
   startup/scene-recreate Play test оставить как successful-path regression;
   он не покрывает faults/cancellation.
3. **Player prefab preflight.** Отвергать неправильный зарегистрированный prefab
   до Spawn; проверить successful Player spawn и отсутствие оставшегося объекта
   после ошибки в Fusion session. На этом этапе общий factory helper не нужен.
4. **Ранние локальные проверки конфигов.** Player spawnpoints и конкретные
   duration/references из затронутых компонентов. Readiness ставить в подходящей
   точке владения данными, а не в едином искусственном validation layer.
5. **ScreenNavigation failures.** Сделать до подключения активных экранов;
   если берём сейчас — ограничить инвариантами hierarchy/history и lifetime.
6. **Один небольшой AI рефакторинг, если нужен именно архитектурный пример.**
   Связанный steering result; processor лишь вместе с полезными сценариями.
   Не брать L2 автоматически. При работе с entity read API объединить локальную
   G3 и L3, без двух последовательных интерфейсных миграций.

Остальные задачи остаются backlog с условиями, указанными выше. Ни увеличение
числа классов, ни их более «архитектурные» названия не являются acceptance criterion.

## Дублирование и зависимости исходного плана

- **L4/L5 + G4** — исправления поведения и короткий общий контракт, а не три
  независимые переработки. Cleanup policy лучше формулировать на выполненном примере.
- **L1/L2 + G2** — передача SO data не должна одновременно превращаться в
  общепроектные frozen snapshots. Сначала сохранить live-tuning semantics.
- **L3 + G3** — один пересмотр существующего read accessor, если нужен. Новый
  query interface перед оптимизацией и второй interface после неё создают лишний churn.
- **G1 + G5** — ошибка prefab отличается от partial component Init failure.
  Preflight решает первое; не доказывает завершённость entity lifecycle.
- **G6 + G4** — coordinator сам по себе не решает поздний async result Console.
  Для этого важен контракт owner cancellation/generation, если сценарий существует.
- **G3 → расширенная G7** — строгая проверка internals невозможна без устойчивых
  публичных контрактов и исключений composition. Простые исправления G7 этой
  зависимости не имеют.
- Порядок плана **L1 перед active failure branches** не следует из необходимости.
  Он допустим как учебный пример по выбору пользователя, но не как риск-ориентированный
  порядок улучшения проекта.

## Что считать достаточным подтверждением отложенных решений

| Задача | Триггер возврата | Достаточное доказательство пользы |
| --- | --- | --- |
| L2 | Новые retarget/dash правила или неудобные тесты policy | Содержательные boundary tests без Runner и более понятный state caller при умеренном размере входа. |
| L6 | Вторая UI поверхность, Recorder lifecycle или расширение metrics | Один provider обслуживает реальные потребности и убирает дублирование/сложный sampling. |
| L7 transport | Измеренная задержка startup/cancel или требование к network UTC | Тест deadline/cancellation и наблюдаемая причина failure без потери offline startup. |
| L3 spatial index | Profiler показывает entity scans как существенный hot path | Измеримое улучшение времени/GC на поддерживаемом масштабе без изменения tie/order semantics. |
| G1 shared helper | Совпадающий механизм нескольких factories после preflight fixes | Уменьшение duplicated failure handling без смешения authority/modes/payload policy. |
| G5 lifecycle API | Partial-init/resource leak или подтверждённый reuse case | Реальный нарушенный invariant в поддерживаемом Fusion mode и один ясно определённый cleanup owner. |
| G6 coordinators | Сложный runtime lifecycle или проверки, которые невозможно удобно реализовать меньшей правкой | Контролируемый lifetime и meaningful toggle/dispose tests; дополнительное владение не появляется. |
| G7 semantic tooling | Повторяющиеся cross-context/feature boundary нарушения | Analyzer ловит конкретные нарушения и проходит разрешённые integrations без broad suppressions. |

Такой сокращённый объём сохраняет исходную feature-архитектуру и даёт проверяемый
результат. Полная реализация всех 14 предложений сейчас принесла бы дополнительную
сложность, часть которой ещё не имеет реального потребителя.
