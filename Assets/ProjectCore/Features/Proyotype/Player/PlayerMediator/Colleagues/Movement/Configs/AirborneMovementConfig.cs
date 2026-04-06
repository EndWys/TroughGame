using System;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player.Configs
{
    [Serializable]
    public class AirborneMovementConfig
    {
        [Header("AIRBORNE & JUMP SETTINGS")]
        [field:SerializeField] public float JumpForce { get; private set; } = 7f;
        [field:SerializeField] public float AirControlMultiplier { get; private set; } = 0.5f;
        [field:SerializeField] public float GravityMultiplier { get; private set; } = 2f; 
        [field:SerializeField] public int JumpBufferTicks { get; private set; } = 5;
        [field:SerializeField] public int CoyoteTimeTicks { get; private set; } = 5;
    }
}