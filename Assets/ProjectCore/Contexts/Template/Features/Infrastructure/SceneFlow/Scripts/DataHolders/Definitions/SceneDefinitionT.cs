using System;

namespace ProjectCore.Template
{
    public abstract class SceneDefinition<TScene, TPayload> : BaseSceneDefinition
        where TScene : IScene<TPayload>
        where TPayload : IScenePayload
    {
        public override Type SceneType => typeof(TScene);
        public override Type PayloadType => typeof(TPayload);
    }
}
