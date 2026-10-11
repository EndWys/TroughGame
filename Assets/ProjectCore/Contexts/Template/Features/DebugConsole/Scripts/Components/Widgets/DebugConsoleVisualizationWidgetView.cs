using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleVisualizationWidgetView : VisualElement
    {
        private readonly Action<string> _pathRequested;
        private readonly Action<DebugVisualizationHUDAdapter.ChannelTreeNode> _nodeRequested;
        private readonly Action<DebugVisualizationHUDAdapter.ChannelTreeNode, bool>
            _channelsChanged;
        private readonly ScrollView _channels;
        private readonly ScrollView _values;
        private readonly VisualElement _breadcrumbs;

        public DebugConsoleVisualizationWidgetView(
            Action<string> pathRequested,
            Action<DebugVisualizationHUDAdapter.ChannelTreeNode> nodeRequested,
            Action<DebugVisualizationHUDAdapter.ChannelTreeNode, bool> channelsChanged)
        {
            _pathRequested = pathRequested;
            _nodeRequested = nodeRequested;
            _channelsChanged = channelsChanged;
            _channels = new ScrollView();
            _values = new ScrollView();
            _breadcrumbs = new VisualElement();

            BuildUI();
        }

        public VisualElement Root => this;

        public void Render(
            DebugVisualizationHUDAdapter.ChannelTreeNode currentNode,
            IReadOnlyList<string> breadcrumbParts,
            IReadOnlyDictionary<string, List<DebugVisualizationHUDValueData>> valueGroups)
        {
            _breadcrumbs.Clear();
            _channels.Clear();

            var rootButton = new Button(() => _pathRequested?.Invoke(string.Empty))
            {
                text = "Channels"
            };
            _breadcrumbs.Add(rootButton);

            string currentPath = string.Empty;
            foreach (string part in breadcrumbParts)
            {
                _breadcrumbs.Add(new Label(">"));
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? part
                    : $"{currentPath}/{part}";
                string path = currentPath;
                _breadcrumbs.Add(new Button(() => _pathRequested?.Invoke(path))
                {
                    text = part
                });
            }

            foreach (DebugVisualizationHUDAdapter.ChannelTreeNode node in currentNode.Children)
                DrawChannelNode(node);

            RenderValues(valueGroups);
        }

        public void RenderValues(
            IReadOnlyDictionary<string, List<DebugVisualizationHUDValueData>> valueGroups)
        {
            _values.Clear();
            foreach (KeyValuePair<string, List<DebugVisualizationHUDValueData>> pair in valueGroups)
            {
                _values.Add(new Label(pair.Key));
                foreach (DebugVisualizationHUDValueData value in pair.Value)
                    _values.Add(new Label($"  {value.Label}: {value.Value}"));
            }
        }

        public void RenderUnavailable()
        {
            _breadcrumbs.Clear();
            _channels.Clear();
            _values.Clear();
            _values.Add(new Label("Debug Visualization is disabled."));
        }

        private void BuildUI()
        {
            name = "DebugVisualization";
            AddToClassList("console-tab");
            AddToClassList("console-dviz-root");
            style.display = DisplayStyle.None;

            _breadcrumbs.AddToClassList("console-dviz-breadcrumbs");
            Add(_breadcrumbs);

            _channels.AddToClassList("console-dviz-channels");
            _values.AddToClassList("console-dviz-values");

            var split = new TwoPaneSplitView(
                0,
                300,
                TwoPaneSplitViewOrientation.Horizontal);
            split.AddToClassList("console-dviz-split");
            split.Add(_channels);
            split.Add(_values);
            Add(split);
        }

        private void DrawChannelNode(
            DebugVisualizationHUDAdapter.ChannelTreeNode node)
        {
            var row = new VisualElement();
            row.AddToClassList("console-dviz-channel-row");
            _channels.Add(row);

            var toggle = new Toggle();
            toggle.SetValueWithoutNotify(AreAllChannelsEnabled(node));
            toggle.RegisterValueChangedCallback(changeEvent =>
                _channelsChanged?.Invoke(node, changeEvent.newValue));
            row.Add(toggle);

            string label = node.HasChildren ? $"{node.Name} >" : node.Name;
            row.Add(new Button(() => _nodeRequested?.Invoke(node)) { text = label });
        }

        private static bool AreAllChannelsEnabled(
            DebugVisualizationHUDAdapter.ChannelTreeNode node)
        {
            if (node.Channel != null && !node.IsEnabled)
                return false;

            foreach (DebugVisualizationHUDAdapter.ChannelTreeNode child in node.Children)
            {
                if (!AreAllChannelsEnabled(child))
                    return false;
            }

            return true;
        }
    }
}
