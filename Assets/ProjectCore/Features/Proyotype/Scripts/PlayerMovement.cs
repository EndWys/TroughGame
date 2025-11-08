using Fusion;
using UnityEngine;

namespace ProjectCore.Features.Proyotype
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float _speed = 5f;

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerInputData input))
            {
                Vector3 move = new Vector3(input.Horizontal, 0, input.Vertical);
                transform.position += move * _speed * Runner.DeltaTime;
            }
        }
    }
}