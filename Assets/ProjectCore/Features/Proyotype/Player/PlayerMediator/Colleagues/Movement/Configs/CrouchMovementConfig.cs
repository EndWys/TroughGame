using System;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player.Configs
{
    [Serializable]
    public class CrouchMovementConfig
    {
        [Header("CROUCH SETTINGS")] 
        [field:SerializeField] public float CrouchSpeed { get; private set; } = 2.5f;
    }
}