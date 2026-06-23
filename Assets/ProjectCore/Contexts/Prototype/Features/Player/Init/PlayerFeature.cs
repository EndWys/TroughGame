using Cysharp.Threading.Tasks;
using Domain;
using Prototype.Prototype;
using Zenject;

namespace Prototype.Player.Init
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
