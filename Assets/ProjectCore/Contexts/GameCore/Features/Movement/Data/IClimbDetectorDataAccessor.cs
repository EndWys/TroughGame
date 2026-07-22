using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IClimbDetectorDataAccessor
    {
        public bool IsNearValidWall { get; }
        public Vector3 CurrentWallNormal { get; }
    }
}
