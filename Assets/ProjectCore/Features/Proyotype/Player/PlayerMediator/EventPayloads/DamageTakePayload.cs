namespace ProjectCore.Features.Prototype.Player.PlayerMediator.EventPayloads
{
    public struct DamageTakePayload
    {
        public int Amount { get; set; }
        public string DamageType { get; set; }
    }
}