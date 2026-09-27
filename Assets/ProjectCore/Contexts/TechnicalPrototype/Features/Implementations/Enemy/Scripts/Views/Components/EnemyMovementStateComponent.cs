using System;
using System.Collections.Generic;
using Fusion;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyMovementStateComponent :
        NetworkBehaviour,
        INetworkEntityComponent,
        IMovementStateDataMutator<EnemyMovementState>
    {
        private static readonly IReadOnlyList<INetworkEntityComponent> _emptyComponents =
            Array.Empty<INetworkEntityComponent>();

        [Networked] public EnemyMovementState CurrentState { get; private set; }
        [Networked] public EnemyMovementState PreviousState { get; private set; }

        public EnemyMovementState CurrentMovementStates => CurrentState;

        public EnemyMovementState PreviousMovementStates => PreviousState;

        public IReadOnlyList<INetworkEntityComponent> Components => _emptyComponents;

        public void ChangeMovementState(EnemyMovementState newState)
        {
            ChangeState(newState);
        }

        public void ChangeState(EnemyMovementState newState)
        {
            PreviousState = CurrentState;
            CurrentState = newState;
        }

        public void Init() { }

        public void NetworkTick() { }

        public void ClientRender() { }
    }
}
