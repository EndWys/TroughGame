using Cysharp.Threading.Tasks;
namespace Domain
{
    public interface IBaseFeature
    {
        public void InstallBindings();
        public UniTask Init();
    }
}
