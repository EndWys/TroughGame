using UnityEngine;

namespace GameCore.Movement
{
    public interface IGroundDetectorDataChanger : IGroundDetectorDataAccessor
    {
        public void ChangeGroundedStatus(bool isGrounded);
        public void ChangeGroundNormal(Vector3 groundNormal);
    }
}