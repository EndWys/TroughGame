using Shared;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ProjectCore.Template
{
    [MovedFrom(true, sourceNamespace: "Domain", sourceAssembly: "ProjectCore.Runtime", sourceClassName: null)]
    public abstract class MemberHandler : MonoBehaviour
    {
        public abstract void Handle(HandlerPayload payload);
    }
}
