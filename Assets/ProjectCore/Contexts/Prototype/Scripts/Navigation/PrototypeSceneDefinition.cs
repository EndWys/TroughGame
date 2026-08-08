using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.Prototype
{
    [CreateAssetMenu(
        fileName = "Config_Prototype_SceneFlow_Prototype",
        menuName = "SO/Prototype/SceneFlow/Prototype")]
    public sealed class PrototypeSceneDefinition : SceneDefinition<PrototypeScene, EmptyScenePayload>
    {
    }
}
