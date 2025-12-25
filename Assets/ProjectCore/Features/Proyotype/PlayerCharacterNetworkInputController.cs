using Fusion;
using ProjectCore.Domain.Scripts.NetworkUtilities;
using UnityEngine;

namespace ProjectCore.Features.Proyotype
{
    public class PlayerCharacterNetworkInputController : BaseNetworkCallbacksBehaviour
    {
        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new PlayerInputData
            {
                Horizontal = Input.GetAxisRaw("Horizontal"),
                Vertical = Input.GetAxisRaw("Vertical")
            };

            input.Set(data);
        }
    }
}