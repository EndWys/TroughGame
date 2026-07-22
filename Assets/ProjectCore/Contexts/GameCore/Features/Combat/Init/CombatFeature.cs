using Cysharp.Threading.Tasks;
using Domain;
using Zenject;

namespace GameCore.Combat.Init
{
    public sealed class CombatFeature : IBaseFeature
    {
        private readonly DiContainer _container;

        [Inject]
        public CombatFeature(DiContainer container)
        {
            _container = container;
        }

        public void InstallBindings()
        {
            _container.BindInterfacesTo<DamageableSystem>().AsSingle();
            _container.BindInterfacesTo<DamageSourceSystem>().AsSingle();
        }

        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }
    }
}
