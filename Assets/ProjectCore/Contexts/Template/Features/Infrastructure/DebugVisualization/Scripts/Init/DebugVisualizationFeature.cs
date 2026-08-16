using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class DebugVisualizationFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private DebugVisualizationSettingsConfig _settingsConfig;
        [SerializeField] private PanelSettings _panelSettings;
        [SerializeField] private StyleSheet _hudStyleSheet;

        private IDebugToolsService _debugToolsService;
        private IDebugVisualizationBackend _debugVisualizationBackend;

        private void OnDestroy()
        {
            if (_debugToolsService != null)
                _debugToolsService.ToolStateChanged -= OnToolStateChanged;
            if (_debugVisualizationBackend == null)
                return;

            DebugVisualizationUtility.Detach(_debugVisualizationBackend);
            _debugVisualizationBackend.Shutdown();
        }

        protected override void InstallBindings()
        {
            BindAsSingle<IDebugVisualizationRegistry, DebugVisualizationRegistry>();
            BindInterfacesAsSingle<DebugVisualizationSystem>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            _debugToolsService = Resolve<IDebugToolsService>();
            _debugVisualizationBackend = Resolve<IDebugVisualizationBackend>();
            DebugVisualizationUtility.Attach(_debugVisualizationBackend);

            _debugToolsService.ToolStateChanged += OnToolStateChanged;
            ApplyState(_debugToolsService.IsEnabled(DebugToolTypes.Visualization));
            return UniTask.CompletedTask;
        }

        private void OnToolStateChanged(DebugToolTypes tool, bool enabled)
        {
            if (tool == DebugToolTypes.Visualization)
                ApplyState(enabled);
        }

        private void ApplyState(bool enabled)
        {
            if (enabled)
            {
                _debugVisualizationBackend.Initialize(
                    _settingsConfig,
                    _panelSettings,
                    _hudStyleSheet);
                return;
            }

            _debugVisualizationBackend.Shutdown();
        }
    }
}
