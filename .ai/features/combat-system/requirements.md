# Боевая система — контракт требований

Ревизия: 6 от 17.10.2026. APR-010 фиксирует решения владельца: Dodge принадлежит Movement; обычный выход только после полного Recovery без окна раннего перехода; используется существующий сетевой таймер движения; неуязвимость — отдельный сетевой эффект с конечным сроком; общий Input-буфер независим. История ревизии 5 сохранена в [requirements-r5.md](requirements-r5.md). Согласование поведения не является приёмкой новой реализации или точной заменяющей архитектуры.

## Цель и границы

Реализовать полный боевой цикл Technical Prototype: действие, обнаружение попадания, авторитетный урон, здоровье, реакции, смерть и перезапуск с согласованным отображением у 1–4 игроков.
Фазы и hitbox составляют первую проверяемую часть полной фичи. План FEAT-01–FEAT-05 ревизии 1 утверждён (APR-002); архитектура FEAT-01 ревизии 7 утверждена (APR-003).
Разработчик включил подготовку и интеграцию placeholder-анимаций трёх атак игрока, атак врагов и смерти, а также прототипный матч-флоу. Механика смены атак остаётся соседней фичей и не считается автоматически завершённой.

## Требования и критерии приёмки

RQ-01–RQ-13 и AC-01–AC-13 сохраняются с изменением RQ-04/AC-04 по APR-010 и границ ответственности Dodge. Результаты прошлых проверок относятся к своим снимкам реализации; проверок новых правил пока нет.

Уточнение RQ-01 / AC-01 (APR-003): один слот хранит входное намерение конечное настраиваемое время, исходно 0,15 с от последнего принятого нажатия. Новое нажатие заменяет команду и перезапускает её таймер. Блокировка, фаза действия, удержание и отсутствие input frame не останавливают и не продлевают таймер. В окне либо после полного завершения исполняется только ещё не истёкшая команда; истёкшая команда требует нового нажатия. Направление следующей атаки по-прежнему определяется последним направлением ввода.

| Требование | Поведение | Источник | Критерий приёмки |
| --- | --- | --- | --- |
| RQ-01 | Атака проходит preparation → active → recovery. С первой фазы до завершения/отмены движение и поворот заблокированы. Ввод продолжает собираться. Буфер имеет один слот: последнее действие заменяет предыдущее, дополнительных приоритетов нет. Ранний переход допускается только в определённой части recovery — cancellation/combo window. Следующая атака использует новое направление из ввода. | S1, S2; DEC-001, DEC-006 | AC-01: locks действуют; вне заданного recovery-окна ранний переход невозможен; внутри окна выполняется последнее буферизованное действие без приоритетов и с новым направлением. |
| RQ-02 | Hurtbox и hitbox различаются; зоны атак активны только в заданных фазовых окнах; цель получает одно попадание за execution без специально заданной многоударности. | S1, S2 | AC-02: preparation/recovery не поражают, active поражает; длительное пересечение не повторяет цель, несколько целей обрабатываются. |
| RQ-03 | Host/State Authority подтверждает попадание и единственное изменение HP, проверяет живость, команду, повторность и неуязвимость; friendly fire отсутствует. | S3 | AC-03: дубликаты, мёртвые/неуязвимые цели и союзники не теряют HP; подтверждённый допустимый hit изменяет HP однократно. |
| RQ-04 | Dodge — состояние движения с preparation/active/recovery. Движение Dodge и его неуязвимость действуют только в active. Обычный выход только после полного recovery, без ранних переходов. Неуязвимость — отдельный сетевой эффект с конечным сроком, связанный с движением через медиатор. | APR-010; DEC-010, DEC-011 заменяют прежнюю часть DEC-002 про окно Dodge | AC-04: preparation/recovery не дают неуязвимость; active предотвращает урон и разрешает движение Dodge; выход до полного recovery по вводу невозможен; эффект истекает в срок даже без завершающего уведомления, а сброс очищает его. |
| RQ-05 | Hitstop — часть анимации атакующего. Hitstun цели блокирует движение и действия; период до следующего допустимого hitstun задаётся для каждого существа и отсчитывается с начала наложенного эффекта. Он независим от длительности hitstun. Для слабых врагов допускается постоянный hitstun при подходящих параметрах атак; игроку задаётся большой период, препятствующий постоянному стану. Дополнительный impact: тряска цели и короткий белый flash. | DEC-002, DEC-007, заменяет часть S1/S4 | AC-05: эффект и отсчёт стартуют вместе; до истечения периода новый hitstun не накладывается; после истечения может накладываться, даже если предыдущий ещё не завершён. Урон и защита от повторного hitstun не смешиваются с неуязвимостью. Shake/flash и анимационный hitstop воспроизводятся согласно принятым правилам. |
| RQ-06 | Hitstun прерывает действие; защищённых действий сейчас нет, их механика отложена. | DEC-002, уточняет S1/S4 | AC-06: прерывание не оставляет активных зон; восстановление управления соответствует состоянию существа. |
| RQ-07 | При 0 HP прекращаются движение, команды и бой; враг показывает placeholder death и удаляется, игрок остаётся неактивным; после полного wipe возможен restart. | S1, S3 | AC-07: dead не действует; restart очищает старые цели, зоны и эффекты и восстанавливает игру согласно принятому сценарию. |
| RQ-08 | HP отображается над персонажами; trail является частью анимации атаки; есть shake/flash. Звуки отложены. Смерть имеет отдельные анимацию и состояние. Различие игроков сейчас не обязательно; различие атак обязательно. | DEC-002, DEC-004, заменяет обязательный SFX из S1/S4 | AC-08: HP UI, trail, shake/flash и смерть интегрированы; подтверждённый hit не создаёт повторную обратную связь из-за сетевой коррекции. |
| RQ-09 | Полный pipeline и отображение согласованно работают у 1–4 игроков. | S1–S4 | AC-09: выполнены сценарии Solo/2/4 с записанными версией, ролями и результатами Host/клиентов. |
| RQ-10 | Массовые попадания работают без критических проблем; система допускает дальнейшее расширение без наполнения будущего контента. | S1 | AC-10: несколько атакующих/целей не вызывают непредусмотренного повторного урона, зависания или рассогласования; условия испытания записаны. |
| RQ-11 | Атаки игрока: короткий удар с квадратной зоной, повёрнутой углом вперёд; круговая атака; узкий длинный укол. Оба типа врагов используют короткую фронтальную круглую зону, Dash Enemy также атакует рывком. Зона рывка следует за визуальной частью врага; lifecycle атаки не зависит от логики/таймингов рывка, общий источник параметров допускается. | DEC-003 | AC-11: зоны соответствуют принятой форме/направлению; движущаяся зона следует за визуальной частью врага; атака не завершается и не переключает фазы по неявной зависимости от состояния движения. Точное active-окно уточняется. |
| RQ-12 | Агент генерирует кадры placeholder art/анимаций, готовит и интегрирует их под принятые тайминги. Атаки различимы: взмах мечом, вращение с огненным следом, укол ледяным копьём с морозным следом. | DEC-004 | AC-12: подготовленные кадры и анимации включены в игру, синхронизированы с фазами; все три атаки визуально различаются. Наличие исходных картинок без интеграции недостаточно. |
| RQ-13 | Host по хоткею открывает окно матча; Start начинает периодический спавн. Полный wipe останавливает спавн и показывает Host Restart. Restart удаляет оставшихся врагов, возвращает всех игроков на стартовые позиции с полным HP и оставляет матч в ожидании: нужны повторные хоткей и Start. | DEC-005, DEC-008 | AC-13: Host выполняет Start → spawn → wipe/остановка spawn → Restart/очистка врагов и восстановление игроков → ожидание без spawn → повторный Start; клиенты не управляют Host-командами матча. |

Исключения: knockback, полноценная контентная система комбо (окно раннего перехода входит), parry/block, crit, armor/resists, damage types/status effects, лечение/индивидуальное revive, friendly fire, финальный арт/баланс, звуки и защищённые действия. Возрождение всех игроков кнопкой Restart входит в объём.
Числовой порог производительности для AC-10 пока не задан; этот критерий не заменяет отдельную приёмку масштаба толпы.
Пример владельца про оружие/билды обосновывает параметризацию hitstun, но не добавляет реализацию системы билдов в объём Combat.

## Предлагаемая первая часть

Общий lifecycle, фазовые разрешения, зоны и исключение повторного hit по одной цели, передача обнаруженного попадания обработчику и минимальная отладочная видимость на одной тестовой атаке.
Полные HP, урон, реакции, смерть и feedback сохраняются в полном объёме Combat, но не предлагаются для первой части.
Граница принята в плане APR-002; точная архитектура первой части — FEAT-01 ревизии 7, APR-003.

## Рабочие параметры для последующего согласования

Принятые правила включены в RQ-01–RQ-13; история решений сохранена в [decisions.md](decisions.md). Открытых смысловых вопросов по обсуждённым решениям нет.

Приняты параметры FEAT-01: тестовая атака 0,20 / 0,10 / 0,30 с и окно Recovery [0,15;0,25) с; Dodge 0,03 / 0,20 / 0,10 с без окна раннего выхода (APR-010); буфер 0,15 с. Тестовый хитбокс — квадрат 0,8×0,8 со смещением вперёд 0,65 и поворотом 45°; hurtbox — круг r=0,25 со смещением (0;0,20). Полная запись рабочих значений — в subtasks/FEAT-01.md, ревизия 7.

Для последующих подзадач остаётся подобрать и согласовать:

- длительности и границы окон остальных атак;
- размеры и активные интервалы остальных зон, включая атаку рывком;
- конкретные HP и урон под ориентиры около 20 попаданий для игрока и 5 для врагов;
- длительности hitstun, периода до повторного hitstun, анимационного hitstop, shake и flash;
- параметры кадров и анимаций, ресурсный план и визуальный прототип;
- хоткей окна матча, периодичность спавна и рабочие параметры испытаний.

Эти оставшиеся значения пока не утверждены; параметры FEAT-01 приняты отдельно выше.

## Источники и снимок

Прочитаны 15.10.2026 по Europe/Kiev (fetch 2026-10-14T21:30:02Z).
Хэш нормализованного enhanced Markdown: LF, без начального fetch/as-of header.

| ID | Источник | Наблюдаемая версия | SHA-256 |
| --- | --- | --- | --- |
| F | [Боевая система](https://app.notion.com/p/3f6ad1247d588189829fc59894bd7a34), `3f6ad124-7d58-8189-829f-c59894bd7a34`, Technical Prototype, порядок 3, Каркас | 2026-10-12T08:56:23.299Z | 06eb48fbdf8532b2e37d7e4c774b8996d3469401d1e42f93c3a5cc3dc25199b6 |
| S1 | [Спецификация Combat](https://app.notion.com/p/3f6ad1247d5881fabb7dcf08fc68a3b9) | 2026-10-12T08:56:18.723Z | bf9f3a13e0afe5b5f656ea3b714e4e12e8d114f34625b3f6d2cef17e041a1140 |
| S2 | [Фазы и hitbox](https://app.notion.com/p/3f6ad1247d5881a78d70ea52dd4d5802), In progress; motor/input Done | 2026-10-12T17:30:47.137Z | 9655745b3a5e223c918acac0cab99dfe4881c0aea0e690533e214a9f696172b0 |
| S3 | [Урон, здоровье, смерть](https://app.notion.com/p/3f6ad1247d5881b28d4ff480877d9339), Ready, зависит от S2 | Не передана в кратком результате | Не передан в кратком результате |
| S4 | [Реакции и feedback](https://app.notion.com/p/3f6ad1247d5881709687eea55039211d), Ready, зависит от S2/S3 | Не передана в кратком результате | Не передан в кратком результате |
| S5 | [GD атак](https://app.notion.com/p/3f7ad1247d5881dcb68ce846e2e2fa41) | 2026-10-12T14:46:28.193Z | 033c6e89f90ffd9f003976210560da98d8d5714178717d503817f7bf37cedcda |
| S6 | [GD визуальных правил](https://app.notion.com/p/3f7ad1247d588135a99ae694a37bde18) | Не передана в кратком результате | Не передан в кратком результате |
| D | [Принятые решения](https://app.notion.com/p/3f6ad1247d5880678a15ef7c5aa7a04e) | 2026-10-12T08:56:14.632Z | 0f10d914dfae3f37c39a0dd2a97a858e8b717e3e35dbcf87b79c490ab6cdb465 |

Полная выборка задач: 29 строк, has_more:false; зависимости перечитаны. Native verification: unverified; truncation/unknown blocks не предоставлены. Эти ограничения не означают доказанной полноты блоков. В карточках Dev фактические испытания не записаны.

## Обновление снимка перед реализацией контрактов

Изолированный Notion-агент повторно прочитал S1 и S2 15.10.2026: содержательных изменений относительно принятой ревизии 7 нет. S1: версия `2026-10-15T16:47:29.805Z`, SHA-256 `b32d1d2e0eb1b9aaf1eac231cb4f222f356ea0f65daf049540460242bf0871fb`. S2: версия и хэш совпадают с таблицей выше. У задачи остаётся In progress; новых фактов испытаний в источниках нет. Явные метаданные полноты/неизвестных блоков не предоставлены.

## Согласование

DEC-001–DEC-005 приняты прямым сообщением разработчика с ответами Q-001–Q-005 от 15.10.2026. DEC-006–DEC-008 приняты следующим сообщением с ответами на дополнительные Q-001/Q-002/Q-005. Ревизия 4 принята APR-001; план ревизии 1 — APR-002. Последующее сообщение «теперь дальше работаем по этому контракту» и согласие с тестовыми значениями принимает архитектуру FEAT-01 ревизии 7 (APR-003), включая уточнение буфера в текущей ревизии требований. Ревью реализации, ресурсы последующих подзадач и результаты испытаний ещё не приняты.


## Source Refresh — 2026-10-16, Revision 9 Implementation

Read-only operation `combat-r9-source-refresh-20261016`, request SHA-256
`b963944b6e8c6fbd4cecc043282e6da00aa55a4daba91f6fabdafca09ca235cd`.
Both target timestamps matched the supplied baseline; no newer behavioral,
acceptance or editorial changes were observed. Baseline hashes were unavailable,
so byte-identical historical content is not claimed. No remote writes.

| Source | Observed version | Normalized content SHA-256 |
| --- | --- | --- |
| Combat specification, 3f6ad124-7d58-81fa-bb7d-cf08fc68a3b9 | 2026-10-16T14:04:41.340Z | a6e1022ab2ee584ff383af89ce86b7268cfb070910475566a942cfdab22d1cc7 |
| Phases/hitbox task, 3f6ad124-7d58-81a7-8d70-ea52dd4d5802 | 2026-10-12T17:30:47.137Z | edcc89b0c0858f281576e8adacbb58930f41804c5cf3cc993aa829525f0de5f3 |
| Live documentation standard v1.2 | 2026-10-12T14:41:35.562Z | 6f4804e45f99d5c0ae2224e806f658b26b000e47b18a8cc3dbcdfc53f6e521da |
| Workflow | 2026-10-12T14:41:35.016Z | 4730d5c9f7ea776ed91d636d9776f4ba4d3bcab4fe8ce534c6e15da5f16970bf |

Revision 9 availability is approved locally by APR-009 and is not yet present in
the fetched Notion snapshot. R8 recorded agent checks remain historical evidence,
not owner diff/Play acceptance. Task In progress and feature skeleton state remain.
Fetch completeness metadata for truncation/unknown blocks was unavailable; the
worker observed closed wrappers with no visible omissions.


## Read-only Source Refresh — Revision 10 Proposal, 2026-10-17

Operation combat-r10-source-refresh-20261017, request SHA-256
b71467d95706efe6b5202758fb780ccd8a04be27255cf5e9182bde57fc3a8c57.
No new product/acceptance changes; specification version 2026-10-16T21:29:54.161Z,
task version 2026-10-12T17:30:47.137Z match baseline. Latest remote evidence is R9
availability and agent Solo6/6; owner full diff/Play and network coverage pending.
R10 is owner local architecture feedback, not a behavioral contract change or
exact implementation approval. Detail/normalization/limitations in the R10 proposal.
No remote writes. Product requirements and five-subtask plan remain unchanged.


## Accepted Revision 6 Responsibility Decisions — 2026-10-17

APR-010 is the current owner instruction in the main chat, including the preceding
Dodge ownership message and latest points 1–4. It accepts this behavioral delta,
not an unseen replacement implementation/API or complete diff/Play acceptance.

- DEC-010: Movement owns Dodge configuration, all three phases and normal completion.
  Use the existing network movement state timer; no second Dodge duration timer.
  Remove early Dodge transition/cancellation window; attack windows remain.
- DEC-011: Dodge notifies the existing mediator about its active movement interval;
  mediator connects this to a separate finite network invulnerability effect.
  Expiry does not require receiving a matching off notification. Existing death/
  reset/Restart cleanup requirements remain. Combat/State Authority validates hits.
- DEC-012: one independent Input buffer remains, latest command only, existing
  lifetime and no new priorities. A queued command executes after full Dodge only
  if unexpired. This does not add a separate Combat or movement command buffer.

Existing owner choices also stand: mediator translates attack notifications to
forced movement-state entry; Attack Enter owns zero velocity; entity root owns
position independently from MovementSimulation. The new Dodge direction supersedes
unaccepted R10 suggestions where Combat provided Dodge phases to movement.

Detailed source findings and architecture-preparation boundaries:
[subtasks/FEAT-01-movement-dodge-decisions-r11.md](subtasks/FEAT-01-movement-dodge-decisions-r11.md).
