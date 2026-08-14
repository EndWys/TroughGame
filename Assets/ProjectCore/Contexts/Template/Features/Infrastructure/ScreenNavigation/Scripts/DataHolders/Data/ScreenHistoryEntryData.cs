namespace ProjectCore.Template
{
    public sealed class ScreenHistoryEntryData
    {
        public ScreenHistoryEntryData(
            BaseScreenDefinition definition,
            BaseScreenView screenView)
        {
            Definition = definition;
            ScreenView = screenView;
        }

        public BaseScreenDefinition Definition { get; }
        public BaseScreenView ScreenView { get; }
    }
}
