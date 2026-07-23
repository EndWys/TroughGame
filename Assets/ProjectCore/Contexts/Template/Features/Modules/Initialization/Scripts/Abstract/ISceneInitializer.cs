using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    public interface ISceneInitializer
    {
        UniTask InitializeAsync();
    }
}
