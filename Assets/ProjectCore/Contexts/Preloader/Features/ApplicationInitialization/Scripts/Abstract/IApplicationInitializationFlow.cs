using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System.Threading;

namespace ProjectCore.Preloader
{
    public interface IApplicationInitializationFlow
    {
        UniTask RunAsync(
            BaseSceneDefinition initialSceneDefinition,
            CancellationToken applicationCancellationToken,
            CancellationToken preloaderCancellationToken);
    }
}
