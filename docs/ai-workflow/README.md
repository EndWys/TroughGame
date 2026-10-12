# Workflow новых фич TroughGame

Версия: **1.2-tg.1**, адаптация архива v1.2 от 2026-10-12.

Локальный навык: [unity-feature-development](../../.agents/skills/unity-feature-development/SKILL.md).
Основной контракт процесса: [workflow-reference.md](workflow-reference.md).
Архитектура, стиль и сцены определяются действующими документами проекта.

Для запуска: «Используй `$unity-feature-development` для новой фичи `<название>`.
ТЗ: `<документ или текст>`». Если ТЗ ещё не согласовано, первая стадия готовит
контракт и вопросы до его утверждения. Для продолжения: «Продолжи workflow фичи
`<feature-kebab-case>` по `.ai/features/<feature-kebab-case>/state.yaml`».

Процесс сохраняет согласования ТЗ, плана, архитектуры подзадач, review и
финальной готовности. Агент выполняет доступные проверки; разработчик
подтверждает ручной Play Mode сценарий. После принятого review, успешного
Play Mode и применимых проверок разрешён автоматический локальный коммит
подзадачи. Push, merge и release требуют отдельного поручения.

`.ai/features/_template/` — шаблон состояния, не начатая фича. Реальные записи
создаются только для новых фич этого проекта. Временные JSON/log/Markdown
отчёты в `.ai/evidence/` игнорируются Git. До очистки сохраните итоговые факты
и краткие результаты в артефактах фичи.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\validate-unity.ps1 -Mode Diagnose
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\validate-unity.ps1 -Mode Compile
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\validate-unity.ps1 -Mode EditMode -Filter ProjectCore
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\workflow-audit.ps1 -Feature <feature-kebab-case>
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\cleanup-feature-evidence.ps1 -Feature <feature-kebab-case> -WhatIf
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\AI\Test-WorkflowTools.ps1
```

`Relevant` компилирует и запускает Editor tests; `-Filter` сужает их по имени.
`PlayMode` — отдельный режим автоматических тестов; он не заменяет ручной
сценарий принятия. Скрипт использует уже подключённый Editor конкретного
проекта и не запускает запасной экземпляр. CLI задаётся параметром
`-UnityCliPath`, переменной `TROUGHGAME_UNITY_CLI_PATH`, PATH или стандартным
Windows install path. Недоступный Editor/CLI или неподтверждённый результат
записывается как пропущенная проверка, а не успех.

Аудит проверяет структуру state и ссылки; он не разбирает полный YAML и не
доказывает наличие всех согласований, корректность кода или AC. Агент сверяет
эти данные отдельно. Cleanup проверяет границы путей и удаляет только
записанные временные каталоги; сначала используйте `-WhatIf`.

Исторический [исходный DOCX](sources/AI_Unity_Development_Workflow_Base_v1.2.docx)
сохранён для сравнения. Его инструкции не определяют правила TroughGame.
Результаты установки: [PORTING_REPORT.md](PORTING_REPORT.md).
