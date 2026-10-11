# Повторная проверка структуры и тестов

Дата: 2026-10-11. Unity 6000.3.14f1, Unity CLI 1.0.0-beta.13.
Проверялось текущее рабочее дерево после миграции `Assets` + `Components`.

Собственные тесты проекта и архитектурная валидация проходят. Общий набор
Unity включает также тесты Zenject; его интеграционный прогон не завершился.
Поэтому успешное выполнение всех 822 обнаруженных тестов не подтверждено.

| Проверка | Результат |
| --- | --- |
| ProjectCore.Tests.Editor | 134 из 134 Passed; 0 Failed, Skipped, Inconclusive |
| ProjectCore.Tests.Play | 6 из 6 Passed; 0 Failed, Skipped, Inconclusive |
| Zenject-UnitTests-Editor | 418 из 418 Passed; 0 Failed, Skipped, Inconclusive |
| Zenject-IntegrationTests-Editor | Набор из 264 тестов остановился на TestZenAutoInjecter.TestAddComponent; итогового результата нет, запуск отменён |
| Архитектурный Strict | 533 скрипта, 167 ресурсов; 0 ошибок, 0 предупреждений; exit 0 |
| Test-TypeResolution.ps1 | Все 12 сценариев прошли |
| Test-ResourceValidation.ps1 | Все 17 сценариев прошли |
| Дополнительная проверка CreateAssetMenu | 3 сценария прошли: корректный контекст принят; опечатка и чужой контекст дают ASSET007 и exit 1 |
| ProjectCore.Runtime.csproj | Сборка успешна, 0 ошибок; 8 предупреждений MSB3277 о версиях зависимостей |
| git diff --check | Успешно |

Регрессионные сценарии проверяют как допустимые структуры, так и нарушения с
ожидаемыми диагностическими кодами. Это подтверждает, что валидатор не просто
возвращает успешный результат на текущем проекте: нарушения действительно
обнаруживаются, а Strict возвращает ненулевой код выхода.

## Незавершённые интеграционные тесты Zenject

В полном прогоне редактор продолжал симуляцию, но объект
`PlaymodeTestsController.ActiveController` уже был уничтожен. Pipeline при этом
оставался в статусе `running`; контекст Unity Test Framework указывал на
`Zenject.Tests.AutoInjecter.TestZenAutoInjecter.TestAddComponent`.

Отдельный запуск этого теста воспроизвёл то же состояние. Оба зависших запуска
отменены через `cancel_tests`; редактор вернулся в состояние `ready`, повторный
Strict остался чистым. Уничтожение runner подтверждено; конкретный источник
уничтожения и связь с миграцией структуры пока не установлены. Считать этот
набор прошедшим или объявлять сбой существовавшим до миграции нельзя.

В рамках повторной проверки код приложения, валидатора и тестов не менялся.
Исходные отчёты сохранены в `Logs/Architecture/recheck-20261011/`:
`inventory.json`, `editor-project.json`, `play-project.json`, `editor-zenject.json`,
`play-zenject.json`, `play-zenject-isolated.json`, `zenject-stall-diagnostic.json`,
`validation-final.json` и логи регрессионных проверок.
