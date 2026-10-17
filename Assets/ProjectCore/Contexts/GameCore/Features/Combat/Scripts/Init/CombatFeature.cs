using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class CombatFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<IDamageableEntityService, DamageableEntityService>();
            BindAsSingle<IDamageSourceEntityService, DamageSourceEntityService>();
            BindAsSingle<ICombatTargetQuery, CombatTargetQueryService>();
        }
    }
}
