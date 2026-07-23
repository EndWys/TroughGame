using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    public interface IFeatureInitializer
    {
        UniTask InitializeAsync();
    }
}
