using Cysharp.Threading.Tasks;
using System.Threading;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeFeature : IBaseFeature
    {
        public void InstallBindings(DiContainer container)
        {
        }

        public UniTask InitializeAsync(
            DiContainer container,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
