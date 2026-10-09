using System;

namespace ProjectCore.Template
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DebugValueAttribute : Attribute
    {
        public DebugValueAttribute(string channel = DebugVisualizationUtility.DefaultChannel)
        {
            Channel = channel;
        }

        public string Channel { get; }
        public string Label { get; set; }
        public DebugVisualizationValueDisplay Display { get; set; } = DebugVisualizationValueDisplay.Hud;
        public float WorldYOffset { get; set; } = 2f;
    }
}
