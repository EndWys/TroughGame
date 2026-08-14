using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IPopupView
    {
        UniTask InitializeAsync(
            IPopupPayload payload,
            CancellationToken cancellationToken);

        UniTask ShowAsync(CancellationToken cancellationToken);

        UniTask HideAsync(CancellationToken cancellationToken);

        UniTask CloseAsync(CancellationToken cancellationToken);
    }
}
