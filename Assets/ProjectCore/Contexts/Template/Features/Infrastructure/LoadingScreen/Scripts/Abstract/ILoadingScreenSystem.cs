using Cysharp.Threading.Tasks;
using Domain;
using System.Threading;

namespace ProjectCore.Template
{
    public interface ILoadingScreenSystem
    {
        bool IsVisible { get; }
        bool IsTransitioning { get; }

        UniTask<Result> ShowAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseLoadingScreenView<TSettings>
            where TSettings : ILoadingScreenSettings;

        UniTask<Result> ShowDefaultAsync(CancellationToken cancellationToken);

        UniTask<Result> HideAsync(CancellationToken cancellationToken);
    }
}
