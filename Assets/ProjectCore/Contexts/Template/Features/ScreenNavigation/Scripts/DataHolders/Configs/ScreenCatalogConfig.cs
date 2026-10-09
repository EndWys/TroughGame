using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_ScreenNavigation_Catalog",
        menuName = "SO/Template/ScreenNavigation/Catalog")]
    public sealed class ScreenCatalogConfig : ScriptableObject
    {
        [SerializeField] private List<BaseScreenDefinition> _definitions =
            new List<BaseScreenDefinition>();

        public IReadOnlyList<BaseScreenDefinition> Definitions => _definitions;
    }
}
