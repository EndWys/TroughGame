using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    [CreateAssetMenu(
        fileName = "Config_TechnicalPrototype_SceneFlow_TechnicalPrototype",
        menuName = "SO/TechnicalPrototype/SceneFlow/TechnicalPrototype")]
    public sealed class TechnicalPrototypeSceneDefinition :
        SceneDefinition<TechnicalPrototypeScene, EmptySceneSettings>
    {
    }
}
