using Cysharp.Threading.Tasks;
using Domain;
using ProjectCore.Template;
using ProjectCore.Prototype;
using Zenject;

namespace ProjectCore.Prototype
{
    public class PlayerFeature : IBaseFeature
    {
        private readonly DiContainer _container;

        [Inject]
        public PlayerFeature(DiContainer container)
        {
            _container = container;
        }

        public void InstallBindings()
        {
            _container.BindInterfacesTo<PlayerNetworkEntityFactory>().AsSingle();
        }

        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }
    }
}
