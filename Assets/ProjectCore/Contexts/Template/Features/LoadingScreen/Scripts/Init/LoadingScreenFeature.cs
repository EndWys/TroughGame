using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class LoadingScreenFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private LoadingScreenComponent _loadingScreenComponent;
        [SerializeField] private LoadingScreenCatalogConfig _loadingScreenCatalog;

        protected override void InstallBindings()
        {
            BindInterfacesAsSingle<LoadingScreenSystem>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_loadingScreenComponent == null)
            {
                throw new InvalidOperationException(
                    "LoadingScreenFeature requires a LoadingScreenComponent.");
            }

            if (_loadingScreenCatalog == null)
            {
                throw new InvalidOperationException(
                    "LoadingScreenFeature requires a loading screen catalog.");
            }

            ResolveAs<ILoadingScreenSystem, LoadingScreenSystem>().Initialize(
                _loadingScreenComponent,
                _loadingScreenCatalog);

            return UniTask.CompletedTask;
        }
    }
}
