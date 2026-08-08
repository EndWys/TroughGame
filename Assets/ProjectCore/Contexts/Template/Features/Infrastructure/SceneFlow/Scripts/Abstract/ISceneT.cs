namespace ProjectCore.Template
{
    public interface IScene<TPayload> : IScene where TPayload : IScenePayload
    {
    }
}
