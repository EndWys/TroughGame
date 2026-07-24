using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Project
{
    public interface IApplicationFlowCoordinator
    {
        string CurrentSceneName { get; }
        bool IsSceneReady { get; }

        UniTask LoadSceneAsync(string sceneName, CancellationToken cancellationToken);
    }
}
