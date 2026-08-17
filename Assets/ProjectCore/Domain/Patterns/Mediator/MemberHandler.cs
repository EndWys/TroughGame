using UnityEngine;

namespace Domain
{
    public abstract class MemberHandler : MonoBehaviour
    {
        public abstract void Handle(HandlerPayload payload);
    }
}
