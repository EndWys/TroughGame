using UnityEngine;

namespace ProjectCore.Template
{
    public readonly struct DebugVisualizationContextAdapter
    {
        private readonly Object _owner;
        private readonly IDebugVisualizationBackend _backend;

        internal DebugVisualizationContextAdapter(
            Object owner,
            IDebugVisualizationBackend backend)
        {
            _owner = owner;
            _backend = backend;
        }

        public void Value(string channel, string label, object value)
        {
            _backend.AddPersistentValue(_owner, channel, label, value);
        }

        public void Label(Vector3 position, string text, string channel = DebugVisualizationUtility.DefaultChannel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Label(
                position,
                text,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f));
        }

        public void Radius(Vector3 center, float radius, string channel = DebugVisualizationUtility.DefaultChannel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Circle(
                center,
                radius,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f,
                64));
        }

        public void ZoneDisc(Vector3 center, float radius, string channel = DebugVisualizationUtility.DefaultChannel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Disc(
                center,
                radius,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f,
                64));
        }
        
        public void Line(Vector3 start, Vector3 end, string channel = DebugVisualizationUtility.DefaultChannel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Line(
                start,
                end,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f));
        }

        private static string NormalizeChannel(string channel)
        {
            return string.IsNullOrWhiteSpace(channel)
                ? DebugVisualizationUtility.DefaultChannel
                : channel;
        }
    }
}
