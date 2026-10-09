using System;
using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDebugVisualizationComponent : BaseDebugInspectableComponent, IDebugDrawable
    {
        private const string BehaviourChannel = "Enemy/Behaviour";
        private const string TargetChannel = "Enemy/Target";
        private const string SteeringChannel = "Enemy/Steering";

        [SerializeField] private EnemyNetworkEntityComponent _enemyNetworkEntity;

        private NetworkEntityRegistry _networkEntityRegistry;
        private IEnemySteeringService _enemySteeringService;

        [Inject]
        private void Construct(
            NetworkEntityRegistry networkEntityRegistry,
            IEnemySteeringService enemySteeringService)
        {
            _networkEntityRegistry = networkEntityRegistry ??
                throw new ArgumentNullException(nameof(networkEntityRegistry));
            _enemySteeringService = enemySteeringService ??
                throw new ArgumentNullException(nameof(enemySteeringService));
        }

        public void DrawDebug(DebugVisualizationContextAdapter context)
        {
            if (_enemyNetworkEntity == null)
            {
                return;
            }

            context.Value(BehaviourChannel, "Current", _enemyNetworkEntity.CurrentBehaviourState);
            context.Value(BehaviourChannel, "Previous", _enemyNetworkEntity.PreviousBehaviourState);
            context.Value(TargetChannel, "Entity", _enemyNetworkEntity.TargetEntityId);
            context.Value(SteeringChannel, "Approach position", _enemySteeringService.ApproachPosition);
            context.Line(transform.position, _enemySteeringService.ApproachPosition, SteeringChannel);

            if (_networkEntityRegistry == null ||
                !_networkEntityRegistry.TryGet(_enemyNetworkEntity.TargetEntityId, out BaseNetworkEntityRoot target))
            {
                return;
            }

            context.Line(transform.position, target.transform.position, TargetChannel);
        }
    }
}
