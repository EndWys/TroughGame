namespace Domain
{
    public interface IMediator
    {
        public void Notify(HandlerPayload payload);
    }
}
