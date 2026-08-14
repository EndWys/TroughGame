using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IScreenNavigationSystem
    {
        bool CanGoBack { get; }
        bool IsTransitioning { get; }
        Type CurrentScreenType { get; }

        UniTask<Result> OpenRootAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseScreenView<TSettings>
            where TSettings : IScreenSettings;

        UniTask<Result> OpenChildAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseScreenView<TSettings>
            where TSettings : IScreenSettings;

        UniTask<Result> NavigateAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseScreenView<TSettings>
            where TSettings : IScreenSettings;

        UniTask<Result> GoBackAsync(CancellationToken cancellationToken);
    }
}
