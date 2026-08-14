using Cysharp.Threading.Tasks;
using Domain;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IPopupSystem
    {
        UniTask<Result<TResponse>> OpenAsync<TPopup, TPayload, TResponse>(
            TPayload payload,
            CancellationToken cancellationToken)
            where TPopup : BasePopupView<TPayload, TResponse>
            where TPayload : IPopupPayload;

        UniTask AbortAsync<TPopup>() where TPopup : BasePopupView;

        UniTask AbortAllAsync();
    }
}
