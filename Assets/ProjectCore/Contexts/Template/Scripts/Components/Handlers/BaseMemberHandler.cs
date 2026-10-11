using Shared;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ProjectCore.Template
{
    [MovedFrom(true, sourceNamespace: "ProjectCore.Template", sourceAssembly: "ProjectCore.Runtime", sourceClassName: "MemberHandler")]
    public abstract class BaseMemberHandler : MonoBehaviour
    {
        public abstract void Handle(HandlerPayload payload);
    }
}
