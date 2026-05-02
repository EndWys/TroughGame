using UnityEngine;

namespace Prototype.Prototype
{
    public interface IClimbDetectorDataChanger : IClimbDetectorDataAccessor
    {
        public void ChangeWallAvailability(bool hasAvailableWall);
        public void ChangeCurrentWallNormal(Vector3 newWallNormal);
    }
}