using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public abstract class BaseDamageSourceComponent : BaseNetworkEntityComponent
    {
        public abstract byte DamageAmount { get; }

        public abstract string DamageType { get; }
    }
}
