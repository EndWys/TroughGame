using Cysharp.Threading.Tasks;
using Domain;
using ProjectCore.Template;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeGameHUDScreenView : BaseScreenView<EmptyScreenSettings>
    {
        private const string HUDTitleName = "HUDTitle";
        private const string OpenConfirmationPopupButtonName = "OpenConfirmationPopupButton";

        private readonly IPopupSystem _popupSystem;

        private CancellationTokenSource _lifetimeCancellation;
        private IVisualElementScheduledItem _popupShortcutUpdate;
        private Label _hudTitle;
        private Button _openConfirmationPopupButton;
        private bool _isConfirmationPopupActive;
        private bool _isConfirmationPopupAborting;

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
            _openConfirmationPopupButton.clicked += HandleToggleConfirmationPopup;
            _popupShortcutUpdate = schedule.Execute(HandlePopupShortcut).Every(1);
            RefreshPopupControlState();

            return UniTask.CompletedTask;
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            if (_openConfirmationPopupButton != null)
            {
                _openConfirmationPopupButton.clicked -= HandleToggleConfirmationPopup;
            }

            _popupShortcutUpdate?.Pause();
            _popupShortcutUpdate = null;
            _lifetimeCancellation?.Cancel();
            _lifetimeCancellation?.Dispose();
            _lifetimeCancellation = null;

            return UniTask.CompletedTask;
        }

        private void HandlePopupShortcut()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                HandleToggleConfirmationPopup();
            }
        }

        private void HandleToggleConfirmationPopup()
        {
            if (_isConfirmationPopupActive)
            {
                if (!_isConfirmationPopupAborting)
                {
                    AbortConfirmationPopupAsync().Forget();
                }

                return;
            }

            OpenConfirmationPopupAsync(_lifetimeCancellation.Token).Forget();
        }

        private async UniTaskVoid OpenConfirmationPopupAsync(CancellationToken cancellationToken)
        {
            _isConfirmationPopupActive = true;
            RefreshPopupControlState();

            try
            {
                Result<PrototypeConfirmationResponses> result = await _popupSystem.OpenAsync<
                    PrototypeConfirmationPopupView,
                    PrototypeConfirmationPopupPayload,
                    PrototypeConfirmationResponses>(
                    new PrototypeConfirmationPopupPayload("PopUp Sytem Test"),
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
                _isConfirmationPopupActive = false;
                _isConfirmationPopupAborting = false;
                RefreshPopupControlState();
            }
        }

        private async UniTaskVoid AbortConfirmationPopupAsync()
        {
            _isConfirmationPopupAborting = true;
            RefreshPopupControlState();

            try
            {
                await _popupSystem.AbortAsync<PrototypeConfirmationPopupView>();
            }
            finally
            {
                _isConfirmationPopupAborting = false;
                RefreshPopupControlState();
            }
        }

        private void RefreshPopupControlState()
        {
            if (_openConfirmationPopupButton == null)
            {
                return;
            }

            _openConfirmationPopupButton.text = _isConfirmationPopupActive
                ? _isConfirmationPopupAborting
                    ? "ABORTING POPUP..."
                    : "ABORT POPUP [P]"
                : "OPEN POPUP [P]";
            _openConfirmationPopupButton.SetEnabled(!_isConfirmationPopupAborting);
        }
    }
}
