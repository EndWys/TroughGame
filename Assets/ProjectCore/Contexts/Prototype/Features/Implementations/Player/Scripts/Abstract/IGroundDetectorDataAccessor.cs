using UnityEngine;

namespace ProjectCore.Prototype
{
    public interface IGroundDetectorDataAccessor
    {
        public bool IsGrounded { get; }
        public Vector3 GroundNormal { get; }
    }
}
