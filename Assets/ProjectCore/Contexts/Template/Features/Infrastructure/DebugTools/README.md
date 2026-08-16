# Debug Tools

`DebugToolsFeatureGroup` composes Cheats, Debug Visualization, and Runtime
Console in the persistent Project context. The child systems keep their own
contracts and implementations. `DebugToolsFeature` owns only availability and
enabled-state policy; it does not absorb the child feature implementations.

Command Line is installed independently before this group. Debug Tools only
uses `ICommandLineService` when command-line state overrides are enabled.

Open `ProjectCore > Debug Tools` to edit build availability, initial module
state, command-line overrides, and Runtime Console settings. Release builds are
disabled by default.

Runtime consumers inject `IDebugToolsService`:

```csharp
if (_debugToolsService.IsEnabled(DebugToolTypes.Visualization))
{
    // Use optional debug-only behavior.
}

Result result = _debugToolsService.Disable(DebugToolTypes.Console);
```

`Enable` and `Disable` affect the current application session and fail when the
selected tool is not allowed for the current build.

The group initializes children in this order: Debug Tools policy, Cheat,
Debug Visualization, and Runtime Console. Child features observe
`ToolStateChanged` and create or destroy only their own runtime objects. All
bindings and subscriptions end with ProjectContext.

Supported command-line overrides:

- `--debug-tools` and `--disable-debug-tools`;
- `--debug-console` and `--disable-debug-console`;
- `--debug-cheats` and `--disable-debug-cheats`;
- `--debug-visualization` and `--disable-debug-visualization`.

Overrides are ignored when `Command-line Overrides` is disabled. They cannot
enable tools in a build target forbidden by the config.
