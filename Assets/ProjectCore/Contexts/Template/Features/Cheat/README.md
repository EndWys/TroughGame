# Cheat

`Cheat` registers typed debug commands and executes them through a single
validated, asynchronous API. It is installed by `DebugToolsFeatureGroup` and
is available only when `DebugToolTypes.Cheats` is allowed and enabled.

## Public API

- `ICheatService.Commands` exposes the cached command catalog.
- `ICheatService.ExecuteAsync(...)` parses and executes a command. Expected
  failures are returned through `Result<CheatExecutionData>`; cancellation is
  propagated through `OperationCanceledException`.
- `ICheatRegistry.Register(...)` registers a handler and returns its lifetime
  token. Disposing the token unregisters every command from that handler.
- `ICheatArgumentConverterRegistry.Register<T>(...)` adds a context-owned
  converter for a custom argument type.

Command names are case-insensitive. Arguments support quoted strings and
escaped quotes. Built-in conversion covers strings, booleans, characters,
numbers with invariant culture, enums, GUIDs, nullable values, and optional
parameters. A handler method may return `void`, a value, `Result`, `Result<T>`,
`UniTask`, or `UniTask<T>`. A single `CancellationToken` parameter is injected
by the service and is not entered by the caller.

## Adding commands

Create a context-owned handler and a narrow contract used by its Feature:

```csharp
public interface IPlayerCheatHandler
{
    public void Initialize();
}

public sealed class PlayerCheatHandler : BaseCheatHandler, IPlayerCheatHandler
{
    [CheatCommand("add_coins", "Player")]
    private Result AddCoins(
        [CheatArgument("amount")] int amount,
        CancellationToken cancellationToken)
    {
        // Apply the command through injected gameplay contracts.
        return Result.Success();
    }
}
```

Bind all handler interfaces without exposing the implementation, then
initialize it from the owning Feature's explicit initialization step:

```csharp
BindInterfacesAsSingle<PlayerCheatHandler>();
Resolve<IPlayerCheatHandler>().Initialize();
```

Because `BaseCheatHandler` implements `IDisposable`, its registration is
removed with the owning DI context. The Cheat Feature contains no project- or
game-specific handlers.

Custom converters follow the same ownership rule: keep the returned
`IDisposable` in the registering context-owned object and dispose it during
that object's cleanup. Duplicate commands, unsupported signatures, invalid
arguments, and execution failures are returned as `Result` errors rather than
escaping through the normal command flow.
