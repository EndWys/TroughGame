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

        [SerializeField] private EnemyNetworkEntityComponent _enemyNetworkEntity;

        private NetworkEntityRegistry _networkEntityRegistry;

        [Inject]
        private void Construct(NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry ??
                throw new ArgumentNullException(nameof(networkEntityRegistry));
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

            if (_networkEntityRegistry == null ||
                !_networkEntityRegistry.TryGet(_enemyNetworkEntity.TargetEntityId, out BaseNetworkEntityRoot target))
            {
                return;
            }

            context.Line(transform.position, target.transform.position, TargetChannel);
        }
    }
}
