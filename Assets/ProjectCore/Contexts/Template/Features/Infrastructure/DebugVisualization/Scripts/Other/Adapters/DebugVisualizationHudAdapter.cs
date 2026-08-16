using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    internal sealed class DebugVisualizationHudAdapter
    {
        private readonly IDebugVisualizationSystem _system;
        private string _currentChannelPath = string.Empty;
        private bool _useChannelValueColors;

        public DebugVisualizationHudAdapter(IDebugVisualizationSystem system)
        {
            _system = system;
        }

        public string CurrentChannelPath => _currentChannelPath;
        public bool UseChannelValueColors => _useChannelValueColors;

        public void ResetPath()
        {
            _currentChannelPath = string.Empty;
        }

        public void Refresh()
        {
            View?.RequestRenderFull();
        }

        public DebugVisualizationHudWidgetView View { get; set; }

        public void RefreshValuesOnly()
        {
            View?.RequestRenderValuesOnly();
        }

        public void SetUseChannelValueColors(bool enabled)
        {
            if (_useChannelValueColors == enabled)
                return;

            _useChannelValueColors = enabled;
            View?.RequestRenderValuesOnly();
        }

        public void RequestFullRefresh()
        {
            View?.RequestRenderFull();
        }

        public ChannelTreeNode BuildTree()
        {
            return ChannelTreeNode.Build(
                _system.ChannelNames,
                _system.IsChannelEnabled);
        }

        public ChannelTreeNode ResolveCurrentNode(ChannelTreeNode root)
        {
            var node = root.Find(_currentChannelPath);
            if (node != null)
                return node;

            _currentChannelPath = string.Empty;
            return root;
        }

        public IReadOnlyList<string> GetBreadcrumbParts()
        {
            if (string.IsNullOrEmpty(_currentChannelPath))
                return Array.Empty<string>();

            return _currentChannelPath.Split('/');
        }

        public void NavigateRoot()
        {
            _currentChannelPath = string.Empty;
            View?.RequestRenderFull();
        }

        public void NavigateTo(string path)
        {
            _currentChannelPath = path ?? string.Empty;
            View?.RequestRenderFull();
        }

        public bool IsChannelEnabled(string channel)
        {
            return _system.IsChannelEnabled(channel);
        }

        public void SetChannelEnabled(string channel, bool enabled)
        {
            _system.SetChannelEnabled(channel, enabled);
            View?.RequestRenderFull();
        }

        public bool AreAllChannelsEnabled(ChannelTreeNode node)
        {
            if (node.Channel != null && !_system.IsChannelEnabled(node.Channel))
                return false;

            for (var i = 0; i < node.Children.Count; i++)
            {
                if (!AreAllChannelsEnabled(node.Children[i]))
                    return false;
            }

            return true;
        }

        public void SetChannelsEnabled(ChannelTreeNode node, bool enabled)
        {
            if (node.Channel != null)
                _system.SetChannelEnabled(node.Channel, enabled);

            for (var i = 0; i < node.Children.Count; i++)
                SetChannelsEnabled(node.Children[i], enabled);
        }

        public IReadOnlyDictionary<string, List<DebugVisualizationHudValueData>> BuildValueGroups()
        {
            var groups = new SortedDictionary<string, List<DebugVisualizationHudValueData>>();
            var values = _system.PersistentValues;

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (!_system.IsChannelEnabled(value.Channel) || !IsInCurrentPath(value.Channel))
                    continue;

                if (!groups.TryGetValue(value.OwnerName, out var ownerValues))
                {
                    ownerValues = new List<DebugVisualizationHudValueData>();
                    groups.Add(value.OwnerName, ownerValues);
                }

                ownerValues.Add(value);
            }

            return groups;
        }

        public DebugVisualizationStyleData GetStyle(string channel)
        {
            return _system.GetChannelStyle(channel);
        }

        public void ClearDraws()
        {
            _system.ClearDraws();
        }

        private bool IsInCurrentPath(string channel)
        {
            return string.IsNullOrEmpty(_currentChannelPath)
                   || string.Equals(channel, _currentChannelPath, StringComparison.OrdinalIgnoreCase)
                   || channel.StartsWith(_currentChannelPath + "/", StringComparison.OrdinalIgnoreCase);
        }

        internal sealed class ChannelTreeNode
        {
            public readonly string Name;
            public readonly string FullPath;
            public readonly List<ChannelTreeNode> Children = new();
            public string Channel;
            public bool IsEnabled;

            public bool HasChildren => Children.Count > 0;

            private readonly Dictionary<string, ChannelTreeNode> _childrenByName = new(StringComparer.Ordinal);

            private ChannelTreeNode(string name, string fullPath)
            {
                Name = name;
                FullPath = fullPath;
            }

            public static ChannelTreeNode Build(
                IReadOnlyList<string> channels,
                Func<string, bool> isChannelEnabled)
            {
                var root = new ChannelTreeNode(string.Empty, string.Empty);

                for (var i = 0; i < channels.Count; i++)
                    root.Add(channels[i], isChannelEnabled);

                Sort(root);
                return root;
            }

            private static void Sort(ChannelTreeNode node)
            {
                node.Children.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
                for (var i = 0; i < node.Children.Count; i++)
                    Sort(node.Children[i]);
            }

            private void Add(string channel, Func<string, bool> isChannelEnabled)
            {
                var parts = channel.Split('/');
                var current = this;
                var currentPath = string.Empty;

                for (var i = 0; i < parts.Length; i++)
                {
                    var part = string.IsNullOrWhiteSpace(parts[i]) ? DebugVisualizationUtility.DefaultChannel : parts[i];
                    currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                    if (!current._childrenByName.TryGetValue(part, out var child))
                    {
                        child = new ChannelTreeNode(part, currentPath);
                        current._childrenByName.Add(part, child);
                        current.Children.Add(child);
                    }

                    current = child;
                }

                current.Channel = channel;
                current.IsEnabled = isChannelEnabled(channel);
            }

            public ChannelTreeNode Find(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return this;

                var parts = path.Split('/');
                var current = this;

                for (var i = 0; i < parts.Length; i++)
                {
                    if (!current._childrenByName.TryGetValue(parts[i], out current))
                        return null;
                }

                return current;
            }
        }
    }

}
