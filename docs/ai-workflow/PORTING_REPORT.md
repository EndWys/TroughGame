# Адаптация workflow: TroughGame

Дата: 2026-10-12. Версия: **1.2-tg.1**.
Источник: предоставленный архив `rivaldom-unity-feature-workflow-porting-v1.2.zip`.

## Установлено

- `.agents/skills/unity-feature-development/SKILL.md` и 10 references:
  требования, research, архитектура, lifecycle, аудит, state/Git,
  результаты стадий, стиль ответов, optional delegation, knowledge promotion.
- `.ai/features/_template/`: state, decisions, research, описание subtasks.
- `Tools/AI/`: connected-Editor validation, workflow audit, scoped cleanup,
  регрессионная проверка инструментов.
- `docs/ai-workflow/`: workflow reference, changelog, инструкция запуска,
  этот отчёт и исторический исходный DOCX в `sources/`.
- `docs/project-overview.md`, `docs/project-map.md`: реальные технологии,
  контексты, installers, startup flow, существующие тестовые assemblies.
- `AGENTS.md`, `docs/README.md`, `.gitignore`: routing навыка, индекс,
  игнорирование временного evidence.

## Политика переноса

| Элемент | Решение |
| --- | --- |
| Поэтапные согласования | Сохранены по ответу разработчика: ТЗ, план, локальная архитектура, review, Play Mode, финальная готовность |
| Автоматические коммиты | Разрешены по ответу разработчика после review, успешного Play Mode и применимых проверок; только принятые пути и state |
| Архитектура и стиль | Владельцами остаются `ARCHITECTURE_RULES.md` и `CONVENTIONS.md`; исходные правила другого проекта не установлены |
| Unity integration | Используется уже подключённый Editor TroughGame; CLI/Pipeline не переустанавливались |
| Финальный аудит | Сохранён, с явным указанием границ структурного скрипта и отдельной проверки фактов агентом |
| Субагенты | Опциональны; только при разрешённом делегировании в текущей среде |
| Evidence | Временные отчёты игнорируются; cleanup ограничен evidence проекта и защищает общие каталоги |
| Исторический DOCX | Сохранён как источник сравнения, исключён из приоритетов |

Валидатор учитывает строковые и объектные ответы Pipeline, проверяет принятие
команд и не принимает неполные результаты тестов за успешные. Исправлена
циклическая формулировка исходного completion: retrospective и knowledge
promotion выполняются после решения о финальной готовности.

## Проверки

| Проверка | Фактический результат |
| --- | --- |
| Skill `quick_validate.py` | Passed |
| YAML шаблона | Parsed, версия `1.2-tg.1` |
| Синтаксис PowerShell | Passed |
| Connected Editor Diagnose | Available, Unity `6000.3.14f1`, CLI `1.0.0-beta.13` |
| Connected Editor Compile | Passed |
| Editor tests с фильтром `ProjectCore` | **134 passed**, 0 failed, 0 skipped, 0 inconclusive |
| Architecture Strict | 533 scripts, 167 resources, **0 errors, 0 warnings** |
| `dotnet build ProjectCore.Runtime.csproj --no-restore -v:minimal` | Passed, 0 errors; 8 предупреждений MSB3277 о версиях зависимостей |
| `Test-WorkflowTools.ps1` | Passed: audit state/missing-state, cleanup dry-run/границы/чужие отчёты, CLI успех/отказ/неполный результат/провал тестов |
| Markdown ссылки и локальные reference paths | Passed |
| `git diff --check` | Passed |
| Git ignore evidence | Проверен через `git check-ignore` |

JSON и логи реальных Unity-проверок сохранены локально в
`.ai/evidence/validation/`. Они не входят в Git. Smoke-тесты создают отдельный
временный репозиторий и удаляют его после проверки; реальных feature states
из исходного проекта или тестовой фичи в рабочем дереве нет.

Полный цикл на новой игровой фиче, ручное Play Mode принятие, коммит checkpoint
и возобновление в отдельном чате ещё не выполнялись: установка workflow не
создаёт игровую фичу. Это дальнейшая проверка использования, а не выполненное
согласование или игровое acceptance evidence.

Runtime C#, сцены, prefabs, Unity `.meta`, assemblies и manifest не менялись.
Изменения установки оставлены в рабочем дереве для просмотра; коммит не создан.
