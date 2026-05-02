using UnityEngine;

namespace Prototype.Prototype
{
    public interface IClimbDetectorDataAccessor
    {
        public bool IsNearValidWall { get; }
        public Vector3 CurrentWallNormal { get; }
    }
}