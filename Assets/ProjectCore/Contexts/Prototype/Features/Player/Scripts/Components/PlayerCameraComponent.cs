using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public sealed class PlayerCameraComponent :
        BaseNetworkEntityComponent,
        IPlayerColleague
    {
        [Header("REFERENCES")]
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _cameraLookTarget;

        [Header("FOV SETTINGS")]
        [SerializeField] private float _baseFOV = 60f;
        [SerializeField] private float _bustedFOVBonus = 10f;

        private FollowCameraComponent _cameraController;

        protected override void OnInitialize()
        {
            if (!ParentNetworkBehaviour.HasInputAuthority)
            {
                return;
            }

            _cameraController = FindAnyObjectByType<FollowCameraComponent>();

            if (_cameraController != null)
            {
                _cameraController.SetTarget(_playerTarget, _cameraLookTarget);
            }
        }

        public void ChangeFieldOfView(PlayerMovementStateChangedPayload playerMovementStateChangedPayload)
        {
            if (_cameraController == null)
            {
                return;
            }

            _cameraController.ChangeFOV(playerMovementStateChangedPayload.MovementStates == MovementStates.Run ? _baseFOV + _bustedFOVBonus : _baseFOV);
        }
    }
}
