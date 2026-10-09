namespace Shared
{
    public interface IMediator
    {
        public void Notify(HandlerPayload payload);
    }
}
