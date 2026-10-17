using UnityEngine;

namespace ProjectCore.GameCore
{
    [CreateAssetMenu(
        fileName = "Config_GameCore_Combat_ActionProfile",
        menuName = "SO/GameCore/Combat/ActionProfile")]
    public sealed class CombatActionProfileConfig : ScriptableObject
    {
        [SerializeField] private ushort _actionId;
        [SerializeField] private CombatActionKind _kind;
        [SerializeField, Min(0f)] private float _preparationSeconds = 0.2f;
        [SerializeField, Min(0f)] private float _activeSeconds = 0.1f;
        [SerializeField, Min(0f)] private float _recoverySeconds = 0.3f;
        [SerializeField, Min(0f)] private float _comboWindowStartSeconds = 0.15f;
        [SerializeField, Min(0f)] private float _comboWindowEndSeconds = 0.25f;

        public ushort ActionId => _actionId;
        public CombatActionKind Kind => _kind;
        public float PreparationSeconds => _preparationSeconds;
        public float ActiveSeconds => _activeSeconds;
        public float RecoverySeconds => _recoverySeconds;
        public float ComboWindowStartSeconds => _comboWindowStartSeconds;
        public float ComboWindowEndSeconds => _comboWindowEndSeconds;
    }
}
