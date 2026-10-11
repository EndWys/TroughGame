namespace ProjectCore.Template
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ProjectCore.Template", sourceAssembly: "ProjectCore.Runtime", sourceClassName: "DebugVisualizationHudValueData")]
    public readonly struct DebugVisualizationHUDValueData
    {
        public readonly string OwnerName;
        public readonly string Channel;
        public readonly string Label;
        public readonly object Value;

        public DebugVisualizationHUDValueData(string ownerName, string channel, string label, object value)
        {
            OwnerName = ownerName;
            Channel = channel;
            Label = string.IsNullOrWhiteSpace(label) ? "Value" : label;
            Value = value;
        }
    }
}
