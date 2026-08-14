using ProjectCore.Template;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeConfirmationPopupPayload : IPopupPayload
    {
        public PrototypeConfirmationPopupPayload(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; }
        public string Message { get; }
    }
}
