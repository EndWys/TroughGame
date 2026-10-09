using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    internal interface IDebugConsoleView
    {
        public event Action ToggleRequested;
        public event Action CloseRequested;
        public event Action<DebugConsoleTabTypes> TabSelected;
        public event Action<string> CommandSubmitted;
        public event Action<string> CommandChanged;
        public event Action<int> HistoryNavigationRequested;
        public event Action<DebugConsoleLogFiltersData> LogFiltersChanged;
        public event Action LogsClearRequested;
        public event Action<int> LogSelected;
        public event Action<DebugConsoleCheatPayload> CheatSubmitted;
        public event Action<string> VisualizationPathRequested;
        public event Action<DebugVisualizationHudAdapter.ChannelTreeNode> VisualizationNodeRequested;
        public event Action<DebugVisualizationHudAdapter.ChannelTreeNode, bool> VisualizationChannelsChanged;
        public event Action<float> Ticked;

        public void Initialize(DebugConsoleSettings settings);
        public void SetVisible(bool visible);
        public void ShowTab(DebugConsoleTabTypes tab);
        public void RenderLogs(
            IReadOnlyList<DebugConsoleLogData> logs,
            int? selectedLogID);
        public void RenderLogDetails(string details);
        public void RenderAutocomplete(IReadOnlyList<CheatCommandDescriptor> commands);
        public void RenderCheats(IReadOnlyList<CheatCommandDescriptor> commands);
        public void SetCommandInput(string command);
        public void SetLogFilters(DebugConsoleLogFiltersData filters);
        public void RenderDebugVisualization(
            DebugVisualizationHudAdapter.ChannelTreeNode currentNode,
            IReadOnlyList<string> breadcrumbParts,
            IReadOnlyDictionary<string, List<DebugVisualizationHudValueData>> valueGroups);
        public void RenderDebugVisualizationValues(
            IReadOnlyDictionary<string, List<DebugVisualizationHudValueData>> valueGroups);
        public void RenderDebugVisualizationUnavailable();
        public void RenderPerformance(DebugConsolePerformanceData data);
        public void FocusCommandInput();
    }
}
