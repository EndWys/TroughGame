using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class LocalCameraFeature : BaseMonoBehaviourFeature
    {
        [SerializeField]
        private LocalCameraComponent _cameraComponent;

        protected override void InstallBindings()
        {
            if (_cameraComponent == null)
            {
                throw new MissingReferenceException(
                    $"{nameof(LocalCameraFeature)} requires a {nameof(LocalCameraComponent)} reference.");
            }

            BindFromInstance<ILocalCamera, LocalCameraComponent>(_cameraComponent);
        }
    }
}
