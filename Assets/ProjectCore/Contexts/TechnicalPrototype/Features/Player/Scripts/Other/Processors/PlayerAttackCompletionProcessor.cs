using ProjectCore.GameCore;
using Shared;
namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerAttackCompletionProcessor : IStateProcessor<PlayerEntityState, PlayerEntityPayload>
    {
        private readonly ICombatActionSnapshotAccessor _action;
        public PlayerAttackCompletionProcessor(ICombatActionSnapshotAccessor action) { _action = action; }
        public bool Execute(PlayerEntityPayload payload, out PlayerEntityState resultState)
        {
            resultState = PlayerEntityState.Idle;
            CombatActionSnapshotData action = _action.ActionSnapshot;
            return action.ActionId == 0 || action.IsComplete;
        }
    }
}
