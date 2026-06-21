using Domain;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public abstract class BasePlayerMovementState :
        BaseMovementState<PlayerInputData>
    {
        public PlayerMovement Context { get; private set; }

        public PoseController PoseController { get; private set; }
        
        public INetworkBehaviourAccessor NetworkBehaviourAccessor { get; private set; }
        public IJumpDataAccessor JumpDataAccessor { get; private set; }
        public IGroundDetectorDataAccessor GroundDetectorDataAccessor { get; private set; }
        public IClimbDetectorDataChanger ClimbDetectorDataChanger { get; private set; }
        
        [Inject]
        private void Construct(PoseController poseController,
            INetworkBehaviourAccessor networkBehaviourAccessor,
            IJumpDataAccessor jumpDataAccessor,
            IGroundDetectorDataAccessor groundDetectorDataAccessor,
            IClimbDetectorDataChanger climbDetectorDataChanger)
        {
            PoseController = poseController;
            NetworkBehaviourAccessor = networkBehaviourAccessor;
            JumpDataAccessor = jumpDataAccessor;
            GroundDetectorDataAccessor = groundDetectorDataAccessor;
            ClimbDetectorDataChanger = climbDetectorDataChanger;
        }

        public virtual void Init(PlayerMovement context)
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
