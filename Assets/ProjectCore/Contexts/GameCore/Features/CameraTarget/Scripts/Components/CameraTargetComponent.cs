using System;
using Fusion;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class CameraTargetComponent :
        BaseNetworkEntityBehaviourComponent,
        ICameraTarget
    {
        private ICameraTargetRegistry _cameraTargetRegistry;

        [Inject]
        private void Construct(ICameraTargetRegistry cameraTargetRegistry)
        {
            _cameraTargetRegistry = cameraTargetRegistry ??
                throw new ArgumentNullException(nameof(cameraTargetRegistry));
        }

        public Transform TargetTransform => transform;

        protected override void OnInitialize()
        {
            if (HasInputAuthority)
            {
                _cameraTargetRegistry.Register(this);
            }
        }

        protected override void OnResetLocalRuntime()
        {
            _cameraTargetRegistry?.Unregister(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            ResetLocalRuntime();
        }

        private void OnDestroy()
        {
            ResetLocalRuntime();
        }
    }
}
