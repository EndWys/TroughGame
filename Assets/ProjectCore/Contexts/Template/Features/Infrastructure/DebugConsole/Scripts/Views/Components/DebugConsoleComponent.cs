using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class DebugConsoleComponent : MonoBehaviour, IDebugConsoleView
    {
        private const float TopSortingOrder = 32000f;

        [Header("Settings")]
        [SerializeField] private int _fontSize = 12;
        [Header("References")]
        [SerializeField] private UIDocument _document;
        [SerializeField] private PanelSettings _panelSettings;
        [SerializeField] private StyleSheet _styleSheet;

        private Action _toggleRequested;
        private Action _closeRequested;
        private Action<DebugConsoleTabTypes> _tabSelected;
        private Action<string> _commandSubmitted;
        private Action<string> _commandChanged;
        private Action<int> _historyNavigationRequested;
        private Action<DebugConsoleLogFiltersData> _logFiltersChanged;
        private Action _logsClearRequested;
        private Action<int> _logSelected;
        private Action<DebugConsoleCheatPayload> _cheatSubmitted;
        private Action<string> _visualizationPathRequested;
        private Action<DebugVisualizationHudAdapter.ChannelTreeNode>
            _visualizationNodeRequested;
        private Action<DebugVisualizationHudAdapter.ChannelTreeNode, bool>
            _visualizationChannelsChanged;
        private Action<float> _ticked;

        private VisualElement _window;
        private Label _statusLabel;
        private Button _logsTabButton;
        private Button _cheatsTabButton;
        private Button _debugVisualizationTabButton;
        private Button _performanceTabButton;
        private Button _launcherButton;
        private DebugConsoleLogsWidgetView _logsTab;
        private DebugConsoleCheatsWidgetView _cheatsTab;
        private DebugConsoleVisualizationWidgetView _debugVisualizationTab;
        private DebugConsolePerformanceWidgetView _performanceTab;
        private KeyCode _toggleKey;
        private bool _isInitialized;

        event Action IDebugConsoleView.ToggleRequested
        {
            add => _toggleRequested += value;
            remove => _toggleRequested -= value;
        }

        event Action IDebugConsoleView.CloseRequested
        {
            add => _closeRequested += value;
            remove => _closeRequested -= value;
        }

        event Action<DebugConsoleTabTypes> IDebugConsoleView.TabSelected
        {
            add => _tabSelected += value;
            remove => _tabSelected -= value;
        }

        event Action<string> IDebugConsoleView.CommandSubmitted
        {
            add => _commandSubmitted += value;
            remove => _commandSubmitted -= value;
        }

        event Action<string> IDebugConsoleView.CommandChanged
        {
            add => _commandChanged += value;
            remove => _commandChanged -= value;
        }

        event Action<int> IDebugConsoleView.HistoryNavigationRequested
        {
            add => _historyNavigationRequested += value;
            remove => _historyNavigationRequested -= value;
        }

        event Action<DebugConsoleLogFiltersData> IDebugConsoleView.LogFiltersChanged
        {
            add => _logFiltersChanged += value;
            remove => _logFiltersChanged -= value;
        }

        event Action IDebugConsoleView.LogsClearRequested
        {
            add => _logsClearRequested += value;
            remove => _logsClearRequested -= value;
        }

        event Action<int> IDebugConsoleView.LogSelected
        {
            add => _logSelected += value;
            remove => _logSelected -= value;
        }

        event Action<DebugConsoleCheatPayload> IDebugConsoleView.CheatSubmitted
        {
            add => _cheatSubmitted += value;
            remove => _cheatSubmitted -= value;
        }

        event Action<string> IDebugConsoleView.VisualizationPathRequested
        {
            add => _visualizationPathRequested += value;
            remove => _visualizationPathRequested -= value;
        }

        event Action<DebugVisualizationHudAdapter.ChannelTreeNode>
            IDebugConsoleView.VisualizationNodeRequested
        {
            add => _visualizationNodeRequested += value;
            remove => _visualizationNodeRequested -= value;
        }

        event Action<DebugVisualizationHudAdapter.ChannelTreeNode, bool>
            IDebugConsoleView.VisualizationChannelsChanged
        {
            add => _visualizationChannelsChanged += value;
            remove => _visualizationChannelsChanged -= value;
        }

        event Action<float> IDebugConsoleView.Ticked
        {
            add => _ticked += value;
            remove => _ticked -= value;
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            if (Input.GetKeyDown(_toggleKey))
                _toggleRequested?.Invoke();

            _ticked?.Invoke(Time.unscaledTime);
        }

        void IDebugConsoleView.Initialize(DebugConsoleSettings settings)
        {
            if (_isInitialized)
                return;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (_document == null || _panelSettings == null || _styleSheet == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DebugConsoleComponent)} has missing serialized UI references.");
            }

            _toggleKey = settings.ToggleKey;
            _panelSettings.sortingOrder = TopSortingOrder;
            _document.sortingOrder = TopSortingOrder;
            BuildUi();
            _isInitialized = true;
        }

        void IDebugConsoleView.SetVisible(bool visible)
        {
            _window.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_launcherButton != null)
            {
                _launcherButton.style.display = visible
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        void IDebugConsoleView.ShowTab(DebugConsoleTabTypes tab)
        {
            _logsTab.Root.style.display = tab == DebugConsoleTabTypes.Logs
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _cheatsTab.Root.style.display = tab == DebugConsoleTabTypes.Cheats
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _debugVisualizationTab.Root.style.display =
                tab == DebugConsoleTabTypes.DebugVisualization
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            _performanceTab.Root.style.display = tab == DebugConsoleTabTypes.Performance
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            SetTabActive(_logsTabButton, tab == DebugConsoleTabTypes.Logs);
            SetTabActive(_cheatsTabButton, tab == DebugConsoleTabTypes.Cheats);
            SetTabActive(
                _debugVisualizationTabButton,
                tab == DebugConsoleTabTypes.DebugVisualization);
            SetTabActive(_performanceTabButton, tab == DebugConsoleTabTypes.Performance);
            _statusLabel.text = tab.ToString();
        }

        void IDebugConsoleView.RenderLogs(
            IReadOnlyList<DebugConsoleLogData> logs,
            int? selectedLogID)
        {
            _logsTab.RenderLogs(logs, selectedLogID);
        }

        void IDebugConsoleView.RenderLogDetails(string details)
        {
            _logsTab.RenderDetails(details);
        }

        void IDebugConsoleView.RenderAutocomplete(
            IReadOnlyList<CheatCommandDescriptor> commands)
        {
            _logsTab.RenderAutocomplete(commands);
        }

        void IDebugConsoleView.RenderCheats(
            IReadOnlyList<CheatCommandDescriptor> commands)
        {
            _cheatsTab.Render(commands);
        }

        void IDebugConsoleView.SetCommandInput(string command)
        {
            _logsTab.SetCommandInput(command);
        }

        void IDebugConsoleView.SetLogFilters(DebugConsoleLogFiltersData filters)
        {
            _logsTab.SetFilters(filters);
        }

        void IDebugConsoleView.RenderDebugVisualization(
            DebugVisualizationHudAdapter.ChannelTreeNode currentNode,
            IReadOnlyList<string> breadcrumbParts,
            IReadOnlyDictionary<string, List<DebugVisualizationHudValueData>> valueGroups)
        {
            _debugVisualizationTab.Render(currentNode, breadcrumbParts, valueGroups);
        }

        void IDebugConsoleView.RenderDebugVisualizationValues(
            IReadOnlyDictionary<string, List<DebugVisualizationHudValueData>> valueGroups)
        {
            _debugVisualizationTab.RenderValues(valueGroups);
        }

        void IDebugConsoleView.RenderDebugVisualizationUnavailable()
        {
            _debugVisualizationTab.RenderUnavailable();
        }

        void IDebugConsoleView.RenderPerformance(DebugConsolePerformanceData data)
        {
            _performanceTab.Render(data);
        }

        void IDebugConsoleView.FocusCommandInput()
        {
            _logsTab.FocusInput();
        }

        private void BuildUi()
        {
            _document.panelSettings = _panelSettings;

            VisualElement root = _document.rootVisualElement;
            root.Clear();
            root.styleSheets.Clear();
            root.styleSheets.Add(_styleSheet);
            root.AddToClassList("console-root");

            _window = new VisualElement();
            _window.AddToClassList("console-window");
            root.Add(_window);

            BuildHeader();

            var content = new VisualElement();
            content.AddToClassList("console-content");
            _window.Add(content);

            _logsTab = new DebugConsoleLogsWidgetView(
                command => _commandSubmitted?.Invoke(command),
                command => _commandChanged?.Invoke(command),
                direction => _historyNavigationRequested?.Invoke(direction),
                filters => _logFiltersChanged?.Invoke(filters),
                () => _logsClearRequested?.Invoke(),
                logID => _logSelected?.Invoke(logID),
                _fontSize);
            _cheatsTab = new DebugConsoleCheatsWidgetView(
                payload => _cheatSubmitted?.Invoke(payload));
            _debugVisualizationTab = new DebugConsoleVisualizationWidgetView(
                path => _visualizationPathRequested?.Invoke(path),
                node => _visualizationNodeRequested?.Invoke(node),
                (node, value) => _visualizationChannelsChanged?.Invoke(node, value));
            _performanceTab = new DebugConsolePerformanceWidgetView();

            content.Add(_logsTab.Root);
            content.Add(_cheatsTab.Root);
            content.Add(_debugVisualizationTab.Root);
            content.Add(_performanceTab.Root);
            BuildLauncher(root);
        }

        private void BuildHeader()
        {
            var header = new VisualElement();
            header.AddToClassList("console-header");
            _window.Add(header);

            _logsTabButton = CreateTabButton(
                "Logs",
                DebugConsoleTabTypes.Logs);
            _cheatsTabButton = CreateTabButton(
                "Cheats",
                DebugConsoleTabTypes.Cheats);
            _debugVisualizationTabButton = CreateTabButton(
                "DebugViz",
                DebugConsoleTabTypes.DebugVisualization);
            _performanceTabButton = CreateTabButton(
                "Performance",
                DebugConsoleTabTypes.Performance);
            header.Add(_logsTabButton);
            header.Add(_cheatsTabButton);
            header.Add(_debugVisualizationTabButton);
            header.Add(_performanceTabButton);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            header.Add(spacer);

            _statusLabel = new Label();
            _statusLabel.AddToClassList("console-status-label");
            header.Add(_statusLabel);

            var versionLabel = new Label($"v{Application.version}");
            versionLabel.AddToClassList("console-version-label");
            header.Add(versionLabel);

            var closeButton = new Button(() => _closeRequested?.Invoke()) { text = "Close" };
            closeButton.AddToClassList("console-close-button");
            header.Add(closeButton);
        }

        private Button CreateTabButton(string title, DebugConsoleTabTypes tab)
        {
            var button = new Button(() => _tabSelected?.Invoke(tab)) { text = title };
            button.AddToClassList("console-tab-button");
            return button;
        }

        private void BuildLauncher(VisualElement root)
        {
            _launcherButton = new Button(() => _toggleRequested?.Invoke()) { text = "Console" };
            _launcherButton.AddToClassList("console-launcher-button");
            root.Add(_launcherButton);
            SetupLauncherDrag();
        }

        private void SetupLauncherDrag()
        {
            bool isDragging = false;
            Vector3 pointerStart = default;
            float leftStart = 8f;
            float topStart = 8f;

            _launcherButton.RegisterCallback<PointerDownEvent>(pointerEvent =>
            {
                isDragging = true;
                pointerStart = pointerEvent.position;
                leftStart = _launcherButton.resolvedStyle.left;
                topStart = _launcherButton.resolvedStyle.top;
                _launcherButton.CapturePointer(pointerEvent.pointerId);
                pointerEvent.StopPropagation();
            });

            _launcherButton.RegisterCallback<PointerMoveEvent>(pointerEvent =>
            {
                if (!isDragging)
                    return;

                Vector3 delta = pointerEvent.position - pointerStart;
                _launcherButton.style.left = Mathf.Max(0f, leftStart + delta.x);
                _launcherButton.style.top = Mathf.Max(0f, topStart + delta.y);
                pointerEvent.StopPropagation();
            });

            _launcherButton.RegisterCallback<PointerUpEvent>(pointerEvent =>
            {
                isDragging = false;
                _launcherButton.ReleasePointer(pointerEvent.pointerId);
                pointerEvent.StopPropagation();
            });
        }

        private static void SetTabActive(Button button, bool isActive)
        {
            button.RemoveFromClassList("console-tab-button-active");
            button.RemoveFromClassList("console-tab-button-inactive");
            button.AddToClassList(isActive
                ? "console-tab-button-active"
                : "console-tab-button-inactive");
        }
    }
}
