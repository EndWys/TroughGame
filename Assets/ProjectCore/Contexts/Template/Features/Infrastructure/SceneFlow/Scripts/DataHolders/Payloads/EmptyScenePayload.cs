namespace ProjectCore.Template
{
    public sealed class EmptyScenePayload : IScenePayload
    {
        public static readonly EmptyScenePayload Instance = new EmptyScenePayload();

        private EmptyScenePayload()
        {
        }
    }
}
