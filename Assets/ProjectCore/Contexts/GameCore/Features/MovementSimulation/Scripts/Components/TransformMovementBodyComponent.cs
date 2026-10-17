using System;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class TransformMovementBodyComponent :
        BaseNetworkEntityBehaviourComponent,
        IMovementBodyMutator
    {
        [SerializeField] private Vector2 _offset = new(0f, 0.2f);
        [SerializeField, Min(0f)] private float _collisionRadius = 0.4f;
        [SerializeField] private LayerMask _collisionMask = -1;

        private IMovementSimulationService _movementSimulationService;
        private IEntityPositionAccessor _position;

        [Inject]
        private void Construct(IMovementSimulationService movementSimulationService, IEntityPositionAccessor position)
        {
            _movementSimulationService = movementSimulationService ??
                throw new ArgumentNullException(nameof(movementSimulationService));
            _position = position ?? throw new ArgumentNullException(nameof(position));
        }

        private Vector2 _velocity;

        public Vector2 Velocity => _velocity;

        Vector2 IMovementBodyVelocityMutator.Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        public Vector2 Position => _position.Position + _offset;

        public Vector2 Offset => _offset;
        public float CollisionRadius => _collisionRadius;
        public LayerMask CollisionMask => _collisionMask;

        private void OnDrawGizmosSelected()
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            Vector3 bodyPosition = transform.position + (Vector3)_offset;
            DrawWireCircle(bodyPosition, _collisionRadius);
            Gizmos.color = previousColor;
        }

        private static void DrawWireCircle(Vector3 center, float radius)
        {
            const int segments = 32;
            Vector3 previous = center + Vector3.right * radius;

            for (int segment = 1; segment <= segments; segment++)
            {
                float angle = segment / (float)segments * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        protected override void OnNetworkTick()
        {
            if (!HasStateAuthority && !HasInputAuthority)
            {
                return;
            }

            _movementSimulationService.Simulate(this, Runner.DeltaTime);
        }

        protected override void OnResetLocalRuntime()
        {
            _velocity = Vector2.zero;
        }

        void IMovementBodyMutator.ApplySimulation(
            Vector2 resolvedVelocity,
            Vector2 resolvedDisplacement)
        {
            _velocity = resolvedVelocity;

            Vector3 position = transform.position;
            position.x += resolvedDisplacement.x;
            position.y += resolvedDisplacement.y;
            transform.position = position;
        }
    }
}
