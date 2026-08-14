using Domain;

namespace ProjectCore.Template
{
    internal sealed class PopupCompletionData
    {
        public PopupCompletionData(object response, Error error)
        {
            Response = response;
            Error = error;
        }

        public object Response { get; }
        public Error Error { get; }
        public bool IsSuccess => Error.IsNone;
    }
}
