using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IGameSceneLifecycle : IContextInitializer
    {
        UniTask ExitAsync(CancellationToken cancellationToken);
    }
}
