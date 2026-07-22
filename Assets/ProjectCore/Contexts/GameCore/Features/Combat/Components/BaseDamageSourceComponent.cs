using Domain;

namespace GameCore.Combat
{
    public abstract class BaseDamageSourceComponent : BaseNetworkEntityComponent
    {
        public abstract byte DamageAmount { get; }

        public abstract string DamageType { get; }
    }
}
