using ProjectCore.GameCore;
using Shared;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerDodgePhasePayload : HandlerPayload
    {
        public PlayerDodgePhasePayload(IColleague sender, DodgeMovementPhase phase, int? activeEndTick)
        {
            Sender = sender;
            Phase = phase;
            ActiveEndTick = activeEndTick;
        }

        public DodgeMovementPhase Phase { get; }
        public int? ActiveEndTick { get; }
    }
}
