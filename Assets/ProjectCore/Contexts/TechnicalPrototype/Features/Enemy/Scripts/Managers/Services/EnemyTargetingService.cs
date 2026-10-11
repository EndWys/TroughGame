using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyTargetingService : IEnemyTargetingService
    {
        private readonly IGameEntityComponentService _gameEntityComponentService;
        private readonly NetworkEntityRegistry _networkEntityRegistry;

        public EnemyTargetingService(
            IGameEntityComponentService gameEntityComponentService,
            NetworkEntityRegistry networkEntityRegistry)
        {
            _gameEntityComponentService = gameEntityComponentService ??
                throw new ArgumentNullException(nameof(gameEntityComponentService));
            _networkEntityRegistry = networkEntityRegistry ??
                throw new ArgumentNullException(nameof(networkEntityRegistry));
        }

        public bool HasAvailableTarget()
        {
            return _gameEntityComponentService.TryFindEntityId<ITargetableNetworkEntity>(
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

        public bool TryGetTargetPosition(
            NetworkEntityIdData targetEntityId,
            out Vector2 targetPosition)
        {
            if (_networkEntityRegistry.TryGetComponent(
                    targetEntityId,
                    out ITargetableNetworkEntity target))
            {
                targetPosition = target.TargetPosition;
                return true;
            }

            targetPosition = default;
            return false;
        }

        public bool TryFindNearestTarget(
            Vector2 origin,
            out NetworkEntityIdData targetEntityId)
        {
            return _gameEntityComponentService.TryFindFirstEntityIdSortedBy<
                ITargetableNetworkEntity,
                float>(
                target => (target.TargetPosition - origin).sqrMagnitude,
                out targetEntityId);
        }
    }
}
