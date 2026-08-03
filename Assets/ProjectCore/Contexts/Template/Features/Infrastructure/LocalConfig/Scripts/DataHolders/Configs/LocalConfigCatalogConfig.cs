using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_LocalConfig_Catalog",
        menuName = "SO/Template/LocalConfig/Catalog")]
    public sealed class LocalConfigCatalogConfig : ScriptableObject
    {
        [SerializeField] private List<BaseLocalConfig> _configs = new List<BaseLocalConfig>();

        public IReadOnlyList<BaseLocalConfig> Configs => _configs;
    }
}
