using Domain;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public class PlayerCharacterNetworkInputController : BaseNetworkCallbacksBehaviour
    {
        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new PlayerInputData
            {
                MoveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized,
                LookYawDelta = Cursor.lockState == CursorLockMode.Locked ? 0f : Input.GetAxisRaw("Mouse X"),
                IsRunning = Input.GetKey(KeyCode.LeftShift),
                IsJumpPressed = Input.GetKeyDown(KeyCode.Space),
                IsCrouchPressed = Input.GetKey(KeyCode.C),
            };

            input.Set(data);
        }
    }
}
