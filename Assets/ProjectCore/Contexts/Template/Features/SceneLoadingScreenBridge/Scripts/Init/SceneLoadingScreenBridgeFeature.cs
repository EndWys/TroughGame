using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public sealed class SceneLoadingScreenBridgeFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<ISceneTransitionPresenter, SceneLoadingScreenAdapter>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
