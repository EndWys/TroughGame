using Cysharp.Threading.Tasks;
using System.Threading;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class MovementFeature : IBaseFeature
    {
        public void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<MovementSystem>().AsSingle();
        }

        public UniTask InitializeAsync(
            DiContainer container,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
