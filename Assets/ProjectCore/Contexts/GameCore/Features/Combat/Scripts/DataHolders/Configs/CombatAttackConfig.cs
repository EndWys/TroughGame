using UnityEngine;

namespace ProjectCore.GameCore
{
    [CreateAssetMenu(
        fileName = "Config_GameCore_Combat_Attack",
        menuName = "SO/GameCore/Combat/Attack")]
    public sealed class CombatAttackConfig : ScriptableObject
    {
        [SerializeField] private CombatActionProfileConfig _actionProfile;
        [SerializeField] private CombatShapeSettings _hitbox = new(
            CombatShapeType.Box,
            new Vector2(0.65f, 0f),
            0f,
            new Vector2(0.8f, 0.8f),
            45f);

        public CombatActionProfileConfig ActionProfile => _actionProfile;
        public CombatShapeSettings Hitbox => _hitbox;
    }
}
