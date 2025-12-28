namespace ProjectCore.Features.Proyotype.Player.PlayerMediator.EventPayloads
{
    public struct DamageTakePayload
    {
        public int Amount { get; set; }
        public string DamageType { get; set; }
    }
}