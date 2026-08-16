using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal sealed class DebugVisualizationNoOpAdapter : IDebugVisualizationBackend
    {
        private static readonly IReadOnlyList<string> EmptyChannels =
            Array.AsReadOnly(Array.Empty<string>());
        private static readonly IReadOnlyList<DebugVisualizationHudValueData> EmptyValues =
            Array.AsReadOnly(Array.Empty<DebugVisualizationHudValueData>());

        public bool IsInitialized => false;

        public bool HudVisible
        {
            get => false;
            set { }
        }

        public IReadOnlyList<string> ChannelNames => EmptyChannels;
        public IReadOnlyList<DebugVisualizationHudValueData> PersistentValues => EmptyValues;

        public void Initialize(
            DebugVisualizationSettingsConfig settings,
            PanelSettings panelSettings,
            StyleSheet hudStyleSheet)
        {
        }

        public void Add(DebugVisualizationDrawCommand command)
        {
        }

        public void AddPersistentValue(UnityEngine.Object owner, string channel, string label, object value)
        {
        }

        public void AddPersistentWorldLabel(
            UnityEngine.Object owner,
            string channel,
            string label,
            object value,
            Vector3 position)
        {
        }

        public bool IsChannelEnabled(string channel)
        {
            return true;
        }

        public void SetChannelEnabled(string channel, bool enabled)
        {
        }

        public DebugVisualizationStyleData GetChannelStyle(string channel)
        {
            return DebugVisualizationStyleData.Default;
        }

        public void Register(UnityEngine.Object target)
        {
        }

        public void Unregister(UnityEngine.Object target)
        {
        }

        public void ClearAll()
        {
        }

        public void ClearDraws()
        {
        }

        public void Shutdown()
        {
        }
    }
}
