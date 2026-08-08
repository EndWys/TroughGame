# Local Save

Persistent local key/value storage. `LocalSaveFeature` is installed in the
Project context. Consumers inject only `ILocalSaveService`.

## Save descriptor

Create one immutable `LocalSaveDescriptor<TData>` for each saved document.

```csharp
private static readonly LocalSaveDescriptor<SettingsData> SettingsDescriptor =
    new LocalSaveDescriptor<SettingsData>(
        "settings.player",
        LocalSaveStorageTypes.PlayerPrefs);
```

The descriptor defines:

- stable `Key`;
- `StorageType`;
- `SerializerType` (`Json` is the only current option);
- document `Version` (default `1`);
- optional `formerKeys` for renamed keys.

Keys may contain letters, digits, `.`, `-`, and `_` only.

## Usage

```csharp
public sealed class SettingsService
{
    private readonly ILocalSaveService _localSaveService;

    public SettingsService(ILocalSaveService localSaveService)
    {
        _localSaveService = localSaveService;
    }

    public async UniTask InitializeAsync(CancellationToken cancellationToken)
    {
        Result<SettingsData> result = await _localSaveService.LoadOrCreateAsync(
            SettingsDescriptor,
            () => new SettingsData(true),
            cancellationToken);

        if (result.IsFailure)
        {
            return;
        }

        SettingsData settings = result.Value;
    }

    public UniTask<Result> SaveAsync(SettingsData settings, CancellationToken cancellationToken)
    {
        return _localSaveService.SaveAsync(SettingsDescriptor, settings, cancellationToken);
    }
}
```

Available operations: `SaveAsync`, `LoadAsync`, `LoadOrCreateAsync`,
`ExistsAsync`, and `DeleteAsync`. Every operation returns `Result` or
`Result<TData>`.

## Storage types

| Type | Use |
| --- | --- |
| `PlayerPrefs` | Small preferences. |
| `File` | Ordinary local progress and larger documents. |
| `SecureFile` | Device-bound encrypted local documents. Not an authority or anti-cheat mechanism. |
| `Database` | Not registered by default. Add an `ILocalSaveStorage` implementation before use. |

## Renaming and versioning

```csharp
var descriptor = new LocalSaveDescriptor<SettingsData>(
    "settings.player.v2",
    LocalSaveStorageTypes.PlayerPrefs,
    formerKeys: new[] { "settings.player" });
```

A successful load from a former key is saved under the current key and deletes
the former value. A document version different from `Version` returns
`LocalSave.VersionMismatch`; it is not loaded automatically.
