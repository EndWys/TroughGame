using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeContextInitializer : BaseContextInitializer, IGameSceneInitializer
    {
        public PrototypeContextInitializer(IFeatureInitializationFlow featureInitializationFlow)
            : base(featureInitializationFlow)
        {
        }

        protected override UniTask OnAfterFeaturesAsync(CancellationToken cancellationToken)
        {
            Debug.Log("Prototype Scene Initialized.");

            return UniTask.CompletedTask;
        }
    }
}
