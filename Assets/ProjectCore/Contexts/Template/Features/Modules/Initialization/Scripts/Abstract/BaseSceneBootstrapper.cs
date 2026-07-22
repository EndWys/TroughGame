using UnityEngine;

namespace ProjectCore.Template
{
    public abstract class BaseSceneBootstrapper : MonoBehaviour
    {
        private bool _isInitialized = false;

        private void Start()
        {
            if (!_isInitialized)
            {
                Init();
                _isInitialized = true;
            }
        }

        protected abstract void Init();
    }
}
