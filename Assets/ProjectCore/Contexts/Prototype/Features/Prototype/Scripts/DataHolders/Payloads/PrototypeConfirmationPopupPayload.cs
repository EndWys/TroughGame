using ProjectCore.Template;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeConfirmationPopupPayload : IPopupPayload
    {
        public PrototypeConfirmationPopupPayload(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
