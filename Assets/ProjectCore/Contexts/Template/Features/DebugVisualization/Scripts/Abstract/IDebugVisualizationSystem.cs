using System.Collections.Generic;

namespace ProjectCore.Template
{
    public interface IDebugVisualizationSystem
    {
        public bool IsInitialized { get; }
        public bool IsHUDVisible { get; set; }
        public IReadOnlyList<string> ChannelNames { get; }
        public IReadOnlyList<DebugVisualizationHUDValueData> PersistentValues { get; }

        public bool IsChannelEnabled(string channel);
        public void SetChannelEnabled(string channel, bool enabled);
        public DebugVisualizationStyleData GetChannelStyle(string channel);
        public void ClearDraws();
    }
}
