# Combat Feature Decisions

## Authorization and Baseline

The developer requested starting the full Combat feature workflow on 2026-10-15.
This authorizes intake and requirements preparation. The developer subsequently
accepted the product decisions below through explicit Q-001–Q-005 answers.
Requirements revision 4 was then accepted by the developer's explicit
continue message (APR-001). Initial plan revision 1 was subsequently approved
(APR-002). No local architecture, implementation review or Play Mode acceptance
has been recorded.

The repository was clean on branch `main`, HEAD
`d9d3914e6b54c62659b0cdeacbcbf18d12dec90b` before workflow files were created.
Only the feature template existed; no Combat workflow was available to resume.
Visible stashes were recorded without applying or modifying any stash.
The tracked-diff fingerprint hashes UTF-8 staged diff followed by unstaged diff;
both were empty at baseline. Workflow files are the only expected new paths.

## Proposed Boundary

The full requested feature remains Combat. General phases and hitbox are a
proposed first deliverable, not a replacement for damage, reactions, death,
restart, feedback, and network verification. This proposal is not an approved
subtask plan. Product decisions are recorded in `requirements.md`.

## Accepted Owner Decisions

Approval source for DEC-001–DEC-005: the developer's explicit message dated
2026-10-15 answering Q-001–Q-005. Publication was authorized by the preceding
message requiring every agreed decision to be recorded in Notion.
The following Russian text is actual project decision content.

| ID | Status | Decision | Impact |
| --- | --- | --- | --- |
| DEC-001 | approved | Атака блокирует движение/поворот с начала preparation. Ввод и направление буферизуются. Следующее действие может запуститься в recovery cancellation/combo window, следующая атака использует новое направление. | Заменяет агентское предложение ждать полного recovery и игнорировать ввод до recovery. Уточнение буфера/окна: DEC-006. |
| DEC-002 | approved | Игрок выдерживает около 20 ударов, оба типа врагов около 5; числовой баланс временный. Hitstop — анимация атакующего; цель получает hitstun, shake и белый flash. Dodge имеет 3 фазы, неуязвимость только active и окно отмены в recovery. У существа есть период защиты от повторного hitstun. Защищённых действий сейчас нет. | Заменяет hitstop обоих участников; отсчёт уточнён DEC-007. |
| DEC-003 | approved | Игрок: короткая квадратная зона углом вперёд, круговая атака, узкий длинный укол. Оба врага атакуют короткой круглой зоной впереди; Dash Enemy также атакует движущейся зоной за визуальной частью во время рывка. Lifecycle атаки не зависит от логики/таймингов рывка; общий источник параметров разрешён. | Формы/контент приняты. Окно активности движущейся зоны уточняется; соседняя смена атак не завершена автоматически. |
| DEC-004 | approved | HP UI над персонажами, trail в анимации, shake/flash, без звука. Смерть — отдельные состояние и анимация. Три визуала: взмах мечом, вращение с огненным следом, ледяной укол с морозным следом. Агент генерирует кадры и готовит/интегрирует placeholder-анимации под тайминги; это обязательный AC. | Расширяет подготовку ресурсов и их интеграцию; отменяет обязательный SFX. Визуальный прототип и ресурсный план ещё не приняты. |
| DEC-005 | approved | Host открывает окно матча хоткеем. Start запускает периодический spawn. Wipe показывает Host окно Restart. Restart оживляет игроков без автоматического начала матча; нужны повторные хоткей и Start. | Добавляет прототипный match flow/UI. Очистка/остановка уточнены DEC-008. |
| DEC-006 | approved | Ранний переход только в определённой части recovery. Буфер имеет один слот с последним действием, новое заменяет старое; дополнительных приоритетов нет. | Источник: прямой ответ разработчика на дополнительный Q-001 от 15.10.2026. Существование такого буфера в коде не проверялось; это продуктовый контракт. |
| DEC-007 | approved | Отсчёт до следующего допустимого hitstun начинается при наложении hitstun, период задаётся каждому существу независимо от длительности эффекта. Слабые враги могут удерживаться в постоянном стане подходящими атаками; игрок имеет большой период до повторного hitstun. | Источник: прямой ответ разработчика на дополнительный Q-002 от 15.10.2026. Не требует гарантированной паузы после каждого hitstun для всех существ. Пример билдов не расширяет текущий объём до системы билдов. |
| DEC-008 | approved | Wipe останавливает spawn; Restart удаляет оставшихся врагов, возвращает игроков на исходные позиции с полным HP и оставляет матч ждать нового Start. | Источник: явное «Q-005 — да» к точному предложению в предыдущем ответе агента, 15.10.2026. |
| DEC-009 | approved | При разработке архитектур предусматривать минимум два независимых участка для эффективной параллельной реализации разными сабагентами и корректного объединения. | Источник: прямое дополнение разработчика к принятию плана от 15.10.2026. Общие контракты/границы фиксируются до реализации; FEAT-план не дробится автоматически. |

## Preserved Boundaries

Existing unrelated Enemy sprite imports are developer work and remain untouched.
Generated art, UI, runtime changes, and implementation subtasks have not started.
Accepted product changes do not imply tests passed, tasks completed, or feature
status changed. Further clarification stays in this same feature workflow.

## Published Source Observation

The dated owner-decision section was appended to the existing
[Combat specification](https://app.notion.com/p/3f6ad1247d5881fabb7dcf08fc68a3b9).
Remote postwrite version: `2026-10-15T09:32:41.861Z`. Readback matched all five
decision paragraphs, stated remaining questions, and acceptance updates.
Historical content and page properties were preserved. No task was closed.
Later clarification of the remaining questions must be recorded separately.

The accepted clarifications DEC-006–DEC-008 were subsequently appended to the
same specification. Postwrite version: `2026-10-15T09:42:13.677Z`.
Actual readback matched the complete clarification text and acceptance updates;
historical content and properties were preserved. All semantic Q-001–Q-005
questions are now answered. Working numerical parameters and full-contract
approval remain separate from these accepted decisions.

## Requirements Gate and Research Handoff

APR-001 accepts requirements revision 4, including RQ-01–RQ-13 and AC-01–AC-13,
from the developer's message: «Окей продолжаем тогда двигаться дальше» after
the revision 4 cleanup. Working numerical and resource parameters remain to
be agreed at the applicable architecture/resource gates before implementation.

The acceptance and research start were recorded in the Combat specification;
verified postwrite version: `2026-10-15T10:08:04.160Z`. Fresh source read
showed no unexpected drift from the previously published clarifications.
No Notion task or development-status property was changed.

Initial project research produced proposed plan revision 1 in `research.md`.
Its five FEAT labels are proposals only; state has no instantiated subtasks.
The runtime build passed with 8 warnings. Strict architecture validation
failed on four pre-existing untracked Enemy image names. These user assets
are preserved and remain outside the proposed changes. No runtime code,
prefab, scene, animation or UI asset has been modified during research.


## Plan Acceptance and Architecture Preparation

Plan revision 1 acceptance and DEC-009 were also published to the Combat
specification and verified by actual remote readback, postwrite version
`2026-10-15T11:52:07.828Z`. Source drift was absent; no task/status was changed.

FEAT-01 architecture revision 1 is now proposed in `subtasks/FEAT-01.md`.
Two read-only inspections were incorporated: explicit proxy mutation guards,
retained-Dodge eligibility at ingestion, command/profile validation, and preserving
the contact ledger on a skipped replay tick. No implementation workers have started.
Architecture proposes two independent implementation packages and main-owned
contracts/actor/DI/Unity integration, as requested by the developer.

## Revision 7 Acceptance and Current Handoff

APR-003 records the developer's main-chat instruction of 2026-10-15:
«теперь дальше работаем по этому контракту. С предложенными тестовыми балансными
значениями и таймингами согласен». The inspected FEAT-01 artifact was architecture
revision 7. This accepts the updated architecture and its prototype values;
implementation review, Play Mode acceptance and feature readiness remain pending.

Revision 7 supersedes the revision 1 proposal recorded earlier in this history:

- The existing one-slot buffer retains an input intent for a finite configurable
  lifetime, initially 0.15 s. Expiry continues while locked and without an input
  frame; only a new accepted press replaces input and restarts the deadline.
- Reuse InputBufferCommandDescriptor, IInputBufferController and
  InputBufferController unchanged. No new buffer interface or descriptor peek.
- Pack the command ID and lock into one byte in InputBufferStateModel, keeping
  TickTimer separate. Command IDs are 1..127; 0 is empty. Actual Fusion layout
  must be verified; bandwidth savings are not an accepted or measured result.
- ICombatActionStateAccessor and ICombatActionSnapshotAccessor feed
  ICombatActionAccessor, inherited by ICombatActionMutator. All resolve to one
  player root; the snapshot is derived, not separately writable.
- Input CommandId is independent from gameplay ushort ActionId. One Attack
  intent covers all attack variants; the executor selects the concrete profile
  at a permitted start, with consumption by CommandId and execution by ActionId.
- The A/B/main ownership and lifecycle/authority checks in revision 7 are accepted.
  Test timing/geometry values in that revision are accepted as prototype settings.

Requirements revision 5 incorporates the finite-lifetime RQ-01/AC-01 clarification,
updates outdated planning/approval statements and preserves the other RQ/AC.
The workflow transitions to implementation. No runtime changes or tests were
performed as part of this acceptance reconciliation. Notion publication is
reported separately only after verified worker readback.

The isolated Notion documentation worker verified publication of these accepted
facts to the existing Combat specification. Receipt:
`combat-feat01-rev7-acceptance-20261015-v2`, request SHA-256
`a6d86a87ce48bc8e4260ffb4f64f0333ce6cfd6c74198efd280b38fea950b21c`.
Prewrite source matched `2026-10-15T11:52:07.828Z`; no unexpected material drift.
Postwrite source version: `2026-10-15T16:47:29.805Z`; fresh remote readback at
`2026-10-15T16:47:46Z` matched the exact decision text and one publication marker.
Historical content and properties were preserved; no task was completed.
Notion standards remained version 1.2 at their configured observed timestamps.
Native page verification/completeness metadata remain unavailable; visual QA
was not performed. These are publication limits, not gameplay test evidence.

## Developer Review Pack 1 — Architecture Revision 8 Proposed

On 2026-10-16 the developer supplied ten corrections/questions about reset and
readiness ownership, repeated validity checks, attack configuration, implicit
validation, a test-only contact event, component ID/position access and naming.
These are review feedback, not acceptance of the complete revision 1 diff.

Read-only source research confirms the event currently has only a test subscriber;
the discarded shape creation is validation by exception; and movement Position
already includes Offset, so directly substituting it would double the hurtbox's
vertical offset. Local architecture revision 8 proposes concrete ownership/API/
serialization/lifecycle changes in subtasks/FEAT-01-review-corrections-r8.md.
Only the new material architecture choices await approval; exact rename directions
are already developer requests. The five-subtask plan and accepted game behavior
remain unchanged. No production/Unity asset edits, tests, Notion publication or
commit occurred in this preparation turn. Prior test results belong to revision 1.

Git reconciliation found 112/113 review-manifest files unchanged. The remaining
CombatHitProbeComponent.cs difference is indentation only at line 33; preserve it.
Historical approved revision 7, common-contract acceptance, BUG-001 evidence and
the review revision 1 manifest/patch remain unchanged. The host returns to
subtask_architecture, waiting_for_approval, within the same FEAT-01.

## Archived Initial Proposal

## BUG-001 Approval, Retest and Combined Review Handoff

APR-005 records the developer's explicit approval on 2026-10-16:
«Подтверждаю, делай фикс бага и продолжай реализацию». The approved correction
was applied to the input fixture and only new empty m_Name whitespace in three
actor prefabs. Existing object formatting, asset values and metadata were preserved.
This instruction also permits continued FEAT-01 implementation after checks;
it does not accept the unseen final diff or required Play Mode.

Original fixture retest passed 10/10. Continued planned coverage added Editor
prefab/phase checks and a Solo contact/despawn/respawn scenario. Full Edit Mode
passed 653/653; automated Solo Play passed 2/2; runtime build passed with 8
dependency warnings; git diff --check passed. The four pre-existing unrelated
ASSET003 image errors remain. Human/manual Host/client acceptance is not claimed.
Combined audit found no P0/P1; AUD-001 is P2 for per-query allocations (unprofiled).

Notion operation `combat-feat01-bug001-review-ready-20261016` completed with fresh
readback, one marker and preserved history/properties, remote postwrite version
`2026-10-15T21:40:41.580Z`. Request SHA-256:
`3caa49f8a42e96408633cba8c6e0e7ba2b2babddb324b49adff31d086e64bcfb`.
The publication explicitly distinguishes agent checks from owner acceptance and
does not close tasks or change their statuses.

The host now awaits developer review of combined FEAT-01 review revision 1,
including corrected BUG-001; one eventual checkpoint remains subject to full
review/Play Mode/check gates. No plan delta, FEAT-02 start or commit occurred.

## Common Contract Review Acceptance — 2026-10-16

The developer explicitly reviewed the common-contract implementation slice:
«я провел проверку реализации єтой части feat-1 и все сделано правильно,
можем продожить реализацию». This accepts the previously presented 17 contracts,
packed buffer model and related tests, and authorizes continuation with A/B and
main integration. It does not accept the complete FEAT-01 diff or required Play Mode.
No commit is created before those complete-subtask gates.

On resume the 21 frozen source hashes matched. The additional DebugConsole
PanelSettings working-tree change is unrelated and preserved. Managed isolated
workspaces combat-phases and combat-geometry were prepared from base commit
`d9d3914e6b54c62659b0cdeacbcbf18d12dec90b`, overlaid with exact frozen contracts
and the accepted packing-test baseline. Worker A/B write allowlists are disjoint;
main retains state/Git/Editor/DI/actor integration and actual validation.

Source refresh `combat-feat01-resume-20261016` completed without material drift.
Request SHA-256: `98d8dfa393b3c29cc3c621603271d51bc9e08f343421cdd32b4eb5865a104661`.
Combat specification remains at `2026-10-15T16:47:29.805Z`; task remains at
`2026-10-12T17:30:47.137Z`, In progress. No new remote test claims or writes.

Moved from requirements revision 3 during editorial cleanup at the developer's request. This proposal was not accepted; current requirements and DEC-001–DEC-008 take precedence. The text below is historical project content.

### История предложения временного действия (заменено решением владельца)

Первоначальное агентское предложение ревизии 1 не принято. Правила ниже оставлены только как история; при расхождении применяются DEC-001–DEC-005. Особенно заменены игнорирование ввода в preparation/active и запуск буфера только после полного recovery. Длительности и геометрия из этой таблицы не утверждены.

| Аспект | Предложение |
| --- | --- |
| Запуск | Нажатие Attack в состоянии готовности запускает одну тестовую атаку. |
| Время | Preparation 0,20 с → active 0,10 с → recovery 0,30 с. |
| Направление | Фиксируется при запуске и не меняется до завершения/отмены. |
| Движение | Заблокировано во всех трёх фазах, после завершения/отмены возвращается. |
| Команды в preparation/active | Новая Attack и Dodge не принимаются. |
| Команды в recovery | Одна Attack сохраняется в буфер и запускается после завершения; повторные нажатия не накапливаются. Dodge отменяет recovery и удаляет буфер. |
| Попадание | Простая зона впереди персонажа, активная только в active; каждую цель обнаруживает один раз за запуск. Точная геометрия тестовой зоны согласуется в контракте до реализации. |
| Прерывание | Тестовая атака не защищена; явная отмена отключает зону, очищает буфер и возвращает управление. Полная обработка урона/реакций остаётся последующими частями. |

Геометрия предложенной зоны для принятия: сфера радиусом 0,5 м, центр на 1 м перед персонажем, в центре его hurtbox по высоте. Это временные игровые размеры; разработчик может изменить их вместе с остальными рабочими правилами.

## Architecture Revision 8 Accepted — 2026-10-16

APR-006 records the developer instruction «Подтверждаю корректность изменений. Реализуй правки». This accepts the revision 8 proposal and authorizes its implementation in FEAT-01. Prior revision 1 review/check artifacts remain historical; the complete corrected implementation still requires audit, diff review and required Play Mode acceptance. The connected Editor was unavailable at preflight; no fallback Editor is launched.

## Revision 8 Implementation and BUG-002 Retest — 2026-10-16

All ten approved review corrections are implemented; full source review revision 2
and audit are saved. Frozen common contract hashes and renamed script GUID remain
unchanged. BUG-002 demonstrated late coroutine observation/injection in Recovery;
APR-007 authorized diagnosis, APR-008 authorized the test-only AfterTick correction
and continuation. Three original repetitions, full Solo3/3 and Edit664/664 passed.
Runtime buffer/phase/profile behavior was not changed.

Host now awaits the complete revision 2 diff review and required owner Play Mode/
retest acceptance. P0/P1 absent; existing AUD-001 P2 remains. Strict still reports
four unrelated ASSET003 images; real Host/client/replay/pool scenarios remain
unexecuted. No commit, next-subtask start or plan change. See revision 2 result,
manifest/patch and audit; revision 1 artifacts remain historical.

## Revision 8 Notion Publication Verified

Isolated publication combat-r8-review-ready-20261016-v3 completed. Received request
SHA-256 b9fdc8d470ac343fd4e5f1a715b5d835bbf4088b05616624b8414af34a435486.
Remote append to the existing Combat specification was read back at
2026-10-16T14:04:52Z; page version 2026-10-16T14:04:41.340Z. Exactly one marker,
prior body preserved as prefix, properties identical, expected append matched
after Notion blank-line/native-date normalization. Earlier v1/v2 envelopes failed
local preflight without remote operations; no duplicate writes were replayed.
Accepted architecture and agent check results were published separately from
pending owner diff/Play acceptance; no status/task/feature completion changes.
Verification was textual/structural; explicit truncation/unknown-block metadata
and visual QA were unavailable.

## Availability Contract Revision 9 Accepted — 2026-10-16

APR-009 records the developer acceptance of the concrete availability proposal
with exactly `_origin != null` as hurtbox local validity. Init/reset remain
unconditional; disable/local invalidity only skip callbacks, distinct from real
network invalidation. This authorizes the revision 9 delta in FEAT-01, not the
complete unseen revision 3 implementation or final Play Mode acceptance. No new
FEAT or plan change. Source snapshot revision 2 was unchanged on reconciliation.


## Revision 9 Implementation / Review 3 — 2026-10-17

APR-009 implemented in exactly five production and three test sources. Hurtbox
predicate is only `_origin != null`. New full review snapshot and separate audit
retain historical r1/r2 evidence. Compile and Edit664/664 passed; runtime build
passed with 0 errors/10 dependency warnings; strict baseline retains four ASSET003
images. R9 Play is unconfirmed: interrupted first run, resumed helper Pipeline
editor_status timeout during reload. No test failure is being called a pass.
Audit P0/P1 absent, existing AUD-001 P2 remains. Owner diff/required Play/retest
acceptance pending; no commit or next-subtask start. Five-part plan unchanged.


## Revision 9 Notion Publication Verified — 2026-10-17

Isolated operation combat-r9-availability-progress-20261017 completed with request
SHA-256 aef90f6f5d00c2cfda2fe9ca24c44a769373857d2a2c2656fa8daa34e4997e19.
Specification append read back at 2026-10-16T21:27:20.410Z, page version
2026-10-16T21:27:13.512Z. Exactly one marker; prior body/properties/icon/cover
preserved; append matches immutable request ignoring Notion blank lines. No task,
feature or property/status changes. Contract acceptance and implementation are
distinguished from pending full diff/Play acceptance and unconfirmed R9 Play run.
Live standard v1.2 and destination schema fetched. Completeness/truncation/unknown
block metadata unavailable; textual readback verified, visual QA not performed.


## Completed R9 Solo Play Verification — 2026-10-17

After Editor reload completed, the next connected helper invocation passed all
six Solo scenarios in 24.32 seconds. This supplies the previously missing R9 Solo
verdict; prior interrupted/unavailable attempts are retained. Same-execution probe
disable/re-enable contact dedup, spawned hurtbox availability/geometry, both base
availability policies and original phase/contact/reset scenarios passed. Owner
full diff/required Play acceptance and Host/client/replay/real pool coverage remain
pending. No source changes occurred after the review snapshot or this check.


## R9 Solo Result Publication Verified — 2026-10-17

Isolated operation combat-r9-play-evidence-20261017 completed; request SHA-256
2c5a69fa64b0e69647ff4edad34468e8712c221cb9ca01203684153c688400cc.
Readback at 2026-10-16T21:30:05.158Z, page version 2026-10-16T21:29:54.161Z.
Exactly one new marker; prior body (including unconfirmed attempt history),
properties/icon/cover preserved; append matches request ignoring blank lines.
Actual R9 Solo6/6 and pending owner/network acceptance are explicit. No task or
property/status changes. Completeness metadata unavailable, no visual QA.


## Proposed Architecture Revision 10 — 2026-10-17

Owner review requests independent movement and root-owned entity position. Source
inspection confirms Combat coupling in movement state machine/states/processor,
and body-owned origin bindings used by Combat. The exact proposed revision is
subtasks/FEAT-01-decoupling-r10.md. No production/Unity changes performed. All150
review3 hashes match, index empty. Keep FEAT-01 and five-part plan; awaiting exact
architecture approval, not marking the full prior implementation accepted.


## Revision 10 Direction Corrected by Owner — 2026-10-17

Developer explicitly selected the existing mediator and public forced state
transition, Attack Enter owns zero velocity, and movement processors stop reading
Combat. This supersedes the unaccepted neutral-provider proposal. Existing public
ChangeState already provides Exit/mutation/Enter and same-state re-entry. Exact
completed proposal in subtasks/FEAT-01-mediator-r10.md includes phased Dodge,
completion/cancel and readiness/replay boundaries. Root-owned position direction
is retained. No production changes by agent. One user formatting-only edit (four
trailing spaces in PlayerActionMovementProcessor) inspected/preserved;149/150
review3 hashes still match. No new FEAT or plan change.


## Accepted Dodge Revision — 2026-10-17, APR-010

Owner says Dodge belongs to Movement and explicitly confirms: (1) no early exit,
full Recovery; (2) use existing network movement state timer; (3) finite network
invulnerability effect deadline; (4) independent shared Input buffer. Recorded as
requirements revision6, DEC-010/011/012. Historical revision5 retained separately;
R10 Combat-owned Dodge phase routing superseded. Attack recovery windows unchanged.
Source check confirms timer exists but current TP Dodge phases still use Combat
snapshot. New exact architecture pending, no agent production/Unity changes or new
runtime proof. Five-subtask plan and review/Play gates remain.


## Dodge Decision Publication Verified — 2026-10-17

Isolated update_documentation combat-dodge-movement-decisions-20261017 completed;
request SHA-256 3003016163ebd43e754a7c83352a6fc5f7bfe5a1a5b45fd5bedc9b12e750e131.
Specification version 2026-10-16T23:11:57.412Z, readback at 2026-10-16T23:12:05Z.
Exactly one marker; baseline unchanged before append; original body/properties
preserved and requested content matched after native formatting. No status or task
closure. Accepted rules explicitly distinguished from pending new implementation
and checks. Live standard v1.2/schema verified. Completeness metadata unavailable;
text/table verified, no visual QA. No agent source/Unity edits in this turn.


## R11 Partial Implementation / BUG-003 — 2026-10-17

APR-011 implementation written; compile/Edit674 passed, previous six Solo cases
passed but all three new Dodge cases timeout. Cause unknown; no new diagnostic
probes or production fix authorized/executed. EXP-001 bounded test-only plan stored
under the same FEAT-01 interruption. Review4 partial manifest/patch/result saved,
not readiness or full review acceptance. Meta GUIDs retained; Editor effects after
migration timeout inspected; new empty m_Name formatting restored under APR-005.
No commit/FEAT-02 completion. Existing strict PNG baseline unchanged.


## R11 Partial Result Publication Verified — 2026-10-17

Isolated publish_progress combat-r11-partial-implementation-20261017 completed.
Request SHA-256 6d8e32b88312f8885e89cc69e66c381dd620a8b7b1970c27c64c6436b3bffa39;
specification version 2026-10-17T00:00:30.760Z, readback 2026-10-17T00:00:40Z.
Worker verified one marker, exact supplied partial result after native blank-line
normalization, prior body preserved and properties unchanged. No status/closure,
readiness or cause/fix claim. Text verified; visual QA not performed; metadata on
completeness/unknown blocks unavailable. Diagnostic approval remains pending.


## Failed-Test Classification Correction — 2026-10-17

Owner explicitly corrected premature BUG-003 classification and requested fixture
actualization. A failed check alone is a symptom; classify cause from evidence.
Withdraw the diagnostic approval stop for stale-test maintenance. Continue in
FEAT-01 implementation; preserve historic failed runs and report any actually
proven runtime behavior defect separately. This does not alter skill/rule files.


## APR-012 — Proven Timer Adapter Correction

Owner approved exact getter correction: «Да, исправь getter и заверши проверки».
This follows runtime proof, not test-failure classification: installed Fusion None
has TargetTick0 and IsRunningfalse; direct forwarding prevented Dodge timer start.
Implementation limited to one getter plus Play fixture actualization/regression.
Full checks in progress, no acceptance/commit/next-subtask authorization inferred.


## R12 Result / Publication Verified — 2026-10-17

Test actualization and APR-012 getter correction completed. Actual connected
Compile passed, final Edit674/674 and Solo9/9 passed; before-fix actualized regression
failed expected Dodge/actual Idle. Runtime build0errors/10dependencywarnings,
strict same4unrelatedPNG ASSET003, tracked diffcheck passed. Focused two-file audit
P0/P1/P2=0/0/0; owner review/retest acceptance/full FEAT audit pending; no commit.

Isolated publish_progress combat-r12-tests-and-timer-result-20261017 completed;
request SHA256 51ab0dea1986a1cac118975c12855cc458eb0a9031536767f5234730d60d478a.
Specification version 2026-10-17T10:19:00.983Z, readback 2026-10-17T10:19:10Z.
One marker, exact section after blank-line normalization, prior body/properties
preserved. No property/status/closure changes. Explicit completeness metadata
unavailable; text readback verified, visual QA not performed.


## Owner Acceptance / Publication Boundary — 2026-10-17

APR-013: developer «Подтверждаю» accepts revision12 exact two-file delta and
executed Compile/Solo9/Edit674 results. Embedded BUG-003 review/retest complete;
return to pending full FEAT-01 audit. No full FEAT acceptance, separate commit,
next-subtask readiness or new manual/network execution inferred.

Owner clarified Notion is game/project documentation, specifications and task
board, not a feature-development process journal. Local workflow stages, approvals,
diagnostic iterations, audits/test runs and technical handoffs belong in state.yaml,
feature/bug Markdown and evidence. This acceptance is recorded locally only.
Canonical publication boundary updated in docs/notion/feature-integration.md and
referenced by AGENTS/feature/Notion/bug instructions. No Notion worker/MCP operation
or remote edit was performed. Existing remote history was not silently rewritten.


## Passive Mediator Owner Feedback — 2026-10-17

Owner rejects the mediator gameplay tick/input/action-admission role: Notify routes
to handlers only. Proposed revision13 moves that cycle into a player-level action
controller, preserves attack/admission/movement tick order and existing notifications.
No production changes yet; exact owner/prefab/API inventory presented for architecture
approval. Notion untouched; this is local development-process/architecture feedback.


## APR-014 — Temporary Action Controller Accepted

Owner accepts revision13 exact code/composition only as temporary and requests
implementation. Passive mediator role is required; controller owns buffered-command
admission between attack update and movement. Preserve temporary architecture debt
locally; no new redesign/task or Notion workflow publication authorized.


## R13 Temporary Mediator Correction Implemented — 2026-10-17

APR-014 temporary architecture implemented: mediator has no gameplay NetworkTick;
new controller owns original admission cycle, root/prefab wired through Editor.
Compile passed, Edit674/674 and Solo10/10 passed, build0errors/10warnings, strict
same4PNG ASSET003, finaldiffcheck passed. Focused7-file audit P0/P1/P2=0/0/0.
Owner diff/Play acceptance pending, full FEAT audit/closure not inferred. Temporary
solution limitation retained; no Notion call, commit or next FEAT.


## APR-015 / Full Audit Resume — 2026-10-17

Owner reports no R13 diff corrections; focused temporary controller/mediator diff
accepted. Do not infer actual owner Host/client execution or whole FEAT completion.
INT-004 returns to full FEAT-01 audit. Current source reconciliation observed a
separate timer API cleanup: EndTick read moved into accessor, float Start removed,
Enemy accessor and both fixtures updated. These existing working-tree changes were
read, understood and preserved without rewriting; review5 captures exact current
hashes. Prior R13 checks apply to their own snapshot; current sources are rechecked.
No Notion call or commit authorized by this process update.


## Full Review5 Audit / Current Verification — 2026-10-17

175-path current snapshot audited incrementally against immutable earlier reports:
P0=0,P1=0,existingP2AUD-001 remains. Preserved observed timer interface/implementation/
fixture edits; added planned command-vs-gameplay-ID300 Solo coverage only. Final
Compile/Edit674/Solo11 passed; build0errors/8warnings; strictsame4PNGerrors; diffcheck
passed. Full current owner review and Host/client/network lifecycle Play remain;
accepted focused R13 diff is not requested again. Temporary controller stays local
architecture debt. No runtime code edit/Notion call/commit/next FEAT this turn.

## DEC-013 / APR-019 — Defer BUG-004 FIX-R1 — 2026-10-17

Owner: «Я бы пока проигнорировал этот фикс. Пропустим», followed by «Продолжи».
Defer the proposed transport change and stop this candidate investigation.
This supersedes APR-017's earlier requirement to fix it before FEAT-01 completion.
EXP-002's250ms delay exceeds the150ms command lifetime; it demonstrates a delivery
loss mechanism, not a proven within-lifetime contract violation. Original R15
triggering conditions remain unknown. The mandatory-fix proposal was premature.

No production fix or common-contract amendment was made. Evidence/proposal retained;
all175 accepted Review5 file hashes match, index empty. INT-005 returns to the
FEAT-01 Play gate without asserting remediation, full Play acceptance, subtask
completion or permission to start FEAT-02. Existing plan unchanged; no Notion edit.

## APR-020 — Hurtbox Contact Visualization R16 — 2026-10-17

Owner: «да подтверждаю», directly following the exact R16 architecture proposal.
Approved real hurtbox outlines and a local0.20s contact flash on the target in
Host/Solo; Client contours only. New GC Combat debug component captures immediately
after the Player hit probe, with existing root Init/reset and registry lookup.
Player/Enemy prefabs and green/red debug styles are the scoped Unity mutations.
No new gameplay contracts/events/RPC, damage logic or FIX-R1 remediation.

Implemented and verified: Compile passed, relevant Edit9/9, Solo12/12, build0errors,
same4unrelated strict PNG errors, actual red -> green Game View captures inspected.
Test harness completion and new network-position fixture corrections are test-only.
See subtasks/FEAT-01-hurtbox-debug-r16-result.md and review6 manifest/delta patch.
R16 diff review and developer manual Play remain pending; no full FEAT completion,
checkpoint, commit or Notion publication inferred.

## Owner Acceptance — 2026-10-17

APR-021 accepts the current diff; APR-022 records the developer's reported manual
Play execution and acceptance: «я выполнил и проверку дифа и я все подтверждаю и
плеймод проверку». This supersedes the pending review/manual gate in earlier text.
Exact peer mode/new network execution is not specified. See
subtasks/FEAT-01-owner-acceptance-r16.md (feature-root relative). INT-006 returns
to the plan/checkpoint step. Plan unchanged; strict's four unrelated ASSET003
errors remain failed, so no automatic commit/FEAT-01 completion yet. No Notion write.

## APR-023 — Exclude Unrelated PNGs and Commit FEAT-01 — 2026-10-17

Owner explicitly instructs ignoring the four imported Enemy PNGs and making the
local commit, then continuing. This resolves the earlier checkpoint block without
renaming/removing/staging those files or their metas. Global Strict remains failed
on those exact paths; accepted implementation checks and review/manual gates stand.
The prior statement that a PNG decision was needed is superseded by this decision.
FEAT-01 is accepted; prepare CP-001, then FEAT-02 architecture. No policy/tooling
change, new subtask, push, merge or Notion workflow publication is authorized.
