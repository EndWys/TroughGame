using System;

namespace ProjectCore.Template
{
    public abstract class ScreenDefinition<TScreen, TSettings> : BaseScreenDefinition
        where TScreen : BaseScreenView<TSettings>
        where TSettings : IScreenSettings
    {
        public override Type ScreenType => typeof(TScreen);
        public override Type SettingsType => typeof(TSettings);
    }
}
