using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;

namespace ProjectCore.Template
{
    public interface IBaseFeature
    {
        void InstallBindings(DiContainer container);
        UniTask InitializeAsync(DiContainer container, CancellationToken cancellationToken);
    }
}
