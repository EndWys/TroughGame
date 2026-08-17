# Logging

`Logging` is reusable Template infrastructure installed once in the persistent
Project context by `LoggingFeature`.

## Responsibilities

- `DebugLoggerService` is the root logger. It writes messages, warnings,
  errors, and exceptions through the Unity `Debug` API.
- `FeatureDebugLoggerService<TFeature>` adds a feature category and delegates
  output to the root `IDebugLogger`. One instance of each closed generic type
  is cached in the current context container.
- `LogService` receives all Unity log callbacks through
  `Application.logMessageReceived` and forwards them to registered
  `ILogHandler` implementations. It is intended for optional destinations such
  as files, analytics, Crashlytics, or development tools.

The output path is:

```text
IDebugLogger<PlayerFeature>
  -> FeatureDebugLoggerService<PlayerFeature>
  -> DebugLoggerService
  -> Unity Debug
  -> Application.logMessageReceived
  -> LogService
  -> ILogHandler
```

## Feature Logging

Inject a logger parameterized by the owning feature into every service,
system, factory, or view that must share its category:

```csharp
public sealed class PlayerService
{
    private readonly IDebugLogger<PlayerFeature> _logger;

    public PlayerService(IDebugLogger<PlayerFeature> logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        _logger.LogMessage("Initialized.");
    }
}
```

The output is `[Player] Initialized.`. The exact `Feature` suffix is removed
from the category name. Manual category registration and logger construction
are not required.

All consumers of `IDebugLogger<PlayerFeature>` in one context receive the same
instance. A different context receives a separate instance, which is released
with that context's DI container. The root `IDebugLogger` remains alive until
the Project context is destroyed.

Use the non-generic `IDebugLogger` only for global infrastructure that does not
belong to a specific feature category.

## Exceptions

Pass both the exception and useful operation context when available:

```csharp
catch (Exception exception)
{
    _logger.LogException(exception, "Failed to load player data.");
}
```

The logger writes the contextual message and then the original exception with
its stack trace. A null exception is rejected.

## Log Handlers

Implement `ILogHandler` and register it through the injected `ILogService` when
logs must be forwarded elsewhere:

```csharp
public sealed class AnalyticsLogHandler : ILogHandler
{
    public void HandleLog(string condition, string stackTrace, LogType type)
    {
        // Forward the log to the selected destination.
    }
}
```

```csharp
_logService.AddLogHandler(_analyticsLogHandler);
```

Duplicate handler instances are ignored. A context-scoped handler must call
`RemoveLogHandler` before its context is destroyed; otherwise the persistent
`LogService` retains it and continues sending callbacks to it. Project-scoped
handlers are cleared automatically when `LogService` is disposed.

`LogService` is not required for ordinary logging. It is an extension point
for observing or forwarding messages already emitted through Unity logging.
