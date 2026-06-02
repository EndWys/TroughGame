using Domain;

namespace Prototype.Prototype
{
    public class DamageTakePayload : HandlerPayload
    {
        public byte Amount { get; set; }
        public string DamageType { get; set; }
    }
}
