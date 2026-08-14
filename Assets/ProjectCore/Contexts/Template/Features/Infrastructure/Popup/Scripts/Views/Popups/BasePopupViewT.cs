using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ProjectCore.Template
{
    public abstract class BasePopupView<TPayload, TResponse> : BasePopupView
        where TPayload : IPopupPayload
    {
        protected void Complete(TResponse response)
        {
            RequestCompletion(new PopupCompletionData(response, Domain.Error.None));
        }

        protected sealed override UniTask OnInitializeAsync(
            IPopupPayload payload,
            CancellationToken cancellationToken)
        {
            if (payload is not TPayload typedPayload)
            {
                throw new ArgumentException(
                    $"{GetType().Name} requires {typeof(TPayload).Name}.",
                    nameof(payload));
            }

            return OnInitializeAsync(typedPayload, cancellationToken);
        }

        protected abstract UniTask OnInitializeAsync(
            TPayload payload,
            CancellationToken cancellationToken);
    }
}
