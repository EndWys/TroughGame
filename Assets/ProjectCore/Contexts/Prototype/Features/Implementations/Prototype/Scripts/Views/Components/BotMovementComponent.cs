using Fusion;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public sealed class BotMovementComponent : NetworkBehaviour
    {
        [SerializeField] private float _speed = 3f;

        private Vector3 _target;

        public override void Spawned()
        {
            if (HasStateAuthority)
                SetNewTarget();
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            Vector3 dir = (_target - transform.position);

            if (dir.sqrMagnitude < 0.5f * 0.5f)
            {
                SetNewTarget();
            }
            else
            {
                transform.position += dir.normalized * _speed * Runner.DeltaTime;
            }
        }

        private void SetNewTarget()
        {
            _target = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        }
    }
}
