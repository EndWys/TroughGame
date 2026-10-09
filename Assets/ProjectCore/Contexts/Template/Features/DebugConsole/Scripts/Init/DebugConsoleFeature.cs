using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugConsoleFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private DebugConsoleComponent _debugConsolePrefab;

        private IDebugToolsService _debugToolsService;
        private IDebugConsoleSystem _debugConsoleSystem;
        private IDebugConsoleController _debugConsoleController;
        private DebugConsoleComponent _debugConsole;

        private void OnDestroy()
        {
            if (_debugToolsService != null)
                _debugToolsService.ToolStateChanged -= OnToolStateChanged;

            DestroyConsole();
        }

        protected override void InstallBindings()
        {
            BindInterfacesAsSingle<DebugConsoleSystem>();
            BindInterfacesAsSingle<DebugConsoleController>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            _debugToolsService = Resolve<IDebugToolsService>();
            _debugConsoleSystem = Resolve<IDebugConsoleSystem>();
            _debugConsoleController = Resolve<IDebugConsoleController>();

            DebugConsoleSettings settings = _debugToolsService.ConsoleSettings;
            _debugConsoleSystem.Initialize(
                settings,
                _debugToolsService.IsAllowed(DebugToolTypes.Console));
            _debugConsoleController.Initialize(settings);

            _debugToolsService.ToolStateChanged += OnToolStateChanged;
            ApplyState(_debugToolsService.IsEnabled(DebugToolTypes.Console));
            return UniTask.CompletedTask;
        }

        private void OnToolStateChanged(DebugToolTypes tool, bool enabled)
        {
            if (tool == DebugToolTypes.Console)
            {
                ApplyState(enabled);
                return;
            }

            if (tool == DebugToolTypes.Visualization)
                _debugConsoleController.RefreshDebugVisualization();
        }

        private void ApplyState(bool enabled)
        {
            if (enabled)
                CreateConsole();
            else
                DestroyConsole();
        }

        private void CreateConsole()
        {
            if (_debugConsole != null)
                return;

            _debugConsole = Resolve<IClassFactory>()
                .CreateMonoBehaviour(_debugConsolePrefab, null);
            _debugConsole.transform.SetParent(null, false);
            DontDestroyOnLoad(_debugConsole.gameObject);
            _debugConsoleController.Attach(_debugConsole);
        }

        private void DestroyConsole()
        {
            if (_debugConsole == null)
                return;

            _debugConsoleController?.Detach(_debugConsole);
            Destroy(_debugConsole.gameObject);
            _debugConsole = null;
        }
    }
}
