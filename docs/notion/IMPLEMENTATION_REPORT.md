# Notion integration: результаты установки

Дата: 2026-10-12. Feature workflow: **1.3-tg.1**. Протокол операций: **1**.

Созданы три локальных навыка: диспетчер `notion-project-coordination`,
read-only worker `notion-feature-intake`, publication worker
`notion-project-documentation`. Все MCP-действия выполняет отдельный сабагент
с минимальным контекстом; разработчик владеет своим workflow и Git.

Источник привязки — переданная пользователем база «Задачи» и её проверенные
предки/relations. Проверены root TroughGame, 4 базы, их data sources, views,
properties и status options. Подробности в [project.json](project.json).
Сабагент обнаружил и прочитал существующий включённый навык Notion
«Стандарт документации TroughGame» v1.2 и процесс ведения документации.
Они используются как живые источники оформления/прогресса; новая конкурирующая
политика в Notion не создана.

Добавлены immutable request/result, source refresh, точные scopes публикации,
preflight rejection без journal, сохранение частичных эффектов и replay markers
для append/create. Журнал `.ai/notion/operations/` независим и игнорируется Git.
В feature `state.yaml` изменён только номер версии шаблона; поля Notion state
не добавлялись. Чтение или изменение реальных feature states не выполнялось.

## Проверки

| Проверка | Результат |
| --- | --- |
| Skill quick_validate.py | Все 3 новых навыка и обновлённый feature skill прошли |
| Local journal tests | **10 passed**, offline, отдельные временные каталоги |
| Existing Test-WorkflowTools.ps1 | Passed после обновления версии fixtures |
| PowerShell syntax, JSON binding, Markdown links | Passed |
| Git whitespace/conflict checks | Passed, включая новые текстовые файлы |
| Read-only MCP preflight отдельным сабагентом | Auth/access работают, реальные schemas и стандарты прочитаны |
| Независимый publication forward-test | Отказал в закрытии при отсутствующем Play Mode, общем state-path, неверном task ID и неизвестном результате прежней записи |
| Исправления по forward-test | Уточнены preflight envelope, deterministic publication marker и partial с нулём подтверждённых effects; повторный review прошёл |
| Live intake forward-test | Прочитаны 26 working tasks и 14 feature rows без продолжения страницы; две Ready Dev-задачи Combat имеют незакрытую зависимость In progress, выбор заблокирован |

В live intake-тесте Python отсутствовал в PATH: первая инициализация не
выполнилась, а read-only fetch уже начался. Сабагент затем использовал bundled
runtime, записал результат и сообщил отклонение. Инструкции уточнены: runtime и
успешная инициализация проверяются до любых MCP-вызовов; диспетчер передаёт
известный путь Python. Это отклонение не выдано за полностью пройденный
порядок execution; нарушений границ feature state или remote writes не было.

Повторный узкий live forward-test после исправления прошёл: bundled Python
успешно создал журнал до единственного fetch задачи; source hash и edit time
совпали с baseline, результат был записан только после чтения. Статус зависимости
остался `In progress`; завершение read-операции не принято за готовность задачи.

Forward-tests не утверждали новую игровую фичу, не запускали разработку и
не меняли страницы, статусы или схему Notion. Тесты записи проверяли поведение
локально на искусственных запросах; реальное end-to-end изменение Notion и
визуальная проверка опубликованной страницы пока не выполнялись.

Runtime/Unity assets не менялись, поэтому повторная сборка/Unity tests для этого
изменения не запускались. Изменения навыков, протокола и локальных инструментов
оставлены для просмотра без коммита. [Инструкция использования](README.md).
