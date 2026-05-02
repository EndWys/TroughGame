using Fusion;

namespace Prototype.Prototype
{
    public interface INetworkBehaviourAccessor
    {
        public NetworkBehaviour ParentNetworkBehaviour { get; }
    }
}