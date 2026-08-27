using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerAnimatorComponent : MonoBehaviour
    {
        private static readonly int MovementStateParameter = Animator.StringToHash("MovementState");
        private static readonly int DirectionXParameter = Animator.StringToHash("DirectionX");
        private static readonly int DirectionYParameter = Animator.StringToHash("DirectionY");

        [SerializeField] private Animator _animator;

        public void PlayIdle()
        {
            _animator.SetInteger(MovementStateParameter, (int)PlayerMovementState.Idle);
        }

        public void PlayLocomotion()
        {
            _animator.SetInteger(MovementStateParameter, (int)PlayerMovementState.Locomotion);
        }

        public void PlayDodge()
        {
            _animator.SetInteger(MovementStateParameter, (int)PlayerMovementState.Dodge);
        }

        public void SetFacingDirection(PlayerFacingDirection facingDirection)
        {
            Vector2 direction = PlayerFacingDirectionUtility.ToVector(facingDirection);
            _animator.SetFloat(DirectionXParameter, direction.x);
            _animator.SetFloat(DirectionYParameter, direction.y);
        }
    }
}
