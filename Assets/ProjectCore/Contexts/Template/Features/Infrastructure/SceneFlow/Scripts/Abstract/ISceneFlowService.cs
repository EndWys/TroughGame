using Cysharp.Threading.Tasks;
using Domain;
using System.Threading;

namespace ProjectCore.Template
{
    public interface ISceneFlowService
    {
        bool IsTransitioning { get; }
        BaseSceneDefinition CurrentSceneDefinition { get; }

        UniTask<Result> LoadAsync<TScene, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScene : IScene<TSettings>
            where TSettings : ISceneSettings;

        UniTask<Result> LoadAsync(
            BaseSceneDefinition sceneDefinition,
            ISceneSettings settings,
            CancellationToken cancellationToken);
    }
}
