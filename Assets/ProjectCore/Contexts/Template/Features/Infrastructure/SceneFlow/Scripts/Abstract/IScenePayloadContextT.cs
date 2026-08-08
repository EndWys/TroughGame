namespace ProjectCore.Template
{
    public interface IScenePayloadContext<out TPayload> where TPayload : IScenePayload
    {
        TPayload Payload { get; }
    }
}
