using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_LoadingScreen_Catalog",
        menuName = "SO/Template/LoadingScreen/Catalog")]
    public sealed class LoadingScreenCatalogConfig : ScriptableObject
    {
        [SerializeField] private BaseLoadingScreenDefinition _defaultDefinition;
        [SerializeField] private List<BaseLoadingScreenDefinition> _definitions =
            new List<BaseLoadingScreenDefinition>();

        public BaseLoadingScreenDefinition DefaultDefinition => _defaultDefinition;
        public IReadOnlyList<BaseLoadingScreenDefinition> Definitions => _definitions;
    }
}
