using UnityEngine;

namespace Prototype.Prototype
{
    public interface IGroundDetectorDataChanger : IGroundDetectorDataAccessor
    {
        public void ChangeGroundedStatus(bool isGrounded);
        public void ChangeGroundNormal(Vector3 groundNormal);
    }
}