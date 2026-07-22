using Cysharp.Threading.Tasks;
namespace ProjectCore.Template
{
    public interface IBaseFeature
    {
        public void InstallBindings();
        public UniTask Init();
    }
}
