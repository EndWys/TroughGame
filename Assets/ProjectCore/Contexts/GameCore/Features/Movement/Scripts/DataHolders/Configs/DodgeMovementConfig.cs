using UnityEngine;

namespace ProjectCore.GameCore
{
    [CreateAssetMenu(
        fileName = "Config_GameCore_Movement_Dodge",
        menuName = "SO/GameCore/Movement/Dodge")]
    public sealed class DodgeMovementConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _speed = 8f;
        [SerializeField, Min(0.01f)] private float _durationSeconds = 0.2f;
        [SerializeField, Min(0.001f)] private float _preparationSeconds = 0.03f;
        [SerializeField, Min(0.001f)] private float _recoverySeconds = 0.1f;

        public float Speed => _speed;
        public float DurationSeconds => _durationSeconds;
        public float PreparationSeconds => _preparationSeconds;
        public float RecoverySeconds => _recoverySeconds;
    }
}
