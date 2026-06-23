using Domain;

namespace Prototype.Prototype
{
    public class PlayerDamageTakenPayload : HandlerPayload
    {
        public byte Amount { get; set; }
        public string DamageType { get; set; }
    }
}
