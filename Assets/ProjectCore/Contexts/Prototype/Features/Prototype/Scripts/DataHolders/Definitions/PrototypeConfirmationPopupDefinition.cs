using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.Prototype
{
    [CreateAssetMenu(
        fileName = "Config_Prototype_Prototype_ConfirmationPopup",
        menuName = "SO/Prototype/Prototype/ConfirmationPopup")]
    public sealed class PrototypeConfirmationPopupDefinition
        : PopupDefinition<
            PrototypeConfirmationPopupView,
            PrototypeConfirmationPopupPayload,
            PrototypeConfirmationResponses>
    {
    }
}
