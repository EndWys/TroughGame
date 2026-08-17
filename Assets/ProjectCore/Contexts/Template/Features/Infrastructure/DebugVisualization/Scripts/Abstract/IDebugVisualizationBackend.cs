using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    internal interface IDebugVisualizationBackend : IDebugVisualizationSystem
    {
        public void Initialize(
            DebugVisualizationSettingsConfig settings,
            PanelSettings panelSettings,
            StyleSheet hudStyleSheet);
        public void Add(DebugVisualizationDrawCommand command);
        public void AddPersistentValue(Object owner, string channel, string label, object value);
        public void AddPersistentWorldLabel(
            Object owner,
            string channel,
            string label,
            object value,
            Vector3 position);
        public void Register(Object target);
        public void Unregister(Object target);
        public void ClearAll();
        public void Shutdown();
    }
}
