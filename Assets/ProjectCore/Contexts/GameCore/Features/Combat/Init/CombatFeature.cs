using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.GameCore
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
