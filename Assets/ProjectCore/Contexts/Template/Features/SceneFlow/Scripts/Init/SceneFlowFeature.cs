using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class SceneFlowFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private SceneCatalogConfig _sceneCatalog;

        protected override void InstallBindings()
        {
            BindAsSingle<ISceneFlowService, SceneFlowService>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_sceneCatalog == null)
            {
                throw new InvalidOperationException("SceneFlowFeature requires a scene catalog.");
            }

            ResolveAs<ISceneFlowService, SceneFlowService>().Initialize(_sceneCatalog);

            return UniTask.CompletedTask;
        }
    }
}
