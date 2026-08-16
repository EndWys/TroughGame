using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ProjectCore.Template
{
    internal sealed class DebugVisualizationSystem : IDebugVisualizationBackend, IDisposable
    {
        private static readonly IReadOnlyList<string> EmptyChannels =
            Array.AsReadOnly(Array.Empty<string>());
        private static readonly IReadOnlyList<DebugVisualizationHudValueData> EmptyValues =
            Array.AsReadOnly(Array.Empty<DebugVisualizationHudValueData>());

        private readonly IDebugVisualizationRegistry _registry;

        private DebugVisualizationRunnerComponent _runner;
        private DebugVisualizationSettingsConfig _settings;
        private bool _isDisposed;

        public DebugVisualizationSystem(IDebugVisualizationRegistry registry)
        {
            _registry = registry;
        }

        public bool IsInitialized => _runner != null;

        public bool HudVisible
        {
            get => _runner != null && _runner.HudVisible;
            set
            {
                if (_runner != null)
                    _runner.HudVisible = value;
            }
        }

        public IReadOnlyList<string> ChannelNames =>
            _runner != null ? _runner.ChannelNames : EmptyChannels;

        public IReadOnlyList<DebugVisualizationHudValueData> PersistentValues =>
            _runner != null ? _runner.PersistentValues : EmptyValues;

        public void Initialize(
            DebugVisualizationSettingsConfig settings,
            PanelSettings panelSettings,
            StyleSheet hudStyleSheet)
        {
            ThrowIfDisposed();
            if (_runner != null)
                return;
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (panelSettings == null)
                throw new ArgumentNullException(nameof(panelSettings));
            if (hudStyleSheet == null)
                throw new ArgumentNullException(nameof(hudStyleSheet));

            _settings = settings;

            var host = new GameObject("[Debug Visualization]");
            Object.DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;

            _runner = host.AddComponent<DebugVisualizationRunnerComponent>();
            _runner.Initialize(this, _registry, settings, panelSettings, hudStyleSheet);
        }

        public void Add(DebugVisualizationDrawCommand command)
        {
            _runner?.Add(command);
        }

        public void AddPersistentValue(Object owner, string channel, string label, object value)
        {
            _runner?.AddPersistentValue(owner, channel, label, value);
        }

        public void AddPersistentWorldLabel(
            Object owner,
            string channel,
            string label,
            object value,
            Vector3 position)
        {
            _runner?.AddPersistentWorldLabel(owner, channel, label, value, position);
        }

        public bool IsChannelEnabled(string channel)
        {
            return _runner == null || _runner.IsChannelEnabled(channel);
        }

        public void SetChannelEnabled(string channel, bool enabled)
        {
            _runner?.SetChannelEnabled(channel, enabled);
        }

        public DebugVisualizationStyleData GetChannelStyle(string channel)
        {
            return _settings != null
                ? _settings.GetStyle(channel)
                : DebugVisualizationStyleData.Default;
        }

        public void Register(Object target)
        {
            if (_runner != null)
                _registry.Register(target);
        }

        public void Unregister(Object target)
        {
            _registry.Unregister(target);
        }

        public void ClearAll()
        {
            _registry.Clear();
            _runner?.ClearAll();
        }

        public void ClearDraws()
        {
            _runner?.ClearDraws();
        }

        public void Shutdown()
        {
            _registry.Clear();

            DebugVisualizationRunnerComponent runner = _runner;
            _runner = null;
            _settings = null;

            if (runner == null)
                return;

            GameObject host = runner.gameObject;
            runner.ClearAll();
            if (host == null)
                return;

            host.SetActive(false);

            if (Application.isPlaying)
                Object.Destroy(host);
            else
                Object.DestroyImmediate(host);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            DebugVisualizationUtility.Detach(this);
            Shutdown();
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(DebugVisualizationSystem));
        }
    }
}
