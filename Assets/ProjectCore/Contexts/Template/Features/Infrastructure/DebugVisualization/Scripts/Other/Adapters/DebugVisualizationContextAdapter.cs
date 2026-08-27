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

        public void Label(
            Vector3 position,
            string text,
            string channel = DebugVisualizationUtility.DefaultChannel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Label(
                position,
                text,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f));
        }

        public void Radius(
            Vector3 center,
            float radius,
            string channel = DebugVisualizationUtility.DefaultChannel)
        {
            AddCircle(center, radius, Vector3.up, channel);
        }

        public void Radius2D(
            Vector3 center,
            float radius,
            string channel = DebugVisualizationUtility.DefaultChannel)
        {
            AddCircle(center, radius, Vector3.forward, channel);
        }

        public void ZoneDisc(
            Vector3 center,
            float radius,
            string channel = DebugVisualizationUtility.DefaultChannel)
        {
            AddDisc(center, radius, Vector3.up, channel);
        }

        public void ZoneDisc2D(
            Vector3 center,
            float radius,
            string channel = DebugVisualizationUtility.DefaultChannel)
        {
            AddDisc(center, radius, Vector3.forward, channel);
        }
        
        public void Line(
            Vector3 start,
            Vector3 end,
            string channel = DebugVisualizationUtility.DefaultChannel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Line(
                start,
                end,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f));
        }

        private void AddCircle(Vector3 center, float radius, Vector3 normal, string channel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Circle(
                center,
                radius,
                normal,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f,
                64));
        }

        private void AddDisc(Vector3 center, float radius, Vector3 normal, string channel)
        {
            string normalizedChannel = NormalizeChannel(channel);
            _backend.Add(DebugVisualizationDrawCommand.Disc(
                center,
                radius,
                normal,
                normalizedChannel,
                _backend.GetChannelStyle(normalizedChannel),
                0f,
                64));
        }

        private static string NormalizeChannel(string channel)
        {
            return string.IsNullOrWhiteSpace(channel)
                ? DebugVisualizationUtility.DefaultChannel
                : channel;
        }
    }
}
