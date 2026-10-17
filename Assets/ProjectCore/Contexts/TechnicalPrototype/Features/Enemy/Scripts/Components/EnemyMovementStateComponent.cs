using Fusion;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyMovementStateComponent :
        BaseNetworkEntityBehaviourComponent,
        IMovementStateDataMutator<EnemyMovementState>
    {
        [Networked] public EnemyMovementState CurrentState { get; private set; }
        [Networked] public EnemyMovementState PreviousState { get; private set; }

        public EnemyMovementState CurrentMovementStates => CurrentState;

        public EnemyMovementState PreviousMovementStates => PreviousState;

        public void ChangeMovementState(EnemyMovementState newState)
        {
            ChangeState(newState);
        }

        public void ChangeState(EnemyMovementState newState)
        {
            PreviousState = CurrentState;
            CurrentState = newState;
        }

    }
}
