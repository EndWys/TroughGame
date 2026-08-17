namespace ProjectCore.Template
{
    public sealed class EmptySceneSettings : ISceneSettings
    {
        public static readonly EmptySceneSettings Instance = new EmptySceneSettings();

        private EmptySceneSettings()
        {
        }
    }
}
