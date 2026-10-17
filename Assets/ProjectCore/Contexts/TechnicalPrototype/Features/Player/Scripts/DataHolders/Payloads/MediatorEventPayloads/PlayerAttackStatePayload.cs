using Shared;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerAttackStatePayload : HandlerPayload
    {
        public PlayerAttackStatePayload(IColleague sender, bool active, bool restarted, bool clearInput = false)
        {
            Sender = sender;
            Active = active;
            Restarted = restarted;
            ClearInput = clearInput;
        }

        public bool Active { get; }
        public bool Restarted { get; }
        public bool ClearInput { get; }
    }
}
