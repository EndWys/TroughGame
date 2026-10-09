using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class CombatFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<IDamageableSystem, DamageableSystem>();
            BindAsSingle<IDamageSourceSystem, DamageSourceSystem>();
        }
    }
}
