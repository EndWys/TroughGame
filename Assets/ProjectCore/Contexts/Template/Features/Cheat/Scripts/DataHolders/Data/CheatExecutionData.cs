namespace ProjectCore.Template
{
    public sealed class CheatExecutionData
    {
        public CheatExecutionData(string commandName, object value)
        {
            CommandName = commandName;
            Value = value;
        }

        public string CommandName { get; }
        public object Value { get; }
    }
}
