using Cysharp.Threading.Tasks;
using Domain;
using System.Threading;

namespace ProjectCore.Template
{
    public interface ISceneFlowService
    {
        bool IsTransitioning { get; }
        BaseSceneDefinition CurrentSceneDefinition { get; }

        UniTask<Result> LoadAsync<TScene, TPayload>(
            TPayload payload,
            CancellationToken cancellationToken)
            where TScene : IScene<TPayload>
            where TPayload : IScenePayload;

        UniTask<Result> LoadAsync(
            BaseSceneDefinition sceneDefinition,
            IScenePayload payload,
            CancellationToken cancellationToken);
    }
}
