using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class TargetableNetworkEntityComponent :
        BaseNetworkEntityComponent,
        ITargetableNetworkEntity
    {
        public Vector2 TargetPosition => transform.position;
    }
}
