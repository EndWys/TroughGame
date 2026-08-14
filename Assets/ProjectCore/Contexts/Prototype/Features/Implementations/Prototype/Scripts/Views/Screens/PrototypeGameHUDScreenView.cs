using Cysharp.Threading.Tasks;
using Domain;
using ProjectCore.Template;
using System;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeGameHUDScreenView : BaseScreenView<EmptyScreenSettings>
    {
        private const string HUDTitleName = "HUDTitle";
        private const string OpenConfirmationPopupButtonName = "OpenConfirmationPopupButton";

        private readonly IPopupSystem _popupSystem;

        private CancellationTokenSource _lifetimeCancellation;
        private Label _hudTitle;
        private Button _openConfirmationPopupButton;

        public PrototypeGameHUDScreenView(IPopupSystem popupSystem)
        {
            _popupSystem = popupSystem ?? throw new ArgumentNullException(nameof(popupSystem));
        }

        protected override UniTask OnInitializeAsync(
            EmptyScreenSettings settings,
            CancellationToken cancellationToken)
        {
            _hudTitle = this.Q<Label>(HUDTitleName)
                ?? throw new InvalidOperationException($"HUD requires a {HUDTitleName} label.");
            _openConfirmationPopupButton = this.Q<Button>(OpenConfirmationPopupButtonName)
                ?? throw new InvalidOperationException(
                    $"HUD requires a {OpenConfirmationPopupButtonName} button.");
            _lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _openConfirmationPopupButton.clicked += HandleOpenConfirmationPopup;

            return UniTask.CompletedTask;
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            if (_openConfirmationPopupButton != null)
            {
                _openConfirmationPopupButton.clicked -= HandleOpenConfirmationPopup;
            }

            _lifetimeCancellation?.Cancel();
            _lifetimeCancellation?.Dispose();
            _lifetimeCancellation = null;

            return UniTask.CompletedTask;
        }

        private void HandleOpenConfirmationPopup()
        {
            OpenConfirmationPopupAsync(_lifetimeCancellation.Token).Forget();
        }

        private async UniTaskVoid OpenConfirmationPopupAsync(CancellationToken cancellationToken)
        {
            _openConfirmationPopupButton.SetEnabled(false);

            try
            {
                Result<PrototypeConfirmationResponses> result = await _popupSystem.OpenAsync<
                    PrototypeConfirmationPopupView,
                    PrototypeConfirmationPopupPayload,
                    PrototypeConfirmationResponses>(
                    new PrototypeConfirmationPopupPayload(
                        "LEAVE PROTOTYPE?",
                        "This popup returns a typed response to the HUD."),
                    cancellationToken);

                _hudTitle.text = result.IsSuccess
                    ? $"POPUP: {result.Value}"
                    : $"POPUP: {result.FirstError.Code}";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _openConfirmationPopupButton.SetEnabled(true);
                }
            }
        }
    }
}
