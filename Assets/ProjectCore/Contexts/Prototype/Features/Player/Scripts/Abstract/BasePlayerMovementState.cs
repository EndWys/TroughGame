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
            PlayerPoseComponent = playerPoseComponent;
            NetworkBehaviourAccessor = networkBehaviourAccessor;
            JumpDataAccessor = jumpDataAccessor;
            GroundDetectorDataAccessor = groundDetectorDataAccessor;
            ClimbDetectorDataMutator = climbDetectorDataMutator;
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
