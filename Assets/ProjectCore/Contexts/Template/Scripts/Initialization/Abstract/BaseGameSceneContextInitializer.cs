using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public abstract class BaseGameSceneContextInitializer : BaseContextInitializer, IGameSceneLifecycle
    {
        protected BaseGameSceneContextInitializer(IFeatureInitializationFlow featureInitializationFlow)
            : base(featureInitializationFlow)
        {
        }

        public UniTask ExitAsync(CancellationToken cancellationToken)
        {
            return OnExitAsync(cancellationToken);
        }

        protected virtual UniTask OnExitAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
