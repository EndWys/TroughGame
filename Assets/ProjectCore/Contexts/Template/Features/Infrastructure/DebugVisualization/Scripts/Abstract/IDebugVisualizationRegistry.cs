using UnityEngine;

namespace ProjectCore.Template
{
    internal interface IDebugVisualizationRegistry
    {
        public void Register(Object target);
        public void Unregister(Object target);
        public void Clear();
        public void Refresh(IDebugVisualizationBackend backend);
    }
}
