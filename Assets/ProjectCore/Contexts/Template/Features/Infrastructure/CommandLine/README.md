# Command Line

`CommandLineFeature` provides one immutable command-line snapshot for the
persistent Project context. It is independent from Debug Tools and remains
available in every build type, including dedicated servers.

`EnvironmentCommandLineArgumentsProvider` reads the process arguments once
during feature initialization. `CommandLineParserUtility` parses them into
`CommandLineArgumentsData`, and `CommandLineService` exposes that immutable
snapshot through `ICommandLineService` for the lifetime of ProjectContext.

Supported syntax:

```text
--console
--port 7777
--port=8888
--name "Test Server"
--offset -10
-- positional --arguments
```

Keys are case-sensitive and must start with `--`. Repeated keys are preserved;
`TryGetValue` returns the last value and `GetValues` returns every value in
input order. A key without a value is a flag. Arguments after the standalone
`--` separator are positional.

Consumers inject `ICommandLineService`:

```csharp
if (_commandLineService.TryGetValue("--port", out string port))
{
    // Use the configured port.
}

if (_commandLineService.HasFlag("--headless"))
{
    // Enable headless behavior.
}
```

Use `HasArgument` when either a flag or a valued key is acceptable, `HasFlag`
for valueless switches, `TryGetValue` for the last supplied value, and
`GetValues` for all repeated values. `PositionalArguments` contains tokens
after the standalone `--` separator.

On dedicated servers, `CancelRequested` is raised on the Unity main thread
after an operating-system cancel signal. The service removes this subscription
when the Project container disposes it.
