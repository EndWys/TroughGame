using Domain;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public abstract class BasePlayerMovementState : MonoBehaviour, IState<MovementStates, PlayerInputData>
    {
        private IReadOnlyList<IPlayerMovementStateProcessor> _processors;

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

        public void Init(PlayerMovement context)
        {
            Context = context;
            _processors = CreateProcessors();
        }

        public abstract void Enter();

        public virtual MovementStates Tick(PlayerInputData input)
        {
            foreach (IPlayerMovementStateProcessor processor in _processors)
            {
                if (processor.Execute(input, out MovementStates resultState))
                {
                    return resultState;
                }
            }

            return FallbackState;
        }

        public abstract void Exit();

        protected abstract IReadOnlyList<IPlayerMovementStateProcessor> CreateProcessors();

        protected abstract MovementStates FallbackState { get; }
        
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
