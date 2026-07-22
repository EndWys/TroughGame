using Fusion;

namespace ProjectCore.GameCore
{
    public interface INetworkBehaviourAccessor
    {
        NetworkBehaviour ParentNetworkBehaviour { get; }
    }
}
