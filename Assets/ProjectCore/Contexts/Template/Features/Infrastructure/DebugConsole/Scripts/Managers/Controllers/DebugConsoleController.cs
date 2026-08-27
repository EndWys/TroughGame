using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleController : IDebugConsoleController, IDisposable
    {
        private const float PerformanceUpdateRateSeconds = 0.25f;
        private const float VisualizationUpdateRateSeconds = 0.2f;

        private readonly IDebugConsoleSystem _system;
        private readonly IDebugToolsService _debugToolsService;
        private readonly IDebugVisualizationSystem _debugVisualizationSystem;
        private readonly List<DebugConsoleLogData> _visibleLogs = new();

        private IDebugConsoleView _view;
        private DebugConsoleSettings _settings;
        private DebugVisualizationHudAdapter _visualizationAdapter;
        private CancellationTokenSource _viewCancellationTokenSource;
        private DebugConsoleLogFiltersData _logFilters = DebugConsoleLogFiltersData.Default;
        private DebugConsoleTabTypes _activeTab = DebugConsoleTabTypes.Logs;
        private string _commandInput = string.Empty;
        private int? _selectedLogID;
        private int _historyIndex;
        private float _nextPerformanceUpdateTime;
        private float _nextVisualizationUpdateTime;
        private bool _logsDirty;
        private bool _isVisible;
        private bool _isInitialized;

        public DebugConsoleController(
            IDebugConsoleSystem system,
            IDebugToolsService debugToolsService,
            IDebugVisualizationSystem debugVisualizationSystem)
        {
            _system = system;
            _debugToolsService = debugToolsService;
            _debugVisualizationSystem = debugVisualizationSystem;
        }

        public void Initialize(DebugConsoleSettings settings)
        {
            if (_isInitialized)
                throw new InvalidOperationException(
                    $"{nameof(DebugConsoleController)} is already initialized.");

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _isVisible = settings.OpenOnStart;
            _isInitialized = true;
        }

        public void Attach(IDebugConsoleView view)
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"{nameof(DebugConsoleController)} is not initialized.");
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (ReferenceEquals(_view, view))
                return;
            if (_view != null)
                Detach(_view);

            view.Initialize(_settings);
            _view = view;
            _viewCancellationTokenSource = new CancellationTokenSource();
            _historyIndex = _system.CommandHistory.Count;
            Subscribe();

            _view.SetVisible(_isVisible);
            _view.ShowTab(_activeTab);
            _view.SetCommandInput(_commandInput);
            _view.SetLogFilters(_logFilters);
            RenderLogs();
            RenderCheats();
            RenderAutocomplete();
            RenderActiveTab();
        }

        public void Detach(IDebugConsoleView view)
        {
            if (!ReferenceEquals(_view, view))
                return;

            Unsubscribe();
            _viewCancellationTokenSource.Cancel();
            _viewCancellationTokenSource.Dispose();
            _viewCancellationTokenSource = null;
            _view = null;
        }

        public void RefreshDebugVisualization()
        {
            if (_view != null && _activeTab == DebugConsoleTabTypes.DebugVisualization)
                RenderDebugVisualization();
        }

        public void Dispose()
        {
            if (_view != null)
                Detach(_view);
        }

        private void Subscribe()
        {
            _system.LogsChanged += OnLogsChanged;
            _system.CommandsChanged += OnCommandsChanged;
            _view.ToggleRequested += OnToggleRequested;
            _view.CloseRequested += OnCloseRequested;
            _view.TabSelected += OnTabSelected;
            _view.CommandSubmitted += OnCommandSubmitted;
            _view.CommandChanged += OnCommandChanged;
            _view.HistoryNavigationRequested += OnHistoryNavigationRequested;
            _view.LogFiltersChanged += OnLogFiltersChanged;
            _view.LogsClearRequested += OnLogsClearRequested;
            _view.LogSelected += OnLogSelected;
            _view.CheatSubmitted += OnCheatSubmitted;
            _view.VisualizationPathRequested += OnVisualizationPathRequested;
            _view.VisualizationNodeRequested += OnVisualizationNodeRequested;
            _view.VisualizationChannelsChanged += OnVisualizationChannelsChanged;
            _view.Ticked += OnTicked;
        }

        private void Unsubscribe()
        {
            _system.LogsChanged -= OnLogsChanged;
            _system.CommandsChanged -= OnCommandsChanged;
            _view.ToggleRequested -= OnToggleRequested;
            _view.CloseRequested -= OnCloseRequested;
            _view.TabSelected -= OnTabSelected;
            _view.CommandSubmitted -= OnCommandSubmitted;
            _view.CommandChanged -= OnCommandChanged;
            _view.HistoryNavigationRequested -= OnHistoryNavigationRequested;
            _view.LogFiltersChanged -= OnLogFiltersChanged;
            _view.LogsClearRequested -= OnLogsClearRequested;
            _view.LogSelected -= OnLogSelected;
            _view.CheatSubmitted -= OnCheatSubmitted;
            _view.VisualizationPathRequested -= OnVisualizationPathRequested;
            _view.VisualizationNodeRequested -= OnVisualizationNodeRequested;
            _view.VisualizationChannelsChanged -= OnVisualizationChannelsChanged;
            _view.Ticked -= OnTicked;
        }

        private void OnLogsChanged()
        {
            _logsDirty = true;
        }

        private void OnCommandsChanged()
        {
            RenderCheats();
            RenderAutocomplete();
        }

        private void OnToggleRequested()
        {
            SetVisible(!_isVisible);
        }

        private void OnCloseRequested()
        {
            SetVisible(false);
        }

        private void OnTabSelected(DebugConsoleTabTypes tab)
        {
            _activeTab = tab;
            _view.ShowTab(tab);
            RenderActiveTab();
        }

        private void OnCommandSubmitted(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            _commandInput = string.Empty;
            _historyIndex = _system.CommandHistory.Count + 1;
            _view.SetCommandInput(string.Empty);
            RenderAutocomplete();
            ExecuteCommandAsync(command, _viewCancellationTokenSource.Token).Forget();
        }

        private void OnCommandChanged(string command)
        {
            _commandInput = command ?? string.Empty;
            RenderAutocomplete();
        }

        private void OnHistoryNavigationRequested(int direction)
        {
            IReadOnlyList<string> history = _system.CommandHistory;
            if (history.Count == 0)
                return;

            _historyIndex = Mathf.Clamp(
                _historyIndex + direction,
                0,
                history.Count);
            _commandInput = _historyIndex == history.Count
                ? string.Empty
                : history[_historyIndex];
            _view.SetCommandInput(_commandInput);
            RenderAutocomplete();
        }

        private void OnLogFiltersChanged(DebugConsoleLogFiltersData filters)
        {
            _logFilters = filters;
            RenderLogs();
        }

        private void OnLogsClearRequested()
        {
            _selectedLogID = null;
            _system.ClearLogs();
        }

        private void OnLogSelected(int logID)
        {
            _selectedLogID = logID;
            RenderLogs();
        }

        private void OnCheatSubmitted(DebugConsoleCheatPayload payload)
        {
            OnCommandSubmitted(BuildCheatCommand(payload));
        }

        private void OnVisualizationPathRequested(string path)
        {
            EnsureVisualizationAdapter();
            _visualizationAdapter.NavigateTo(path);
            RenderDebugVisualization();
        }

        private void OnVisualizationNodeRequested(
            DebugVisualizationHudAdapter.ChannelTreeNode node)
        {
            EnsureVisualizationAdapter();
            if (node.HasChildren)
                _visualizationAdapter.NavigateTo(node.FullPath);
            else if (!string.IsNullOrEmpty(node.Channel))
            {
                _visualizationAdapter.SetChannelEnabled(
                    node.Channel,
                    !_visualizationAdapter.IsChannelEnabled(node.Channel));
            }

            RenderDebugVisualization();
        }

        private void OnVisualizationChannelsChanged(
            DebugVisualizationHudAdapter.ChannelTreeNode node,
            bool enabled)
        {
            EnsureVisualizationAdapter();
            _visualizationAdapter.SetChannelsEnabled(node, enabled);
            RenderDebugVisualization();
        }

        private void OnTicked(float unscaledTime)
        {
            if (!_isVisible)
                return;

            if (_activeTab == DebugConsoleTabTypes.Logs && _logsDirty)
                RenderLogs();

            if (_activeTab == DebugConsoleTabTypes.Performance
                && unscaledTime >= _nextPerformanceUpdateTime)
            {
                _nextPerformanceUpdateTime = unscaledTime + PerformanceUpdateRateSeconds;
                RenderPerformance();
            }

            if (_activeTab == DebugConsoleTabTypes.DebugVisualization
                && unscaledTime >= _nextVisualizationUpdateTime)
            {
                _nextVisualizationUpdateTime = unscaledTime + VisualizationUpdateRateSeconds;
                RenderDebugVisualizationValues();
            }
        }

        private async UniTask ExecuteCommandAsync(
            string command,
            CancellationToken cancellationToken)
        {
            try
            {
                await _system.ExecuteCommandAsync(command, cancellationToken);
                _historyIndex = _system.CommandHistory.Count;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }

        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            _view.SetVisible(visible);
            if (!visible)
                return;

            RenderActiveTab();
            _view.FocusCommandInput();
        }

        private void RenderActiveTab()
        {
            switch (_activeTab)
            {
                case DebugConsoleTabTypes.Logs:
                    RenderLogs();
                    break;
                case DebugConsoleTabTypes.Cheats:
                    RenderCheats();
                    break;
                case DebugConsoleTabTypes.DebugVisualization:
                    RenderDebugVisualization();
                    break;
                case DebugConsoleTabTypes.Performance:
                    RenderPerformance();
                    break;
            }
        }

        private void RenderLogs()
        {
            if (_view == null)
                return;

            _logsDirty = false;
            _visibleLogs.Clear();
            DebugConsoleLogData selectedLog = null;
            IReadOnlyList<DebugConsoleLogData> logs = _system.Logs;
            for (int i = 0; i < logs.Count; i++)
            {
                DebugConsoleLogData log = logs[i];
                if (!ShouldShowLog(log))
                    continue;

                _visibleLogs.Add(log);
                if (_selectedLogID.HasValue && log.ID == _selectedLogID.Value)
                    selectedLog = log;
            }

            if (_selectedLogID.HasValue && selectedLog == null)
                _selectedLogID = null;

            _view.RenderLogs(_visibleLogs, _selectedLogID);
            _view.RenderLogDetails(selectedLog == null
                ? "Select a log entry to see stacktrace/details."
                : string.IsNullOrWhiteSpace(selectedLog.StackTrace)
                    ? selectedLog.Message
                    : selectedLog.StackTrace);
        }

        private bool ShouldShowLog(DebugConsoleLogData log)
        {
            bool isTypeVisible = log.Type switch
            {
                LogType.Warning => _logFilters.ShowWarnings,
                LogType.Error => _logFilters.ShowErrors,
                LogType.Exception => _logFilters.ShowExceptions,
                LogType.Assert => _logFilters.ShowErrors,
                _ => _logFilters.ShowMessages
            };

            return isTypeVisible
                   && (string.IsNullOrWhiteSpace(_logFilters.Query)
                       || log.Message.Contains(
                           _logFilters.Query,
                           StringComparison.OrdinalIgnoreCase));
        }

        private void RenderCheats()
        {
            _view?.RenderCheats(_system.Commands);
        }

        private void RenderAutocomplete()
        {
            if (_view == null || string.IsNullOrWhiteSpace(_commandInput))
            {
                _view?.RenderAutocomplete(Array.Empty<CheatCommandDescriptor>());
                return;
            }

            string token = _commandInput
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? string.Empty;
            CheatCommandDescriptor[] matches = _system.Commands
                .Where(command => command.Name.Contains(
                    token,
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(command => command.Name, StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray();
            _view.RenderAutocomplete(matches);
        }

        private void RenderDebugVisualization()
        {
            if (_view == null)
                return;
            if (!_debugToolsService.IsEnabled(DebugToolTypes.Visualization)
                || !_debugVisualizationSystem.IsInitialized)
            {
                _view.RenderDebugVisualizationUnavailable();
                return;
            }

            EnsureVisualizationAdapter();
            DebugVisualizationHudAdapter.ChannelTreeNode root =
                _visualizationAdapter.BuildTree();
            DebugVisualizationHudAdapter.ChannelTreeNode current =
                _visualizationAdapter.ResolveCurrentNode(root);
            _view.RenderDebugVisualization(
                current,
                _visualizationAdapter.GetBreadcrumbParts(),
                _visualizationAdapter.BuildValueGroups());
        }

        private void RenderDebugVisualizationValues()
        {
            if (_view == null
                || !_debugToolsService.IsEnabled(DebugToolTypes.Visualization)
                || !_debugVisualizationSystem.IsInitialized)
            {
                return;
            }

            EnsureVisualizationAdapter();
            _view.RenderDebugVisualizationValues(
                _visualizationAdapter.BuildValueGroups());
        }

        private void EnsureVisualizationAdapter()
        {
            _visualizationAdapter ??=
                new DebugVisualizationHudAdapter(_debugVisualizationSystem);
        }

        private void RenderPerformance()
        {
            if (_view == null)
                return;

            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            _view.RenderPerformance(new DebugConsolePerformanceData(
                1f / deltaTime,
                deltaTime * 1000f,
                ToMegabytes(Profiler.GetMonoUsedSizeLong()),
                ToMegabytes(Profiler.GetTotalReservedMemoryLong()),
                ToMegabytes(Profiler.GetMonoHeapSizeLong())));
        }

        private static string BuildCheatCommand(DebugConsoleCheatPayload payload)
        {
            var parts = new List<string> { payload.Descriptor.Name };
            for (int i = 0; i < payload.Descriptor.Arguments.Count; i++)
            {
                string value = payload.ArgumentValues[i] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(value)
                    && payload.Descriptor.Arguments[i].IsOptional)
                {
                    continue;
                }

                parts.Add(CheatCommandFormatterUtility.QuoteArgument(value));
            }

            return string.Join(" ", parts);
        }

        private static float ToMegabytes(long bytes)
        {
            return bytes / (1024f * 1024f);
        }
    }
}
