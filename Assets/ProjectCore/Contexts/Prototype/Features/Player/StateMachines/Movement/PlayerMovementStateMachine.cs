using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using GameCore.Movement;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerMovementStateMachine : 
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
        private IMovementStateDataChanger _movementStateDataChanger;
        private IGroundDetectorDataChanger _groundDetectorDataChanger;
        private IJumpDataChanger _jumpDataChanger;

        [Inject]
        private void Construct(IMovementStateDataChanger movementStateDataChanger,
            IJumpDataChanger jumpDataChanger,
            IGroundDetectorDataChanger groundDetectorDataChanger,
            IPlayerMediator mediator)
        {
            _movementStateDataChanger = movementStateDataChanger;
            _jumpDataChanger = jumpDataChanger;
            _groundDetectorDataChanger = groundDetectorDataChanger;
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

            if (_groundDetectorDataChanger.IsGrounded)
            {
                _jumpDataChanger.RestartCoyoteTimer(AirborneConfig.CoyoteTimeTicks);
                _jumpDataChanger.ChangeJumpingStatus(false);
            }

            if (payload.IsJumpPressed)
            {
                _jumpDataChanger.RestartJumpBufferTimer(AirborneConfig.JumpBufferTicks);
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
            
            _jumpDataChanger.ChangeJumpingStatus(true);
            _jumpDataChanger.StopCoyoteTimer();
            _jumpDataChanger.StopJumpBufferTimer();
            
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
                    MovementStates = _movementStateDataChanger.CurrentMovementStates,
                    PreviousMovementStates = _movementStateDataChanger.PreviousMovementStates,
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
