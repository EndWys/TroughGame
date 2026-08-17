using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IScreenView
    {
        UniTask InitializeAsync(
            IScreenSettings settings,
            CancellationToken cancellationToken);

        UniTask ShowAsync(CancellationToken cancellationToken);

        UniTask HideAsync(CancellationToken cancellationToken);

        UniTask CloseAsync(CancellationToken cancellationToken);
    }
}
