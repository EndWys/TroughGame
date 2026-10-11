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
            BindInterfacesAndSelfAsSingle<LocalInputProvider>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_inputActions == null)
            {
                throw new InvalidOperationException(
                    "InputFeature requires an InputActionAsset.");
            }

            ResolveAs<ILocalInputReader, LocalInputProvider>()
                .Initialize(_inputActions);

            return UniTask.CompletedTask;
        }
    }
}
