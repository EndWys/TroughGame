using System;
using UnityEngine;

namespace Prototype.Prototype
{
    [Serializable]
    public class CrouchMovementConfig
    {
        [Header("CROUCH SETTINGS")] 
        [field:SerializeField] public float CrouchSpeed { get; private set; } = 2.5f;
    }
}