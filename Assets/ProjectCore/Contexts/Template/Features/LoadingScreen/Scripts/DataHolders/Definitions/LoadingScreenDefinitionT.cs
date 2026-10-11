using System;

namespace ProjectCore.Template
{
    public abstract class LoadingScreenDefinition<TScreen, TSettings>
        : BaseLoadingScreenDefinition
        where TScreen : BaseLoadingScreenView<TSettings>
        where TSettings : ILoadingScreenSettings
    {
        public override Type ScreenType => typeof(TScreen);
        public override Type SettingsType => typeof(TSettings);
    }
}
