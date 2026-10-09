using UnityEngine;

namespace ProjectCore.GameCore
{
    [CreateAssetMenu(
        fileName = "Config_GameCore_Movement_Locomotion",
        menuName = "SO/GameCore/Movement/Locomotion")]
    public sealed class LocomotionMovementConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _maxSpeed = 5f;

        public float MaxSpeed => _maxSpeed;
    }
}
