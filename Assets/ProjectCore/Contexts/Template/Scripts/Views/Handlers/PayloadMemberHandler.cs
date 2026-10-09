using Shared;
using UnityEngine.Scripting.APIUpdating;

namespace ProjectCore.Template
{
    [MovedFrom(true, sourceNamespace: "Domain", sourceAssembly: "ProjectCore.Runtime", sourceClassName: null)]
    public abstract class PayloadMemberHandler<TPayload> : MemberHandler
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
