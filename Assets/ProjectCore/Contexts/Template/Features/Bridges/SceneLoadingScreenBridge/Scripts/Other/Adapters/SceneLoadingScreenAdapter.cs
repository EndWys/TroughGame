using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Threading;

namespace ProjectCore.Template
{
    public sealed class SceneLoadingScreenAdapter : ISceneTransitionPresenter
    {
        private readonly ILoadingScreenSystem _loadingScreenSystem;

        private bool _isLoadingScreenOwned;

        public SceneLoadingScreenAdapter(ILoadingScreenSystem loadingScreenSystem)
        {
            _loadingScreenSystem = loadingScreenSystem
                ?? throw new ArgumentNullException(nameof(loadingScreenSystem));
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            Result result = await _loadingScreenSystem.ShowDefaultAsync(
                cancellationToken);

            ThrowIfFailed(result);
            _isLoadingScreenOwned = true;
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            if (!_isLoadingScreenOwned)
            {
                return;
            }

            Result result = await _loadingScreenSystem.HideAsync(cancellationToken);

            ThrowIfFailed(result);
            _isLoadingScreenOwned = false;
        }

        private static void ThrowIfFailed(Result result)
        {
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.FirstError.Message);
            }
        }
    }
}
