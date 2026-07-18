using Cysharp.Threading.Tasks;
using Domain;

namespace Prototype.Prototype.Init
{
    public class PrototypeFeature : IBaseFeature
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
