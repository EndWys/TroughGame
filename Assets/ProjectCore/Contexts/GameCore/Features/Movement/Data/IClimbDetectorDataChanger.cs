using UnityEngine;

namespace GameCore.Movement
{
    public interface IClimbDetectorDataChanger : IClimbDetectorDataAccessor
    {
        public void ChangeWallAvailability(bool hasAvailableWall);
        public void ChangeCurrentWallNormal(Vector3 newWallNormal);
    }
}