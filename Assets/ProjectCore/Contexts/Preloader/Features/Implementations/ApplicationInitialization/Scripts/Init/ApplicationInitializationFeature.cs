using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System.Threading;
using Zenject;

namespace ProjectCore.Preloader
{
    public sealed class ApplicationInitializationFeature : IBaseFeature
    {
        public void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ApplicationInitializationFlow>().AsSingle();
        }

        public UniTask InitializeAsync(
            DiContainer container,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
