using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class CameraTargetComponent :
        NetworkBehaviour,
        INetworkEntityComponent,
        ICameraTarget
    {
        private static readonly IReadOnlyList<INetworkEntityComponent> _emptyComponents =
            Array.Empty<INetworkEntityComponent>();

        private ICameraTargetRegistry _cameraTargetRegistry;

        [Inject]
        private void Construct(ICameraTargetRegistry cameraTargetRegistry)
        {
            _cameraTargetRegistry = cameraTargetRegistry ??
                throw new ArgumentNullException(nameof(cameraTargetRegistry));
        }

        public Transform TargetTransform => transform;

        public IReadOnlyList<INetworkEntityComponent> Components => _emptyComponents;

        public void Init()
        {
            if (HasInputAuthority)
            {
                _cameraTargetRegistry.Register(this);
            }
        }

        public void NetworkTick()
        {
        }

        public void ClientRender()
        {
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _cameraTargetRegistry?.Unregister(this);
        }

        private void OnDestroy()
        {
            _cameraTargetRegistry?.Unregister(this);
        }
    }
}
