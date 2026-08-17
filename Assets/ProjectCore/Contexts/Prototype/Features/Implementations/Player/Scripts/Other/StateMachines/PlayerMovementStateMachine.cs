using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.Prototype
{
    public sealed class PlayerMovementStateMachine :
        BaseMovementStateMachine<BasePlayerMovementState, PlayerInputData>, IPlayerColleague
    {
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        
        [field:Space]
        [field:SerializeField] public LocomotionMovementConfig LocomotionConfig { get; private set; }
        [field:SerializeField] public AirborneMovementConfig AirborneConfig { get; private set; }
        [field:SerializeField] public CrouchMovementConfig CrouchConfig { get; private set; }

        [Space]
        [SerializedDictionary("State Type", "State Object")]
        [SerializeField] private SerializedDictionary<MovementStates, BasePlayerMovementState> _movementStates;
        
        private IPlayerMediator _mediator;
        private IMovementStateDataMutator _movementStateDataMutator;
        private IGroundDetectorDataMutator _groundDetectorDataMutator;
        private IJumpDataMutator _jumpDataMutator;

        [Inject]
        private void Construct(IMovementStateDataMutator movementStateDataMutator,
            IJumpDataMutator jumpDataMutator,
            IGroundDetectorDataMutator groundDetectorDataMutator,
            IPlayerMediator mediator)
        {
            _movementStateDataMutator = movementStateDataMutator;
            _jumpDataMutator = jumpDataMutator;
            _groundDetectorDataMutator = groundDetectorDataMutator;
            _mediator = mediator;
        }

        protected override MovementStates InitialState => MovementStates.Idle;

        protected override Dictionary<MovementStates, BasePlayerMovementState> CreateMovementStatesDictionary()
        {
            return _movementStates;
        }

        protected override void InitMovementState(BasePlayerMovementState state)
        {
            state.Init(this);
        }

        protected override bool TryGetMovementPayload(out PlayerInputData payload)
        {
            return ParentNetworkBehaviour.GetInput(out payload);
        }

        protected override void BeforeMovementUpdate(PlayerInputData payload)
        {
            ProcessRotation(payload.LookYawDelta);

            if (_groundDetectorDataMutator.IsGrounded)
            {
                _jumpDataMutator.RestartCoyoteTimer(AirborneConfig.CoyoteTimeTicks);
                _jumpDataMutator.ChangeJumpingStatus(false);
            }

            if (payload.IsJumpPressed)
            {
                _jumpDataMutator.RestartJumpBufferTimer(AirborneConfig.JumpBufferTicks);
            }
        }
        
        public void ExecuteJump()
        {
            Vector3 velocity = Rigidbody.linearVelocity;
            velocity.y = 0f;
            Rigidbody.linearVelocity = velocity;
            
            ExecuteJump(Vector3.up);
        }
        
        public void ExecuteJump(Vector3 direction)
        {
            Rigidbody.AddForce(direction * AirborneConfig.JumpForce, ForceMode.Impulse);
            
            _jumpDataMutator.ChangeJumpingStatus(true);
            _jumpDataMutator.StopCoyoteTimer();
            _jumpDataMutator.StopJumpBufferTimer();
            
            _mediator.Notify(new PlayerJumpedPayload()
            {
                Sender = this,
            });
        }
        
        protected override void BeforePreviousStateExit()
        {
            _mediator.Notify(
                new PlayerMovementStateChangedPayload()
                {
                    Sender = this,
                    MovementStates = _movementStateDataMutator.CurrentMovementStates,
                    PreviousMovementStates = _movementStateDataMutator.PreviousMovementStates,
                });
        }
        
        private void ProcessRotation(float yawDelta)
        {
            if (Mathf.Abs(yawDelta) > 0.01f)
            {
                float rotationStep = yawDelta 
                                     * LocomotionConfig.RotationSpeed 
                                     * ParentNetworkBehaviour.Runner.DeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(0, rotationStep, 0);
                Rigidbody.rotation *= deltaRotation;
            }
        }

    }
}
