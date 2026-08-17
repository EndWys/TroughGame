namespace ProjectCore.Template
{
    public sealed class EmptyLoadingScreenSettings : ILoadingScreenSettings
    {
        public static readonly EmptyLoadingScreenSettings Instance =
            new EmptyLoadingScreenSettings();

        private EmptyLoadingScreenSettings()
        {
        }
    }
}
