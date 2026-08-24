using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class TransformMovementBodyComponent :
        NetworkBehaviour,
        INetworkEntityComponent,
        IMovementBodyMutator
    {
        private static readonly IReadOnlyList<INetworkEntityComponent> _emptyComponents =
            Array.Empty<INetworkEntityComponent>();

        [SerializeField] private Vector2 _offset = new(0f, 0.2f);
        [SerializeField, Min(0f)] private float _collisionRadius = 0.4f;
        [SerializeField] private LayerMask _collisionMask = -1;

        private IMovementSimulationService _movementSimulationService;

        [Inject]
        private void Construct(IMovementSimulationService movementSimulationService)
        {
            _movementSimulationService = movementSimulationService ??
                throw new ArgumentNullException(nameof(movementSimulationService));
        }

        public Vector2 Velocity { get; set; }

        public Vector2 Position
        {
            get
            {
                Vector3 position = transform.position;
                position.x += _offset.x;
                position.y += _offset.y;
                return new Vector2(position.x, position.y);
            }
        }

        public IReadOnlyList<INetworkEntityComponent> Components => _emptyComponents;
        public Vector2 Offset => _offset;
        public float CollisionRadius => _collisionRadius;
        public LayerMask CollisionMask => _collisionMask;

        private void OnDrawGizmosSelected()
        {
            Color previousColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            Vector3 bodyPosition = transform.position + (Vector3)_offset;
            Gizmos.DrawWireSphere(bodyPosition, _collisionRadius);
            Gizmos.color = previousColor;
        }

        public void Init()
        {
        }

        public void NetworkTick()
        {
            if (!HasStateAuthority && !HasInputAuthority)
            {
                return;
            }

            _movementSimulationService.Simulate(this, Runner.DeltaTime);
        }

        public void ClientRender()
        {
        }

        public void ApplySimulation(
            Vector2 resolvedVelocity,
            Vector2 resolvedDisplacement)
        {
            Velocity = resolvedVelocity;

            Vector3 position = transform.position;
            position.x += resolvedDisplacement.x;
            position.y += resolvedDisplacement.y;
            transform.position = position;
        }
    }
}
