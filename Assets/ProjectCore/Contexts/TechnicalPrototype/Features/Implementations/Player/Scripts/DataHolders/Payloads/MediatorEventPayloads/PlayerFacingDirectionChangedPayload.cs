using Domain;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerFacingDirectionChangedPayload : HandlerPayload
    {
        public PlayerFacingDirectionChangedPayload(IColleague sender, PlayerFacingDirection facingDirection)
        {
            Sender = sender;
            FacingDirection = facingDirection;
        }

        public PlayerFacingDirection FacingDirection { get; }
    }
}
