using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_Popup_Catalog",
        menuName = "SO/Template/Popup/Catalog")]
    public sealed class PopupCatalogConfig : ScriptableObject
    {
        [SerializeField] private List<BasePopupDefinition> _definitions =
            new List<BasePopupDefinition>();

        public IReadOnlyList<BasePopupDefinition> Definitions => _definitions;
    }
}
