using UnityEngine;

namespace Domain
{
    public abstract class MemberHandler : MonoBehaviour
    {
        public abstract void Handle(HandlerPayload payload);
    }

    public abstract class MemberHandler<TPayload> : MemberHandler
        where TPayload : HandlerPayload
    {
        public override void Handle(HandlerPayload payload)
        {
            if (payload is TPayload typedPayload)
            {
                Handle(typedPayload);
            }
        }

        public abstract void Handle(TPayload payload);
    }
}
