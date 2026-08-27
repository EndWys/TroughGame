using System;
using UnityEngine;

namespace ProjectCore.Template
{
    [Serializable]
    public sealed class DebugVisualizationChannelStyleConfig
    {
        [SerializeField] private string _channel = "General";
        [SerializeField] private DebugVisualizationStyleData _style = DebugVisualizationStyleData.Default;

        public DebugVisualizationChannelStyleConfig()
        {
        }

        public DebugVisualizationChannelStyleConfig(string channel, DebugVisualizationStyleData style)
        {
            _channel = string.IsNullOrWhiteSpace(channel)
                ? DebugVisualizationUtility.DefaultChannel
                : channel;
            _style = style ?? DebugVisualizationStyleData.Default;
        }

        public string Channel => _channel;
        public DebugVisualizationStyleData Style => _style;

        public bool Matches(string channel)
        {
            if (string.IsNullOrWhiteSpace(_channel) || string.IsNullOrWhiteSpace(channel))
                return false;

            if (string.Equals(_channel, channel, StringComparison.OrdinalIgnoreCase))
                return true;

            return channel.Length > _channel.Length
                   && channel.StartsWith(_channel, StringComparison.OrdinalIgnoreCase)
                   && channel[_channel.Length] == '/';
        }
    }
}
