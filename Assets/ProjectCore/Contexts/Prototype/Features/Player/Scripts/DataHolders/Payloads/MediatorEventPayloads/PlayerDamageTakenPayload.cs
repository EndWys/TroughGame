using Shared;

namespace ProjectCore.Prototype
{
    public sealed class PlayerDamageTakenPayload : HandlerPayload
    {
        public byte Amount { get; set; }
        public string DamageType { get; set; }
    }
}
