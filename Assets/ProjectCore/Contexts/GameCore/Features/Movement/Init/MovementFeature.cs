using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.GameCore
{
    public class MovementFeature : IBaseFeature
    {
        private readonly DiContainer _container;

        [Inject]
        public MovementFeature(DiContainer container)
        {
            _container = container;
        }

        public void InstallBindings()
        {
            _container.BindInterfacesTo<MovementSystem>().AsSingle();
        }

        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }
    }
}
