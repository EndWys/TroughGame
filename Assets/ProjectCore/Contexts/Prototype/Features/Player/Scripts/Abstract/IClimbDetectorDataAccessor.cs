using UnityEngine;

namespace ProjectCore.Prototype
{
    public interface IClimbDetectorDataAccessor
    {
        public bool IsNearValidWall { get; }
        public Vector3 CurrentWallNormal { get; }
    }
}
