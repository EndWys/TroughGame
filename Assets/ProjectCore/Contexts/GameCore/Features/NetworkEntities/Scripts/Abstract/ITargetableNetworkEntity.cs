using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface ITargetableNetworkEntity
    {
        Vector2 TargetPosition { get; }
    }
}
