using System;
using UnityEngine;

namespace GameCore.Movement
{
    [Serializable]
    public class LocomotionMovementConfig
    {
        [Header("LOCOMOTION SETTINGS")]
        [field:SerializeField] public float WalkSpeed { get; private set; } = 5f;
        [field:SerializeField] public float RunSpeed { get; private set; } = 8f;
        [field:SerializeField] public float RotationSpeed { get; private set; } = 150f;
    }
}