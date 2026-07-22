using Cysharp.Threading.Tasks;
using ProjectCore.Template;

namespace ProjectCore.Prototype
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
