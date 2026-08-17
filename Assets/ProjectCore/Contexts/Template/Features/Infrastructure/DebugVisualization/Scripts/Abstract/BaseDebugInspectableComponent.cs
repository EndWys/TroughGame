using UnityEngine;

namespace ProjectCore.Template
{
    public abstract class BaseDebugInspectableComponent : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            DebugVisualizationUtility.Register(this);
        }

        protected virtual void OnDisable()
        {
            DebugVisualizationUtility.Unregister(this);
        }
    }
}
