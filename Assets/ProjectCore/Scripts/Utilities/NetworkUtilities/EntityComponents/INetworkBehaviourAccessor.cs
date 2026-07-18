using Fusion;

namespace Domain
{
    public interface INetworkBehaviourAccessor
    {
        NetworkBehaviour ParentNetworkBehaviour { get; }
    }
}
