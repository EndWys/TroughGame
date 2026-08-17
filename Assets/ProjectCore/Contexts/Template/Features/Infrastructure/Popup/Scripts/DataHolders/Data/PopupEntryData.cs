using Cysharp.Threading.Tasks;
using Domain;
using System.Threading;

namespace ProjectCore.Template
{
    internal sealed class PopupEntryData
    {
        public PopupEntryData(
            BasePopupDefinition definition,
            BasePopupView popupView,
            CancellationToken callerCancellationToken,
            CancellationTokenSource lifetimeCancellation)
        {
            Definition = definition;
            PopupView = popupView;
            CallerCancellationToken = callerCancellationToken;
            LifetimeCancellation = lifetimeCancellation;
            CompletionSource = new UniTaskCompletionSource<PopupCompletionData>();
            ResultSource = new UniTaskCompletionSource<PopupCompletionData>();
            State = PopupStates.Opening;
            AbortError = Error.None;
        }

        public BasePopupDefinition Definition { get; }
        public BasePopupView PopupView { get; }
        public CancellationToken CallerCancellationToken { get; }
        public CancellationTokenSource LifetimeCancellation { get; }
        public UniTaskCompletionSource<PopupCompletionData> CompletionSource { get; }
        public UniTaskCompletionSource<PopupCompletionData> ResultSource { get; }
        public PopupStates State { get; set; }
        public Error AbortError { get; set; }
    }
}
