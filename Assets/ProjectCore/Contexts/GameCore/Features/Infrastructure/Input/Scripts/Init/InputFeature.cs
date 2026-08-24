using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCore.GameCore
{
    public sealed class InputFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private InputActionAsset _inputActions;

        protected override void InstallBindings()
        {
            BindInterfacesAndSelfAsSingle<LocalInputAccumulator>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_inputActions == null)
            {
                throw new InvalidOperationException(
                    "InputFeature requires an InputActionAsset.");
            }

            ResolveAs<ILocalInputAccumulator, LocalInputAccumulator>()
                .Initialize(_inputActions);

            return UniTask.CompletedTask;
        }
    }
}
