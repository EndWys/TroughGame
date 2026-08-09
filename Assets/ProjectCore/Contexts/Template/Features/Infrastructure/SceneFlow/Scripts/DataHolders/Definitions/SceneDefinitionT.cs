using System;

namespace ProjectCore.Template
{
    public abstract class SceneDefinition<TScene, TSettings> : BaseSceneDefinition
        where TScene : IScene<TSettings>
        where TSettings : ISceneSettings
    {
        public override Type SceneType => typeof(TScene);
        public override Type SettingsType => typeof(TSettings);
    }
}
