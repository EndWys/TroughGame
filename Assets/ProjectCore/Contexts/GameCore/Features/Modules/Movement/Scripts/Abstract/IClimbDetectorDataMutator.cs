using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IClimbDetectorDataMutator : IClimbDetectorDataAccessor
    {
        public void ChangeWallAvailability(bool hasAvailableWall);
        public void ChangeCurrentWallNormal(Vector3 newWallNormal);
    }
}
