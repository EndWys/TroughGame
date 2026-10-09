using System;
using System.Diagnostics;
using UnityEngine;

namespace ProjectCore.Template
{
    public static class DebugVisualizationUtility
    {
        public const string DefaultChannel = "General";

        private static readonly IDebugVisualizationBackend NoOpSystem =
            new DebugVisualizationNoOpAdapter();

        private static IDebugVisualizationBackend _system = NoOpSystem;

        public static bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_VISUALS
                return _system.IsInitialized;
#else
                return false;
#endif
            }
        }

        public static bool HudVisible
        {
            get => _system.HudVisible;
            set => _system.HudVisible = value;
        }

        internal static void Attach(IDebugVisualizationBackend system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (ReferenceEquals(_system, system))
                return;
            if (!ReferenceEquals(_system, NoOpSystem))
            {
                throw new InvalidOperationException(
                    "A debug visualization system is already attached.");
            }

            _system = system;
        }

        internal static void Detach(IDebugVisualizationBackend system)
        {
            if (ReferenceEquals(_system, system))
                _system = NoOpSystem;
        }

        internal static void Reset()
        {
            _system = NoOpSystem;
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void DrawNumber(
            Vector3 position,
            float value,
            string channel = DefaultChannel,
            DebugVisualizationStyleConfig style = null,
            float? duration = null)
        {
            DrawLabel(position, value.ToString("0.##"), channel, style, duration);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void DrawLabel(
            Vector3 position,
            string text,
            string channel = DefaultChannel,
            DebugVisualizationStyleConfig style = null,
            float? duration = null)
        {
            if (!_system.IsInitialized)
                return;

            string normalizedChannel = NormalizeChannel(channel);
            _system.Add(DebugVisualizationDrawCommand.Label(
                position,
                text,
                normalizedChannel,
                ResolveStyle(normalizedChannel, style),
                duration));
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void DrawLine(
            Vector3 start,
            Vector3 end,
            string channel = DefaultChannel,
            DebugVisualizationStyleConfig style = null,
            float? duration = null)
        {
            if (!_system.IsInitialized)
                return;

            string normalizedChannel = NormalizeChannel(channel);
            _system.Add(DebugVisualizationDrawCommand.Line(
                start,
                end,
                normalizedChannel,
                ResolveStyle(normalizedChannel, style),
                duration));
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void DrawRay(
            Vector3 start,
            Vector3 direction,
            string channel = DefaultChannel,
            DebugVisualizationStyleConfig style = null,
            float? duration = null)
        {
            DrawLine(start, start + direction, channel, style, duration);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void DrawRadius(
            Vector3 center,
            float radius,
            string channel = DefaultChannel,
            DebugVisualizationStyleConfig style = null,
            float? duration = null,
            int segments = 64)
        {
            if (!_system.IsInitialized)
                return;

            string normalizedChannel = NormalizeChannel(channel);
            _system.Add(DebugVisualizationDrawCommand.Circle(
                center,
                radius,
                Vector3.up,
                normalizedChannel,
                ResolveStyle(normalizedChannel, style),
                duration,
                Mathf.Max(8, segments)));
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void DrawZoneDisc(
            Vector3 center,
            float radius,
            string channel = DefaultChannel,
            DebugVisualizationStyleConfig style = null,
            float? duration = null,
            int segments = 64)
        {
            if (!_system.IsInitialized)
                return;

            string normalizedChannel = NormalizeChannel(channel);
            _system.Add(DebugVisualizationDrawCommand.Disc(
                center,
                radius,
                Vector3.up,
                normalizedChannel,
                ResolveStyle(normalizedChannel, style),
                duration,
                Mathf.Max(8, segments)));
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void SetChannelEnabled(string channel, bool enabled)
        {
            if (!_system.IsInitialized)
                return;

            _system.SetChannelEnabled(NormalizeChannel(channel), enabled);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void Register(UnityEngine.Object target)
        {
            if (!_system.IsInitialized)
                return;

            _system.Register(target);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void Unregister(UnityEngine.Object target)
        {
            _system.Unregister(target);
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void ClearAll()
        {
            if (!_system.IsInitialized)
                return;

            _system.ClearAll();
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("DEBUG_VISUALS")]
        public static void ClearDraws()
        {
            if (!_system.IsInitialized)
                return;

            _system.ClearDraws();
        }

        private static string NormalizeChannel(string channel)
        {
            return string.IsNullOrWhiteSpace(channel) ? DefaultChannel : channel;
        }

        private static DebugVisualizationStyleData ResolveStyle(
            string channel,
            DebugVisualizationStyleConfig style)
        {
            return style != null ? style.Data : _system.GetChannelStyle(channel);
        }
    }
}
