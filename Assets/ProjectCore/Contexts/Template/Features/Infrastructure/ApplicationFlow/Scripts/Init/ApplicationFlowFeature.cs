using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class ApplicationFlowFeature : IBaseFeature
    {
        public void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ApplicationFlowCoordinator>().AsSingle();
        }

        public UniTask InitializeAsync(
            DiContainer container,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
