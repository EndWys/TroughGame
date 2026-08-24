using System.Collections.Generic;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityComponent :
        BaseNetworkEntityRoot,
        IMovementStateDataMutator<PlayerMovementState>
    {
        [SerializeField]
        private PlayerMovementStateMachine _movementStateMachine;
        [SerializeField]
        private TransformMovementBodyComponent _movementBodyComponent;
        [SerializeField]
        private CameraTargetComponent _cameraTargetComponent;

        [Networked] public PlayerMovementState CurrentState { get; private set; }
        [Networked] public PlayerMovementState PreviousState { get; private set; }

        public PlayerMovementState CurrentMovementStates => CurrentState;
        public PlayerMovementState PreviousMovementStates => PreviousState;

        protected override void BeforeComponentsInitialized()
        {
            if (HasStateAuthority || HasInputAuthority)
            {
                Runner.SetIsSimulated(Object, true);
            }
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _movementStateMachine,
                _movementBodyComponent,
                _cameraTargetComponent,
            };
        }

        public void ChangeMovementState(PlayerMovementState newState)
        {
            ChangeState(newState);
        }

        public void ChangeState(PlayerMovementState newState)
        {
            PreviousState = CurrentState;
            CurrentState = newState;
        }
    }
}
