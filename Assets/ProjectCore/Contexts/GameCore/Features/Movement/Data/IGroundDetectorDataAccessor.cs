using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IGroundDetectorDataAccessor
    {
        public bool IsGrounded { get; }
        public Vector3 GroundNormal { get; }
    }
}
