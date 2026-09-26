using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyTargetingService
    {
        private readonly IGameEntityComponentAccessor _gameEntityComponentAccessor;
        private readonly NetworkEntityRegistry _networkEntityRegistry;

        public EnemyTargetingService(
            IGameEntityComponentAccessor gameEntityComponentAccessor,
            NetworkEntityRegistry networkEntityRegistry)
        {
            _gameEntityComponentAccessor = gameEntityComponentAccessor ??
                throw new ArgumentNullException(nameof(gameEntityComponentAccessor));
            _networkEntityRegistry = networkEntityRegistry ??
                throw new ArgumentNullException(nameof(networkEntityRegistry));
        }

        public bool HasAvailableTarget()
        {
            return _gameEntityComponentAccessor.TryFindEntityId<ITargetableNetworkEntity>(
                _ => true,
                out _);
        }

        public bool IsTargetAvailable(NetworkEntityIdData targetEntityId)
        {
            return targetEntityId.IsValid &&
                   _networkEntityRegistry.TryGetComponent(
                       targetEntityId,
                       out ITargetableNetworkEntity _);
        }

        public bool TryFindNearestTarget(
            Vector2 origin,
            out NetworkEntityIdData targetEntityId)
        {
            return _gameEntityComponentAccessor.TryFindFirstEntityIdSortedBy<
                ITargetableNetworkEntity,
                float>(
                target => (target.TargetPosition - origin).sqrMagnitude,
                out targetEntityId);
        }
    }
}
