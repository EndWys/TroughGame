using Shared;
using UnityEngine.Scripting.APIUpdating;

namespace ProjectCore.Template
{
    [MovedFrom(true, sourceNamespace: "ProjectCore.Template", sourceAssembly: "ProjectCore.Runtime", sourceClassName: "PayloadMemberHandler")]
    public abstract class BasePayloadMemberHandler<TPayload> : BaseMemberHandler
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
