using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public class PrototypeSceneInitializer : BaseSceneInitializer
    {
        protected override UniTask OnInitializeAsync()
        {
            Debug.Log("Prototype Scene Initialized.");

            return UniTask.CompletedTask;
        }
    }
}
