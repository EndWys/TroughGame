using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ProjectCore.Template
{
    [AttributeUsage(AttributeTargets.Field)]
    [MovedFrom(true, sourceNamespace: "Domain", sourceAssembly: "ProjectCore.Runtime", sourceClassName: null)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}
