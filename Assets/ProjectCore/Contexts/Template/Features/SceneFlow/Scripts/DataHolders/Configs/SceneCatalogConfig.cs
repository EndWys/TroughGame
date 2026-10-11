using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_SceneFlow_Catalog",
        menuName = "SO/Template/SceneFlow/Catalog")]
    public sealed class SceneCatalogConfig : ScriptableObject
    {
        [SerializeField] private List<BaseSceneDefinition> _definitions = new List<BaseSceneDefinition>();

        public IReadOnlyList<BaseSceneDefinition> Definitions => _definitions;
    }
}
