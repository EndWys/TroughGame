using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectCore.Template;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class TechnicalPrototypeContextInitializer : BaseGameSceneContextInitializer
    {
        public TechnicalPrototypeContextInitializer(IFeatureInitializationFlow featureInitializationFlow)
            : base(featureInitializationFlow)
        {
        }

        protected override UniTask OnAfterFeaturesAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
