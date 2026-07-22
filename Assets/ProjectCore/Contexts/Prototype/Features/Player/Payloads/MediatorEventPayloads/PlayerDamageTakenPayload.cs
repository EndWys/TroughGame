using Domain;

namespace ProjectCore.Prototype
{
    public class PlayerDamageTakenPayload : HandlerPayload
    {
        public byte Amount { get; set; }
        public string DamageType { get; set; }
    }
}
