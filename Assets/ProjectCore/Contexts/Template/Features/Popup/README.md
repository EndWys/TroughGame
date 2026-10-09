# Popup

`PopupFeature` provides scene-scoped UI Toolkit popups. Each popup owns a
single operation, receives a typed payload, and completes with a typed response
or an error.

## Main components

| Component | Responsibility |
| --- | --- |
| `PopupFeature` | Binds and initializes the popup system for the current scene. |
| `IPopupSystem` | Opens popups and exposes explicit emergency abort operations. |
| `PopupSystem` | Creates popup instances, controls input, and completes popup flows. |
| `PopupComponent` | Provides `PopupHost` and `PopupInputBlocker` from a dedicated `UIDocument`. |
| `PopupCatalogConfig` | Contains popup definitions available in the current scene. |
| `BasePopupDefinition` | Connects popup, payload, response, and UXML layout types. |
| `BasePopupView<TPayload, TResponse>` | Typed UI Toolkit popup base class. |
| `IPopupPayload` | Input supplied once during popup initialization. |

Popup views are created through `IClassFactory`, so constructor dependencies
come from the owning scene DI container. Payloads are passed directly to the
view and are not registered in DI.

## Public API

Open a popup and await its final response:

```csharp
Result<ConfirmationResponses> result = await _popupSystem.OpenAsync<
    ConfirmationPopupView,
    ConfirmationPopupPayload,
    ConfirmationResponses>(
    new ConfirmationPopupPayload(title, message),
    cancellationToken);
```

The task remains pending for the complete popup lifetime.

- `Complete(response)` returns `Result<TResponse>.Success`.
- `Dismiss()` returns `Popup.Dismissed`.
- `Fail(error)` returns the supplied error.
- `AbortAsync<TPopup>()` aborts the newest active popup of that type and returns
  `Popup.Aborted` to its awaiting `OpenAsync`.
- `AbortAllAsync()` aborts every active popup.
- Cancellation of the supplied token removes the popup and throws
  `OperationCanceledException`.
- Scene context disposal removes every popup and returns
  `Popup.ContextDisposed`.

`Abort` is an exceptional external interruption. Normal popup closure is
always initiated by the popup view through its own interaction logic.

## Lifecycle and input

Opening a popup executes the following flow:

1. Validate the definition, payload, and response types.
2. Create the popup through the current DI container.
3. Clone its UXML into `ContentRoot`.
4. Register it as `Opening` and immediately block all lower UI input.
5. Call `OnInitializeAsync` with the typed payload.
6. Show the popup while its own controls remain blocked.
7. Mark it as `Opened` and enable input only for the top popup.
8. Wait for `Complete`, `Dismiss`, `Fail`, abort, or cancellation.
9. Disable input before starting the closing animation.
10. Call `OnCloseAsync`, remove the popup, and enable the next top popup.

Each popup has its own `Opening`, `Opened`, `Closing`, and `Closed` state.
Popups do not transition into one another. A new popup is placed above existing
popups; lower popups remain alive but cannot receive input.

`PopupInputBlocker` is enabled synchronously before the first asynchronous
operation. The opening popup receives input only after its show animation has
completed. During closing, lower popups remain blocked until the closing popup
has been removed.

## Adding a popup

### 1. Create a payload and response

```csharp
public sealed class DeleteItemPopupPayload : IPopupPayload
{
    public DeleteItemPopupPayload(string itemName)
    {
        ItemName = itemName;
    }

    public string ItemName { get; }
}

public enum DeleteItemResponses
{
    Confirmed,
    Declined
}
```

Use `EmptyPopupPayload` when no input is needed. The response may be a boolean,
enum, identifier, or complete form data object. Do not use `Result<T>` as the
response type because `OpenAsync` already returns `Result<TResponse>`.

### 2. Create the view

Store popup views in `Scripts/Views/Popups`.

```csharp
public sealed class DeleteItemPopupView
    : BasePopupView<DeleteItemPopupPayload, DeleteItemResponses>
{
    private Button _confirmButton;
    private Button _declineButton;

    protected override UniTask OnInitializeAsync(
        DeleteItemPopupPayload payload,
        CancellationToken cancellationToken)
    {
        this.Q<Label>("Message").text = $"Delete {payload.ItemName}?";

        _confirmButton = this.Q<Button>("ConfirmButton");
        _declineButton = this.Q<Button>("DeclineButton");
        _confirmButton.clicked += HandleConfirmed;
        _declineButton.clicked += HandleDeclined;

        return UniTask.CompletedTask;
    }

    protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
    {
        _confirmButton.clicked -= HandleConfirmed;
        _declineButton.clicked -= HandleDeclined;
        return UniTask.CompletedTask;
    }

    private void HandleConfirmed()
    {
        Complete(DeleteItemResponses.Confirmed);
    }

    private void HandleDeclined()
    {
        Complete(DeleteItemResponses.Declined);
    }
}
```

Use `Dismiss()` for a close button that produces no response. Use
`Complete(cancelResponse)` instead when cancellation is a valid response.

### 3. Create and register the definition

```csharp
[CreateAssetMenu(
    fileName = "Config_Gameplay_Inventory_DeleteItemPopup",
    menuName = "SO/Gameplay/Inventory/DeleteItemPopup")]
public sealed class DeleteItemPopupDefinition
    : PopupDefinition<
        DeleteItemPopupView,
        DeleteItemPopupPayload,
        DeleteItemResponses>
{
}
```

1. Store UXML, USS, textures, and fonts under the implementation feature's
   `GraphicResources`.
2. Store the concrete definition asset under its `DataResources`.
3. Assign the popup UXML to the definition.
4. Add the definition to the scene `PopupCatalogConfig`.

The concrete UXML describes only popup content. The shared popup base provides
the full-screen backdrop, content host, and interaction blocker.

## Scene setup

Each scene using popups requires:

1. A dedicated `UIDocument` using the popup root UXML and popup panel settings.
2. A `PopupComponent` referencing that document.
3. A `PopupFeature` on the context installer GameObject, referencing the
   component and scene popup catalog.
4. `ObjectFactoryFeature` registered before `PopupFeature`.

```csharp
AddFeature<ObjectFactoryFeature>();
AddFeatureFromComponent<PopupFeature>();
```

The popup document sorting order must remain above the screen document. The DI
container disposes `PopupSystem` together with its Scene Context; active popup
flows never survive a scene change.
