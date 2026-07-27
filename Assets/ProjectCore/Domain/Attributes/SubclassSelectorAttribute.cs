using System;
using UnityEngine;

namespace Domain
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}
