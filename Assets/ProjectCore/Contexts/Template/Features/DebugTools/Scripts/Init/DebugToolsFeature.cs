using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugToolsFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private DebugToolsSettingsConfig _settingsConfig;

        private IDebugToolsService _debugToolsService;

#if UNITY_EDITOR
        internal IDebugToolsService EditorService => _debugToolsService;
#endif

        protected override void InstallBindings()
        {
            BindAsSingle<IDebugToolsService, DebugToolsService>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            _debugToolsService = Resolve<IDebugToolsService>();
            ResolveAs<IDebugToolsService, DebugToolsService>().Initialize(
                _settingsConfig,
                Resolve<ICommandLineService>());

            return UniTask.CompletedTask;
        }
    }
}
