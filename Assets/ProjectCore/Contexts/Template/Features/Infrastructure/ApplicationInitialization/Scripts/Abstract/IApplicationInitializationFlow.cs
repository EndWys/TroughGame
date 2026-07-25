using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IApplicationInitializationFlow
    {
        UniTask RunAsync(
            string initialSceneName,
            CancellationToken applicationCancellationToken,
            CancellationToken preloaderCancellationToken);
    }
}
