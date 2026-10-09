using System;

namespace ProjectCore.Template
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DebugRadiusAttribute : Attribute
    {
        public DebugRadiusAttribute(string channel = DebugVisualizationUtility.DefaultChannel)
        {
            Channel = channel;
        }

        public string Channel { get; }
        public string Label { get; set; }
        public bool Filled { get; set; }
        public DebugVisualizationValueDisplay ValueDisplay { get; set; } = DebugVisualizationValueDisplay.Hud;
        public float WorldYOffset { get; set; } = 2f;
    }
}
