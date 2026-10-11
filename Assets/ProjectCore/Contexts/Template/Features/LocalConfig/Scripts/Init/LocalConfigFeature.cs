using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class LocalConfigFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private LocalConfigCatalogConfig _localConfigCatalog;

        protected override void InstallBindings()
        {
            BindInterfacesAndSelfAsSingle<LocalConfigService>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            ResolveAs<ILocalConfigService, LocalConfigService>().Initialize(_localConfigCatalog);
            return UniTask.CompletedTask;
        }
    }
}
