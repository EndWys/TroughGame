using System.Collections.Generic;
using Domain;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal sealed class DebugVisualizationRunnerComponent : MonoBehaviour
    {
        private readonly List<DebugVisualizationHudValueData> _persistentValues = new();
        private readonly List<PersistentWorldLabel> _persistentWorldLabels = new();
        private readonly Dictionary<Object, int> _worldLabelStack = new();
        private readonly Dictionary<Object, WorldOwnerLabel> _worldOwnerLabels = new();
        private readonly Dictionary<string, bool> _channels = new();
        private readonly List<string> _channelNames = new();

        private IDebugVisualizationBackend _backend;
        private IDebugVisualizationRegistry _registry;
        private BoundedBuffer<DebugVisualizationDrawCommand> _draws;
        private DebugVisualizationSettingsConfig _settings;
        private Material _lineMaterial;
        private float _nextHudValuesRefreshTime;
        private bool _hudVisible;

        private DebugVisualizationHudAdapter _hudController;
        private DebugVisualizationHudWidgetView _hudView;

        public bool HudVisible
        {
            get => _hudVisible;
            set
            {
                _hudVisible = value;
                UpdateHudVisibility();
            }
        }

        public IReadOnlyCollection<string> Channels => _channelNames;
        internal IReadOnlyList<string> ChannelNames => _channelNames;
        internal IReadOnlyList<DebugVisualizationHudValueData> PersistentValues => _persistentValues;

        public void Initialize(
            IDebugVisualizationBackend backend,
            IDebugVisualizationRegistry registry,
            DebugVisualizationSettingsConfig settings,
            PanelSettings panelSettings,
            StyleSheet hudStyleSheet)
        {
            _backend = backend;
            _registry = registry;
            _settings = settings;
            _draws = new BoundedBuffer<DebugVisualizationDrawCommand>(
                Mathf.Max(1, settings.MaxTransientDraws));
            _hudController = new DebugVisualizationHudAdapter(backend);
            _hudView = new DebugVisualizationHudWidgetView(_hudController);
            _hudView.Attach(gameObject, panelSettings, hudStyleSheet);
            _hudController.Refresh();
            HudVisible = settings.ShowHudOnStart;
        }

        public void Add(DebugVisualizationDrawCommand command)
        {
            RegisterChannel(command.Channel);
            _draws.Add(command);
        }

        public void AddPersistentValue(Object owner, string channel, string label, object value)
        {
            channel = string.IsNullOrWhiteSpace(channel) ? DebugVisualizationUtility.DefaultChannel : channel;
            RegisterChannel(channel);
            _persistentValues.Add(new DebugVisualizationHudValueData(owner != null ? owner.name : "Unknown", channel, label, value));
        }

        public void AddPersistentWorldLabel(Object owner, string channel, string label, object value, Vector3 position)
        {
            channel = string.IsNullOrWhiteSpace(channel) ? DebugVisualizationUtility.DefaultChannel : channel;
            RegisterChannel(channel);
            _persistentWorldLabels.Add(new PersistentWorldLabel(owner, channel, label, value, position));
        }

        public bool IsChannelEnabled(string channel)
        {
            channel = string.IsNullOrWhiteSpace(channel) ? DebugVisualizationUtility.DefaultChannel : channel;
            return !_channels.TryGetValue(channel, out var enabled) || enabled;
        }

        public void SetChannelEnabled(string channel, bool enabled)
        {
            channel = string.IsNullOrWhiteSpace(channel) ? DebugVisualizationUtility.DefaultChannel : channel;
            RegisterChannel(channel);
            _channels[channel] = enabled;
        }

        public void ClearAll()
        {
            _draws.Clear();
            _persistentValues.Clear();
            _persistentWorldLabels.Clear();
            _worldLabelStack.Clear();
            _worldOwnerLabels.Clear();
            _channels.Clear();
            _channelNames.Clear();
            _hudController?.ResetPath();
            _hudController?.Refresh();
        }

        public void ClearDraws()
        {
            _draws.Clear();
            _hudController?.Refresh();
        }

        internal DebugVisualizationStyleData GetChannelStyle(string channel)
        {
            return _settings.GetStyle(channel);
        }

        private void OnDestroy()
        {
            _hudView?.Dispose();
            ClearAll();
            if (_lineMaterial != null)
                Destroy(_lineMaterial);
        }

        private void LateUpdate()
        {
            _persistentValues.Clear();
            _persistentWorldLabels.Clear();
            _registry.Refresh(_backend);
            _draws.RemoveAll(static draw => draw.IsExpired);

            if (_hudVisible && Time.unscaledTime >= _nextHudValuesRefreshTime)
            {
                _nextHudValuesRefreshTime = Time.unscaledTime + 0.1f;
                _hudController?.RefreshValuesOnly();
            }
        }

        private void OnRenderObject()
        {
            if (_draws.Count == 0)
                return;

            Camera camera = Camera.current;
            if (camera != null
                && (camera.cameraType == CameraType.Preview
                    || camera.cameraType == CameraType.Reflection))
            {
                return;
            }

            EnsureLineMaterial();
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            RenderGeometry(false);
            RenderGeometry(true);
            GL.PopMatrix();
        }

        private void OnGUI()
        {
            DrawWorldLabels();
        }

        private void UpdateHudVisibility()
        {
            _hudView?.SetVisible(_hudVisible);
        }

        private void DrawWorldLabels()
        {
            var camera = Camera.main;
            if (camera == null)
                return;

            DrawPersistentWorldLabels(camera);
            DrawTransientWorldLabels(camera);
        }

        private void DrawPersistentWorldLabels(Camera camera)
        {
            _worldLabelStack.Clear();
            _worldOwnerLabels.Clear();

            for (var i = 0; i < _persistentWorldLabels.Count; i++)
            {
                var label = _persistentWorldLabels[i];

                if (!IsChannelEnabled(label.Channel))
                    continue;

                var stackIndex = 0;
                if (label.Owner != null)
                {
                    _worldLabelStack.TryGetValue(label.Owner, out stackIndex);
                    _worldLabelStack[label.Owner] = stackIndex + 1;
                }

                if (DrawScreenLabel(camera, label.Position, $"{label.Label}: {FormatValue(label.Value)}", _settings.GetStyle(label.Channel), stackIndex, out var rect)
                    && label.Owner != null)
                {
                    _worldOwnerLabels[label.Owner] = WorldOwnerLabel.Combine(
                        _worldOwnerLabels.TryGetValue(label.Owner, out var existing) ? existing : default,
                        label.Owner.name,
                        label.Channel,
                        rect);
                }
            }

            foreach (var pair in _worldOwnerLabels)
            {
                var owner = pair.Key;
                var label = pair.Value;

                if (owner == null || !label.HasValue || !IsChannelEnabled(label.Channel))
                    continue;

                DrawScreenLabelAboveRect(label.OwnerName, label.TopRect, _settings.GetStyle(label.Channel));
            }
        }

        private void DrawTransientWorldLabels(Camera camera)
        {
            for (var i = 0; i < _draws.Count; i++)
            {
                var draw = _draws[i];

                if (draw.Kind != DebugVisualizationDrawKind.Label || !IsChannelEnabled(draw.Channel))
                    continue;

                DrawScreenLabel(camera, draw.A, draw.Text, draw.Style, 0, out _);
            }
        }

        private static bool DrawScreenLabel(
            Camera camera,
            Vector3 worldPosition,
            string text,
            DebugVisualizationRenderStyleData style,
            int stackIndex,
            out Rect rect)
        {
            return DrawScreenLabel(
                camera,
                worldPosition,
                text,
                style.Color,
                style.TextSize,
                stackIndex,
                out rect);
        }

        private static bool DrawScreenLabel(
            Camera camera,
            Vector3 worldPosition,
            string text,
            DebugVisualizationStyleData style,
            int stackIndex,
            out Rect rect)
        {
            return DrawScreenLabel(
                camera,
                worldPosition,
                text,
                style.VisibleColor,
                style.TextSize,
                stackIndex,
                out rect);
        }

        private static bool DrawScreenLabel(
            Camera camera,
            Vector3 worldPosition,
            string text,
            Color color,
            float textSize,
            int stackIndex,
            out Rect rect)
        {
            var screenPoint = camera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z <= 0f)
            {
                rect = default;
                return false;
            }

            var previousColor = GUI.color;
            var previousSize = GUI.skin.label.fontSize;
            GUI.color = color;
            GUI.skin.label.fontSize = Mathf.RoundToInt(textSize);

            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            var y = Screen.height - screenPoint.y - size.y * 0.5f - stackIndex * (size.y + 2f);
            rect = new Rect(screenPoint.x - size.x * 0.5f, y, size.x, size.y);
            GUI.Label(rect, text);

            GUI.skin.label.fontSize = previousSize;
            GUI.color = previousColor;
            return true;
        }

        private static void DrawScreenLabelAboveRect(string text, Rect topRect, DebugVisualizationStyleData style)
        {
            var previousColor = GUI.color;
            var previousSize = GUI.skin.label.fontSize;
            GUI.color = style.VisibleColor;
            GUI.skin.label.fontSize = Mathf.RoundToInt(style.TextSize);

            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            var rect = new Rect(topRect.center.x - size.x * 0.5f, topRect.yMin - size.y - 2f, size.x, size.y);
            GUI.Label(rect, text);

            GUI.skin.label.fontSize = previousSize;
            GUI.color = previousColor;
        }

        private void RenderGeometry(bool depthTest)
        {
            ApplyMaterialDepth(depthTest);
            _lineMaterial.SetPass(0);

            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < _draws.Count; i++)
            {
                DebugVisualizationDrawCommand draw = _draws[i];
                if (draw.Kind == DebugVisualizationDrawKind.Disc
                    && draw.Style.DepthTest == depthTest
                    && IsChannelEnabled(draw.Channel))
                {
                    EmitDiscFill(draw);
                }
            }
            GL.End();

            GL.Begin(GL.QUADS);
            for (int i = 0; i < _draws.Count; i++)
            {
                DebugVisualizationDrawCommand draw = _draws[i];
                if (draw.Style.DepthTest != depthTest
                    || !IsChannelEnabled(draw.Channel))
                {
                    continue;
                }

                switch (draw.Kind)
                {
                    case DebugVisualizationDrawKind.Line:
                        EmitThickLine(draw.A, draw.B, draw.Style);
                        break;
                    case DebugVisualizationDrawKind.Circle:
                    case DebugVisualizationDrawKind.Disc:
                        EmitCircle(draw);
                        break;
                }
            }
            GL.End();
        }

        private static void EmitThickLine(
            Vector3 start,
            Vector3 end,
            DebugVisualizationRenderStyleData style)
        {
            var direction = end - start;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            var camera = Camera.current != null ? Camera.current : Camera.main;
            var cameraForward = camera != null ? camera.transform.forward : Vector3.forward;
            var normal = Vector3.Cross(direction.normalized, cameraForward).normalized;

            if (normal.sqrMagnitude <= 0.0001f)
                normal = Vector3.Cross(direction.normalized, Vector3.up).normalized;

            var halfWidth = Mathf.Max(0.001f, style.LineWidth) * 0.5f;
            normal *= halfWidth;

            GL.Color(style.Color);
            GL.Vertex(start - normal);
            GL.Vertex(start + normal);
            GL.Vertex(end + normal);
            GL.Vertex(end - normal);
        }

        private static void EmitCircle(DebugVisualizationDrawCommand draw)
        {
            var previous = PointOnCircle(draw.A, draw.Radius, 0f);

            for (var i = 1; i <= draw.Segments; i++)
            {
                var angle = i / (float)draw.Segments * Mathf.PI * 2f;
                var next = PointOnCircle(draw.A, draw.Radius, angle);
                EmitThickLine(previous, next, draw.Style);
                previous = next;
            }
        }

        private static void EmitDiscFill(DebugVisualizationDrawCommand draw)
        {
            GL.Color(draw.Style.FillColor);

            for (var i = 0; i < draw.Segments; i++)
            {
                var currentAngle = i / (float)draw.Segments * Mathf.PI * 2f;
                var nextAngle = (i + 1) / (float)draw.Segments * Mathf.PI * 2f;

                GL.Vertex(draw.A);
                GL.Vertex(PointOnCircle(draw.A, draw.Radius, currentAngle));
                GL.Vertex(PointOnCircle(draw.A, draw.Radius, nextAngle));
            }
        }

        private static Vector3 PointOnCircle(Vector3 center, float radius, float angle)
        {
            return center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private void RegisterChannel(string channel)
        {
            if (_channels.ContainsKey(channel))
                return;

            _channels[channel] = true;
            _channelNames.Add(channel);
            _channelNames.Sort();
            _hudController?.Refresh();
        }

        private void EnsureLineMaterial()
        {
            if (_lineMaterial != null)
                return;

            var shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMaterial.SetInt("_ZWrite", 0);
        }

        private void ApplyMaterialDepth(bool depthTest)
        {
            EnsureLineMaterial();
            _lineMaterial.SetInt("_ZTest", depthTest
                ? (int)UnityEngine.Rendering.CompareFunction.LessEqual
                : (int)UnityEngine.Rendering.CompareFunction.Always);
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => "null",
                float number => number.ToString("0.###"),
                double number => number.ToString("0.###"),
                _ => value.ToString()
            };
        }

        private readonly struct PersistentWorldLabel
        {
            public readonly Object Owner;
            public readonly string Channel;
            public readonly string Label;
            public readonly object Value;
            public readonly Vector3 Position;

            public PersistentWorldLabel(Object owner, string channel, string label, object value, Vector3 position)
            {
                Owner = owner;
                Channel = channel;
                Label = string.IsNullOrWhiteSpace(label) ? (owner != null ? owner.name : "Value") : label;
                Value = value;
                Position = position;
            }
        }

        private readonly struct WorldOwnerLabel
        {
            public readonly string OwnerName;
            public readonly string Channel;
            public readonly Rect TopRect;
            public readonly bool HasValue;

            private WorldOwnerLabel(string ownerName, string channel, Rect topRect)
            {
                OwnerName = ownerName;
                Channel = channel;
                TopRect = topRect;
                HasValue = true;
            }

            public static WorldOwnerLabel Combine(WorldOwnerLabel current, string ownerName, string channel, Rect rect)
            {
                if (!current.HasValue || rect.yMin < current.TopRect.yMin)
                    return new WorldOwnerLabel(ownerName, channel, rect);

                return current;
            }
        }
    }
}
