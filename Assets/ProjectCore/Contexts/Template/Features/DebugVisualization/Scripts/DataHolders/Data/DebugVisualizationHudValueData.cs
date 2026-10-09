namespace ProjectCore.Template
{
    public readonly struct DebugVisualizationHudValueData
    {
        public readonly string OwnerName;
        public readonly string Channel;
        public readonly string Label;
        public readonly object Value;

        public DebugVisualizationHudValueData(string ownerName, string channel, string label, object value)
        {
            OwnerName = ownerName;
            Channel = channel;
            Label = string.IsNullOrWhiteSpace(label) ? "Value" : label;
            Value = value;
        }
    }
}
