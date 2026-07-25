using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    [Serializable]
    public sealed class CrouchMovementConfig
    {
        [Header("CROUCH SETTINGS")] 
        [field:SerializeField] public float CrouchSpeed { get; private set; } = 2.5f;
    }
}
