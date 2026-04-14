using Cysharp.Threading.Tasks;
namespace ProjectCore.Domain.DITools
{
    public interface IBaseFeature
    {
        public void InstallBindings();
        public UniTask Init();
    }
}
