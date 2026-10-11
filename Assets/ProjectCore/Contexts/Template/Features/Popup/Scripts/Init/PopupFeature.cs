using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class PopupFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private PopupComponent _popupComponent;
        [SerializeField] private PopupCatalogConfig _popupCatalog;

        protected override void InstallBindings()
        {
            BindInterfacesAsSingle<PopupSystem>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_popupComponent == null)
            {
                throw new InvalidOperationException("PopupFeature requires a PopupComponent.");
            }

            if (_popupCatalog == null)
            {
                throw new InvalidOperationException("PopupFeature requires a popup catalog.");
            }

            ResolveAs<IPopupSystem, PopupSystem>().Initialize(
                _popupComponent,
                _popupCatalog);

            return UniTask.CompletedTask;
        }
    }
}
