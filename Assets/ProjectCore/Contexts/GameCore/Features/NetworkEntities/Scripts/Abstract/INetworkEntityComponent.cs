using Shared;

namespace ProjectCore.GameCore
{
    public interface INetworkEntityComponent : IComposite<INetworkEntityComponent>
    {
        bool IsInitialized { get; }

        bool HasValidNetworkContext { get; }

        bool HasSimulationAuthority { get; }

        bool IsComponentAvailable { get; }

        void Init();

        void NetworkTick();

        void ClientRender();

        void ResetLocalRuntime();
    }
}
