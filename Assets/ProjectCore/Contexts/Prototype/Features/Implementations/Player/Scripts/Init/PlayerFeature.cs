using Cysharp.Threading.Tasks;
using System.Threading;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.Prototype
{
    public sealed class PlayerFeature : IBaseFeature
    {
        public void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<PlayerNetworkEntityFactory>().AsSingle();
        }

        public UniTask InitializeAsync(
            DiContainer container,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
