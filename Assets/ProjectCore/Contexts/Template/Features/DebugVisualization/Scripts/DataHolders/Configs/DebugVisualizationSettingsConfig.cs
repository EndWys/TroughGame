using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_DebugVisualization_Settings",
        menuName = "SO/Template/DebugVisualization/Settings")]
    public sealed class DebugVisualizationSettingsConfig : ScriptableObject
    {
        [SerializeField] private bool _showHudOnStart = true;
        [SerializeField, Min(32)] private int _maxTransientDraws = 1024;
        [SerializeField]
        private DebugVisualizationStyleData _defaultStyle = DebugVisualizationStyleData.Default;
        [SerializeField] private List<DebugVisualizationChannelStyleConfig> _channelStyles = new();

        public bool ShowHudOnStart => _showHudOnStart;
        public int MaxTransientDraws => _maxTransientDraws;
        public DebugVisualizationStyleData DefaultStyle => _defaultStyle;
        public IReadOnlyList<DebugVisualizationChannelStyleConfig> ChannelStyles => _channelStyles;

        public DebugVisualizationStyleData GetStyle(string channel)
        {
            channel = string.IsNullOrWhiteSpace(channel) ? DebugVisualizationUtility.DefaultChannel : channel;
            DebugVisualizationChannelStyleConfig bestMatch = null;

            for (var i = 0; i < _channelStyles.Count; i++)
            {
                if (_channelStyles[i].Matches(channel))
                {
                    if (bestMatch == null || _channelStyles[i].Channel.Length > bestMatch.Channel.Length)
                        bestMatch = _channelStyles[i];
                }
            }

            if (bestMatch != null)
                return bestMatch.Style;

            var createdStyle =
                new DebugVisualizationChannelStyleConfig(channel, DebugVisualizationStyleData.Default);
            _channelStyles.Add(createdStyle);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            return createdStyle.Style;
        }
    }

}
