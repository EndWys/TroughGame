using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface ICameraTarget
    {
        Transform TargetTransform { get; }
    }
}
