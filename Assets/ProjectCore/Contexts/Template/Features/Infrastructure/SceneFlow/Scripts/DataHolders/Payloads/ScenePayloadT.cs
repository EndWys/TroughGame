using System;

namespace ProjectCore.Template
{
    public sealed class ScenePayload<TPayload> : IScenePayloadContext<TPayload>
        where TPayload : IScenePayload
    {
        public ScenePayload(TPayload payload)
        {
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        public TPayload Payload { get; }
    }
}
