namespace ProjectCore.Template
{
    internal sealed class LoadingScreenEntryData
    {
        public LoadingScreenEntryData(
            BaseLoadingScreenDefinition definition,
            BaseLoadingScreenView screenView)
        {
            Definition = definition;
            ScreenView = screenView;
            State = LoadingScreenStates.Opening;
        }

        public BaseLoadingScreenDefinition Definition { get; }
        public BaseLoadingScreenView ScreenView { get; }
        public LoadingScreenStates State { get; set; }
    }
}
