using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_LoadingScreen_Default",
        menuName = "SO/Template/LoadingScreen/Default")]
    public sealed class DefaultLoadingScreenDefinition
        : LoadingScreenDefinition<DefaultLoadingScreenView, EmptyLoadingScreenSettings>
    {
    }
}
