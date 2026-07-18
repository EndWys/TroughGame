using UnityEngine;

namespace GameCore.Movement
{
    public interface IGroundDetectorDataAccessor
    {
        public bool IsGrounded { get; }
        public Vector3 GroundNormal { get; }
    }
}