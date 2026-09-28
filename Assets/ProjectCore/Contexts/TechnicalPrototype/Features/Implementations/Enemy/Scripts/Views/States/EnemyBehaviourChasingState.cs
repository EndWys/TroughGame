using System;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourChasingState : EnemyBehaviourState
    {
        [SerializeField] private EnemyChaseConfig _chaseConfig;

        private EnemyInputSourceComponent _inputSource;
        private IEnemyTargetAccessor _targetAccessor;
        private IEnemyTargetMutator _targetMutator;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;
        private IEnemyTargetingService _targetingService;
        private IEnemySteeringService _steeringService;
        private NetworkEntityIdData _retargetCandidateId;
        private float _retargetElapsedTime;

        [Inject]
        private void Construct(
            EnemyInputSourceComponent inputSource,
            IEnemyTargetAccessor targetAccessor,
            IEnemyTargetMutator targetMutator,
            INetworkBehaviourAccessor networkBehaviourAccessor,
            IEnemyTargetingService targetingService,
            IEnemySteeringService steeringService)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            _targetAccessor = targetAccessor ?? throw new ArgumentNullException(nameof(targetAccessor));
            _targetMutator = targetMutator ?? throw new ArgumentNullException(nameof(targetMutator));
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
            _targetingService = targetingService ?? throw new ArgumentNullException(nameof(targetingService));
            _steeringService = steeringService ?? throw new ArgumentNullException(nameof(steeringService));
        }

        public override void Enter()
        {
            _retargetCandidateId = NetworkEntityIdData.None;
            _retargetElapsedTime = 0f;
        }

        public override EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload)
        {
            if (_chaseConfig == null)
            {
                throw new InvalidOperationException("Enemy chasing state requires a chase config reference.");
            }

            if (!_targetingService.TryGetTargetPosition(
                    _targetAccessor.TargetEntityId,
                    out Vector2 targetPosition))
            {
                _inputSource.SetInput(default);
                _targetMutator.ClearTargetEntityId();
                return EnemyBehaviourStateType.TargetSelection;
            }

            Vector2 position = _networkBehaviourAccessor.ParentNetworkBehaviour.transform.position;
            NetworkEntityIdData targetEntityId = ReassessTarget(
                position,
                targetPosition,
                _targetAccessor.TargetEntityId);

            if (targetEntityId != _targetAccessor.TargetEntityId &&
                _targetingService.TryGetTargetPosition(targetEntityId, out targetPosition))
            {
                _targetMutator.SetTargetEntityId(targetEntityId);
            }

            Vector2 direction = _steeringService.CalculateDirection(
                targetEntityId,
                targetPosition,
                _chaseConfig.StoppingDistance,
                _chaseConfig.SeparationDistance,
                _chaseConfig.SeparationWeight);

            float approachArrivalDistance = _chaseConfig.ApproachArrivalDistance;

            if ((position - _steeringService.ApproachPosition).sqrMagnitude <=
                approachArrivalDistance * approachArrivalDistance)
            {
                _inputSource.SetInput(default);
                return EnemyBehaviourStateType.Chasing;
            }

            _inputSource.SetInput(new EnemyInputData(direction.normalized));
            return EnemyBehaviourStateType.Chasing;
        }

        private NetworkEntityIdData ReassessTarget(
            Vector2 position,
            Vector2 currentTargetPosition,
            NetworkEntityIdData currentTargetEntityId)
        {
            if (!_targetingService.TryFindNearestTarget(position, out NetworkEntityIdData candidateEntityId) ||
                candidateEntityId == currentTargetEntityId ||
                !_targetingService.TryGetTargetPosition(candidateEntityId, out Vector2 candidatePosition))
            {
                ResetRetargeting();
                return currentTargetEntityId;
            }

            float currentDistance = Vector2.Distance(position, currentTargetPosition);
            float candidateDistance = Vector2.Distance(position, candidatePosition);

            if (currentDistance - candidateDistance < _chaseConfig.RetargetDistanceAdvantage)
            {
                ResetRetargeting();
                return currentTargetEntityId;
            }

            if (_retargetCandidateId != candidateEntityId)
            {
                _retargetCandidateId = candidateEntityId;
                _retargetElapsedTime = 0f;
            }

            _retargetElapsedTime += _networkBehaviourAccessor.ParentNetworkBehaviour.Runner.DeltaTime;

            if (_retargetElapsedTime < _chaseConfig.RetargetDelay)
            {
                return currentTargetEntityId;
            }

            ResetRetargeting();
            return candidateEntityId;
        }

        private void ResetRetargeting()
        {
            _retargetCandidateId = NetworkEntityIdData.None;
            _retargetElapsedTime = 0f;
        }
    }
}
