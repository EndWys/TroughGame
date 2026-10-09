using UnityEngine;

namespace ProjectCore.GameCore
{
    [CreateAssetMenu(
        fileName = "Config_GameCore_Movement_Dash",
        menuName = "SO/GameCore/Movement/Dash")]
    public sealed class DashMovementConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _distance = 2.5f;
        [SerializeField, Min(0.01f)] private float _durationSeconds = 0.5f;
        [SerializeField, Min(0f)] private float _cooldownSeconds = 10f;

        public float Distance => _distance;
        public float DurationSeconds => _durationSeconds;
        public float CooldownSeconds => _cooldownSeconds;
        public float Speed => _distance / _durationSeconds;
    }
}
