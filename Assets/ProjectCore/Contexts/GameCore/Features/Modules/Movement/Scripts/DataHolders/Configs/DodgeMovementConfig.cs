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

        public float Speed => _speed;
        public float DurationSeconds => _durationSeconds;
    }
}
