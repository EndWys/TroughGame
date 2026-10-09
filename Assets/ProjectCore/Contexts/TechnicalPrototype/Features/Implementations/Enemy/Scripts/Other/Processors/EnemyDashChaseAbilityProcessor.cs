using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyDashChaseAbilityProcessor : BaseEnemyChaseAbilityProcessor
    {
        private readonly INetworkBehaviourAccessor _networkBehaviourAccessor;
        private readonly IEnemyDashStateAccessor _dashStateAccessor;
        private readonly DashMovementConfig _dashMovementConfig;
        private readonly float _targetOffset;
        private readonly float _startDistanceTolerance;

        public EnemyDashChaseAbilityProcessor(
            INetworkBehaviourAccessor networkBehaviourAccessor,
            IEnemyDashStateAccessor dashStateAccessor,
            DashMovementConfig dashMovementConfig,
            float targetOffset,
            float startDistanceTolerance)
        {
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
            _dashStateAccessor = dashStateAccessor ??
                throw new ArgumentNullException(nameof(dashStateAccessor));
            _dashMovementConfig = dashMovementConfig ??
                throw new ArgumentNullException(nameof(dashMovementConfig));
            _targetOffset = targetOffset;
            _startDistanceTolerance = startDistanceTolerance;
        }

        public override bool TryCreateInput(
            NetworkEntityIdData targetEntityId,
            Vector2 targetPosition,
            Vector2 approachPosition,
            out EnemyInputData input)
        {
            if (targetEntityId.Type != PlayerNetworkEntityConstants.Player ||
                !_dashStateAccessor.IsDashCooldownFinished)
            {
                input = default;
                return false;
            }

            Vector2 enemyPosition = _networkBehaviourAccessor.ParentNetworkBehaviour.transform.position;
            Vector2 targetOffsetDirection = GetTargetOffsetDirection(
                enemyPosition,
                targetPosition,
                approachPosition);
            Vector2 dashTargetPosition = targetPosition + targetOffsetDirection * _targetOffset;
            Vector2 dashDirection = dashTargetPosition - enemyPosition;
            float dashDistance = dashDirection.magnitude;

            if (Mathf.Abs(dashDistance - _dashMovementConfig.Distance) > _startDistanceTolerance ||
                dashDistance <= Mathf.Epsilon)
            {
                input = default;
                return false;
            }

            input = new EnemyInputData(
                direction: dashDirection.normalized,
                isDashRequested: true,
                dashTargetPosition: dashTargetPosition);
            return true;
        }

        private static Vector2 GetTargetOffsetDirection(
            Vector2 enemyPosition,
            Vector2 targetPosition,
            Vector2 approachPosition)
        {
            Vector2 approachDirection = approachPosition - targetPosition;

            if (approachDirection.sqrMagnitude > Mathf.Epsilon)
            {
                return approachDirection.normalized;
            }

            Vector2 enemyDirection = enemyPosition - targetPosition;
            return enemyDirection.sqrMagnitude > Mathf.Epsilon ? enemyDirection.normalized : Vector2.right;
        }
    }
}
