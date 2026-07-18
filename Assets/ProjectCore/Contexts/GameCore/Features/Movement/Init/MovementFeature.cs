using Cysharp.Threading.Tasks;
using Domain;

namespace GameCore.Movement.Init
{
    public class MovementFeature : IBaseFeature
    {
        public void InstallBindings()
        {
        }

        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }
    }
}
