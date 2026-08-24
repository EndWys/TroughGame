using System;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkInputComponent : BaseNetworkCallbacksBehaviour
    {
        private ILocalInputAccumulator _localInputAccumulator;

        [Inject]
        private void Construct(ILocalInputAccumulator localInputAccumulator)
        {
            _localInputAccumulator = localInputAccumulator ??
                throw new ArgumentNullException(nameof(localInputAccumulator));
        }

        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            Vector2 moveDirection = _localInputAccumulator.ReadValue<Vector2>(
                PlayerInputActionConstants.Move);

            input.Set(new PlayerInputData
            {
                MoveDirection = Vector2.ClampMagnitude(moveDirection, 1f),
            });
        }
    }
}
