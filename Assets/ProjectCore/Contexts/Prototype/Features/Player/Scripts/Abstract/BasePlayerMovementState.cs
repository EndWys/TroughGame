using System;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.Prototype
{
    public abstract class BasePlayerMovementState :
        BaseMovementState<MovementStates, PlayerInputData>
    {
        public PlayerMovementStateMachine Context { get; private set; }

        public PlayerPoseComponent PlayerPoseComponent { get; private set; }
        
        public INetworkBehaviourAccessor NetworkBehaviourAccessor { get; private set; }
        public IJumpDataAccessor JumpDataAccessor { get; private set; }
        public IGroundDetectorDataAccessor GroundDetectorDataAccessor { get; private set; }
        public IClimbDetectorDataMutator ClimbDetectorDataMutator { get; private set; }
        
        [Inject]
        private void Construct(PlayerPoseComponent playerPoseComponent,
            INetworkBehaviourAccessor networkBehaviourAccessor,
            IJumpDataAccessor jumpDataAccessor,
            IGroundDetectorDataAccessor groundDetectorDataAccessor,
            IClimbDetectorDataMutator climbDetectorDataMutator)
        {
            PlayerPoseComponent = playerPoseComponent ?? throw new ArgumentNullException(nameof(playerPoseComponent));
            NetworkBehaviourAccessor = networkBehaviourAccessor ?? throw new ArgumentNullException(nameof(networkBehaviourAccessor));
            JumpDataAccessor = jumpDataAccessor ?? throw new ArgumentNullException(nameof(jumpDataAccessor));
            GroundDetectorDataAccessor = groundDetectorDataAccessor ?? throw new ArgumentNullException(nameof(groundDetectorDataAccessor));
            ClimbDetectorDataMutator = climbDetectorDataMutator ?? throw new ArgumentNullException(nameof(climbDetectorDataMutator));
        }

        public virtual void Init(PlayerMovementStateMachine context)
        {
            Context = context;
            Init();
        }
        
        protected virtual void ApplyVelocityChange(Vector3 targetVelocity)
        {
            Vector3 currentVelocity = Context.Rigidbody.linearVelocity;
            
            Vector3 velocityChange = targetVelocity - currentVelocity;
            
            velocityChange = ApplyVelocityChangeModifier(velocityChange);
            
            Context.Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        
        protected void SetClimbingPhysics(bool isClimbing)
        {
            Context.Rigidbody.useGravity = !isClimbing;
            if (isClimbing)
            {
                Context.Rigidbody.linearVelocity = Vector3.zero; 
            }
        }

        public virtual Vector3 ApplyVelocityChangeModifier(Vector3 velocityChange)
        {
            return velocityChange;
        }
    }
}
