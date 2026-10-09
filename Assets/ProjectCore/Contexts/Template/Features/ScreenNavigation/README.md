# Screen Navigation

`ScreenNavigationFeature` provides scene-scoped UI Toolkit screen navigation.
Screens are created through the current DI container, initialized with typed
settings, displayed inside one `UIDocument`, and tracked in a hierarchical
navigation history.

## Main components

| Component | Responsibility |
| --- | --- |
| `ScreenNavigationFeature` | Binds and initializes the navigation system for the current scene context. |
| `IScreenNavigationSystem` | Public API used by features, systems, and views. |
| `ScreenNavigationSystem` | Creates screens, performs transitions, and maintains navigation history. |
| `ScreenNavigationComponent` | Provides the `ScreenHost` element from the scene `UIDocument`. |
| `ScreenCatalogConfig` | Contains every screen definition available in the current scene. |
| `BaseScreenDefinition` | Connects a screen type, settings type, parent type, and UXML layout. |
| `BaseScreenView<TSettings>` | Typed UI Toolkit screen base class. |
| `IScreenSettings` | Input supplied to a screen during initialization. |

Screen views are created through `IClassFactory`. Constructor dependencies are
therefore resolved from the DI container of the scene that owns the feature.
Screen settings are passed directly to `OnInitializeAsync`; they are not bound
into the scene container.

## Navigation API

| Method | Behaviour |
| --- | --- |
| `OpenRootAsync` | Opens a root screen and closes the complete previous history. The definition must not have a parent. |
| `OpenChildAsync` | Opens a child of the current screen and appends it to history. Its declared parent must be the current screen. |
| `NavigateAsync` | Opens a root or moves to another child branch. A child parent must already exist in the current history. |
| `GoBackAsync` | Restores the previous screen and closes the current screen. |

`CanGoBack`, `IsTransitioning`, and `CurrentScreenType` expose the current
navigation state. Every navigation operation returns `Result`. A second
operation requested during an active transition fails with
`ScreenNavigation.TransitionInProgress`.

## Screen lifecycle

Opening a screen executes the following flow:

1. Validate the definition, settings type, and parent relationship.
2. Create the screen view through `IClassFactory`.
3. Clone the definition UXML into the screen view.
4. Call `OnInitializeAsync` with the typed settings.
5. Add the screen to `ScreenHost` and show it.
6. Hide the previous screen after the new screen becomes visible.
7. Close history entries that do not belong to the selected branch.
8. Add the new screen to navigation history.

`ShowAsync` and `HideAsync` currently use a 180 ms opacity transition. Showing
the next screen before hiding the previous one prevents an empty frame between
screens.

When a screen is removed from history, `CloseAsync` calls `OnCloseAsync` and
removes the view from the visual tree. Override `OnCloseAsync` to unsubscribe
screen-owned callbacks or release other screen-owned state.

## Adding a screen

### 1. Create settings

Use `EmptyScreenSettings` when no input is required. Otherwise create an
immutable implementation of `IScreenSettings`.

```csharp
public sealed class ShopScreenSettings : IScreenSettings
{
    public ShopScreenSettings(string selectedProductId)
    {
        SelectedProductId = selectedProductId;
    }

    public string SelectedProductId { get; }
}
```

### 2. Create the view

Place the C# view in `Scripts/Views/Screens`. Query and bind UXML elements in
`OnInitializeAsync`; the layout is already cloned at this point.

```csharp
public sealed class ShopScreenView : BaseScreenView<ShopScreenSettings>
{
    private readonly ShopSystem _shopSystem;

    public ShopScreenView(ShopSystem shopSystem)
    {
        _shopSystem = shopSystem;
    }

    protected override UniTask OnInitializeAsync(
        ShopScreenSettings settings,
        CancellationToken cancellationToken)
    {
        Label title = this.Q<Label>("Title");
        title.text = _shopSystem.GetProductName(settings.SelectedProductId);

        return UniTask.CompletedTask;
    }
}
```

### 3. Create the definition

Place the definition in `Scripts/DataHolders/Definitions`. A root screen does
not override `ParentScreenType`.

```csharp
[CreateAssetMenu(
    fileName = "Config_Gameplay_ShopScreen",
    menuName = "SO/Gameplay/Screens/Shop")]
public sealed class ShopScreenDefinition
    : ScreenDefinition<ShopScreenView, ShopScreenSettings>
{
    public override Type ParentScreenType => typeof(MainMenuScreenView);
}
```

Omit `ParentScreenType` when `ShopScreenView` must be a root screen.

### 4. Register presentation assets

1. Store UXML, USS, textures, and fonts under the feature's `GraphicResources`.
2. Create the concrete screen definition asset under `DataResources`.
3. Assign the screen UXML to the definition asset.
4. Add the definition asset to the scene `ScreenCatalogConfig`.

The catalog rejects empty definitions, missing layouts, and duplicate screen
types during feature initialization.

### 5. Open the screen

Inject `IScreenNavigationSystem` and use the operation matching the screen's
place in the hierarchy.

```csharp
Result result = await _screenNavigationSystem
    .OpenChildAsync<ShopScreenView, ShopScreenSettings>(
        new ShopScreenSettings(productId),
        cancellationToken);

if (result.IsFailure)
{
    // Handle or propagate result.FirstError.
}
```

## Scene setup

Each scene using screen navigation requires:

1. A `UIDocument` whose root UXML contains an element named `ScreenHost`.
2. A `ScreenNavigationComponent` referencing that `UIDocument`.
3. A `ScreenNavigationFeature` referencing the component and scene catalog.
4. `ObjectFactoryFeature` registered before `ScreenNavigationFeature`.

Register the feature from the scene context installer:

```csharp
AddFeature<ObjectFactoryFeature>();
AddFeatureFromComponent<ScreenNavigationFeature>();
```
