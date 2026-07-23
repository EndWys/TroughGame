using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface IGroundDetectorDataMutator : IGroundDetectorDataAccessor
    {
        public void ChangeGroundedStatus(bool isGrounded);
        public void ChangeGroundNormal(Vector3 groundNormal);
    }
}
