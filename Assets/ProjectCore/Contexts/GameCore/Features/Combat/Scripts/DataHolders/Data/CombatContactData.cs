namespace ProjectCore.GameCore
{
    public readonly struct CombatContactData
    {
        public CombatContactData(
            NetworkEntityIdData sourceEntityId,
            NetworkEntityIdData targetEntityId,
            uint executionId,
            int tick)
        {
            SourceEntityId = sourceEntityId;
            TargetEntityId = targetEntityId;
            ExecutionId = executionId;
            Tick = tick;
        }

        public NetworkEntityIdData SourceEntityId { get; }
        public NetworkEntityIdData TargetEntityId { get; }
        public uint ExecutionId { get; }
        public int Tick { get; }
    }
}
