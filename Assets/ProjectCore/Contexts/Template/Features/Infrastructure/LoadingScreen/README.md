# Loading Screen

`LoadingScreenFeature` provides scene-independent UI Toolkit loading screens.
It is installed in `ProjectContext`, so its `UIDocument` remains alive while
gameplay scenes are unloaded and initialized.

Only one loading screen can be active at a time. Loading screens have no
history, hierarchy, user response, or self-controlled completion. The owner
that starts an operation must hide the loading screen when the operation ends.

## Public API

Inject `ILoadingScreenSystem`:

```csharp
Result showResult = await _loadingScreenSystem.ShowAsync<
    CustomLoadingScreenView,
    CustomLoadingScreenSettings>(
    settings,
    cancellationToken);

Result hideResult = await _loadingScreenSystem.HideAsync(cancellationToken);
```

Use the configured default screen when no custom settings are required:

```csharp
Result result = await _loadingScreenSystem.ShowDefaultAsync(cancellationToken);
```

`IsVisible` reports whether a loading screen is currently owned by the system.
`IsTransitioning` is true during its show or hide lifecycle.

## Lifecycle

Show:

1. Validate the definition and settings.
2. Create the View through `IClassFactory` and inject its dependencies.
3. Clone the definition UXML into the View.
4. Block input immediately.
5. Run `InitializeAsync` and `ShowAsync`.

Hide:

1. Keep input blocked.
2. Run `HideAsync` and `CloseAsync`.
3. Remove the View.
4. Release input.

Cancellation and lifecycle failures remove the View immediately. `HideAsync`
is idempotent when no loading screen is active. Project context disposal
performs synchronous emergency cleanup.

## Main Types

- `ILoadingScreenSystem` is the consumer contract.
- `LoadingScreenSystem` owns the active View and its lifecycle.
- `LoadingScreenComponent` owns the persistent root `UIDocument`.
- `BaseLoadingScreenView<TSettings>` is the base class for concrete Views.
- `LoadingScreenDefinition<TScreen, TSettings>` maps a View and settings type
  to a UXML asset.
- `LoadingScreenCatalogConfig` registers definitions and selects the default.
- `DefaultLoadingScreenView` is the default project loading screen.

## Adding A Loading Screen

1. Create immutable settings implementing `ILoadingScreenSettings`.
2. Create a View inheriting `BaseLoadingScreenView<TSettings>` in
   `Scripts/Views/Screens`.
3. Create its UXML and USS in `GraphicResources`.
4. Create a definition inheriting
   `LoadingScreenDefinition<TView, TSettings>`.
5. Create the definition asset and assign its UXML.
6. Register the definition in `LoadingScreenCatalogConfig`.
7. Call the typed `ShowAsync` overload and always pair it with `HideAsync`.

The catalog default must use `EmptyLoadingScreenSettings`.

## Scene Flow Integration

`SceneLoadingScreenBridgeFeature` binds `SceneLoadingScreenAdapter` as an
`ISceneTransitionPresenter`. The bridge depends only on public contracts:

1. `SceneFlowService` shows the default loading screen.
2. The current scene exits and is unloaded.
3. The new scene loads and completes `IGameSceneLifecycle.InitializeAsync`.
4. `SceneFlowService` hides the loading screen in `finally`.

The loading screen therefore also covers the initial Preloader-to-gameplay
transition without any change to `ApplicationEntryPoint`.
