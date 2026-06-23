using Cysharp.Threading.Tasks;
using Domain;

namespace Prototype.Player.Init
{
    public class PlayerFeature : IBaseFeature
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
