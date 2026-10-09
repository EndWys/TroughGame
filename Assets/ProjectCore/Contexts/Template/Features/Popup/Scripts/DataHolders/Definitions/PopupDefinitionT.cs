using System;

namespace ProjectCore.Template
{
    public abstract class PopupDefinition<TPopup, TPayload, TResponse> : BasePopupDefinition
        where TPopup : BasePopupView<TPayload, TResponse>
        where TPayload : IPopupPayload
    {
        public override Type PopupType => typeof(TPopup);
        public override Type PayloadType => typeof(TPayload);
        public override Type ResponseType => typeof(TResponse);
    }
}
