using Cysharp.Threading.Tasks;
using Domain;

namespace Prototype.Movement.Init
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
