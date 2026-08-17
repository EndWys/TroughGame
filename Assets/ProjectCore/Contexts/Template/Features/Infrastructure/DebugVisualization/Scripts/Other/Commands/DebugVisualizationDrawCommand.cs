using UnityEngine;

namespace ProjectCore.Template
{
    internal readonly struct DebugVisualizationDrawCommand
    {
        public readonly DebugVisualizationDrawKind Kind;
        public readonly string Channel;
        public readonly DebugVisualizationRenderStyleData Style;
        public readonly Vector3 A;
        public readonly Vector3 B;
        public readonly float Radius;
        public readonly int Segments;
        public readonly string Text;
        public readonly float ExpiresAt;

        private DebugVisualizationDrawCommand(DebugVisualizationDrawKind kind, Vector3 a, Vector3 b, float radius, string text, string channel, DebugVisualizationStyleData style, float? duration, int segments)
        {
            Kind = kind;
            A = a;
            B = b;
            Radius = radius;
            Text = text;
            Channel = channel;
            Style = new DebugVisualizationRenderStyleData(style);
            Segments = segments;
            ExpiresAt = duration.HasValue
                ? Time.unscaledTime + Mathf.Max(0f, duration.Value)
                : float.PositiveInfinity;
        }

        public bool IsExpired => Time.unscaledTime > ExpiresAt;

        public static DebugVisualizationDrawCommand Label(Vector3 position, string text, string channel, DebugVisualizationStyleData style, float? duration)
        {
            return new DebugVisualizationDrawCommand(DebugVisualizationDrawKind.Label, position, default, 0f, text, channel, style, duration, 0);
        }

        public static DebugVisualizationDrawCommand Line(Vector3 start, Vector3 end, string channel, DebugVisualizationStyleData style, float? duration)
        {
            return new DebugVisualizationDrawCommand(DebugVisualizationDrawKind.Line, start, end, 0f, null, channel, style, duration, 0);
        }

        public static DebugVisualizationDrawCommand Circle(Vector3 center, float radius, string channel, DebugVisualizationStyleData style, float? duration, int segments)
        {
            return new DebugVisualizationDrawCommand(DebugVisualizationDrawKind.Circle, center, default, Mathf.Max(0f, radius), null, channel, style, duration, segments);
        }

        public static DebugVisualizationDrawCommand Disc(Vector3 center, float radius, string channel, DebugVisualizationStyleData style, float? duration, int segments)
        {
            return new DebugVisualizationDrawCommand(DebugVisualizationDrawKind.Disc, center, default, Mathf.Max(0f, radius), null, channel, style, duration, segments);
        }
    }
}
