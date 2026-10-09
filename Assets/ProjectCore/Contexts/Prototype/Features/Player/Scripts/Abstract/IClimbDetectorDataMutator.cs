using UnityEngine;

namespace ProjectCore.Prototype
{
    public interface IClimbDetectorDataMutator : IClimbDetectorDataAccessor
    {
        public void ChangeWallAvailability(bool hasAvailableWall);
        public void ChangeCurrentWallNormal(Vector3 newWallNormal);
    }
}
