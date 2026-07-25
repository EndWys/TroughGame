
namespace ProjectCore.GameCore
{
    public abstract class BaseDamageableComponent : BaseNetworkEntityComponent
    {
        public abstract int Durability { get; }

        public abstract bool IsDestroyed { get; }

        public abstract void ApplyDamage(byte amount);
    }
}
