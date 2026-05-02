using Fusion;

namespace Prototype.Prototype
{
    public interface INetworkDataHolder<TNetworkData> where TNetworkData : INetworkStruct
    {
        public abstract TNetworkData Data { get; set; }
    }
}