using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectCore.Template
{
    public abstract class BaseSceneInitializer : MonoBehaviour, ISceneInitializer
    {
        private bool _isInitialized;

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await OnInitializeAsync();
            _isInitialized = true;
        }

        protected abstract UniTask OnInitializeAsync();
    }
}
