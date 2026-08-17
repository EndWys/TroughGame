using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class ScreenNavigationFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private ScreenNavigationComponent _screenNavigationComponent;
        [SerializeField] private ScreenCatalogConfig _screenCatalog;

        protected override void InstallBindings()
        {
            BindAsSingle<IScreenNavigationSystem, ScreenNavigationSystem>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_screenNavigationComponent == null)
            {
                throw new InvalidOperationException(
                    "ScreenNavigationFeature requires a ScreenNavigationComponent.");
            }

            if (_screenCatalog == null)
            {
                throw new InvalidOperationException(
                    "ScreenNavigationFeature requires a screen catalog.");
            }

            ResolveAs<IScreenNavigationSystem, ScreenNavigationSystem>().Initialize(
                _screenNavigationComponent,
                _screenCatalog);

            return UniTask.CompletedTask;
        }
    }
}
