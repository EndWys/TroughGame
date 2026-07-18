using UnityEngine;

namespace GameCore.Movement
{
    public interface IClimbDetectorDataAccessor
    {
        public bool IsNearValidWall { get; }
        public Vector3 CurrentWallNormal { get; }
    }
}