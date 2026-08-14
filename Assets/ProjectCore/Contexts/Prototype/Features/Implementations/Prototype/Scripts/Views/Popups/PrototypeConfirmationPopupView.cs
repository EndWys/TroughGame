using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeConfirmationPopupView
        : BasePopupView<PrototypeConfirmationPopupPayload, PrototypeConfirmationResponses>
    {
        private const string TitleName = "Title";
        private const string MessageName = "Message";
        private const string ConfirmButtonName = "ConfirmButton";
        private const string DeclineButtonName = "DeclineButton";
        private const string CloseButtonName = "CloseButton";

        private Button _confirmButton;
        private Button _declineButton;
        private Button _closeButton;

        protected override UniTask OnInitializeAsync(
            PrototypeConfirmationPopupPayload payload,
            CancellationToken cancellationToken)
        {
            Label title = this.Q<Label>(TitleName)
                ?? throw new InvalidOperationException($"Popup requires a {TitleName} label.");
            Label message = this.Q<Label>(MessageName)
                ?? throw new InvalidOperationException($"Popup requires a {MessageName} label.");
            _confirmButton = this.Q<Button>(ConfirmButtonName)
                ?? throw new InvalidOperationException($"Popup requires a {ConfirmButtonName} button.");
            _declineButton = this.Q<Button>(DeclineButtonName)
                ?? throw new InvalidOperationException($"Popup requires a {DeclineButtonName} button.");
            _closeButton = this.Q<Button>(CloseButtonName)
                ?? throw new InvalidOperationException($"Popup requires a {CloseButtonName} button.");

            title.text = payload.Title;
            message.text = payload.Message;

            _confirmButton.clicked += HandleConfirmed;
            _declineButton.clicked += HandleDeclined;
            _closeButton.clicked += HandleDismissed;

            return UniTask.CompletedTask;
        }

        protected override UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            if (_confirmButton != null)
            {
                _confirmButton.clicked -= HandleConfirmed;
            }

            if (_declineButton != null)
            {
                _declineButton.clicked -= HandleDeclined;
            }

            if (_closeButton != null)
            {
                _closeButton.clicked -= HandleDismissed;
            }

            return UniTask.CompletedTask;
        }

        private void HandleConfirmed()
        {
            Complete(PrototypeConfirmationResponses.Confirmed);
        }

        private void HandleDeclined()
        {
            Complete(PrototypeConfirmationResponses.Declined);
        }

        private void HandleDismissed()
        {
            Dismiss();
        }
    }
}
