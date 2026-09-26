using System;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourWaitingState : EnemyBehaviourState
    {
        private EnemyInputSourceComponent _inputSource;
        private IEnemyTargetAccessor _targetAccessor;
        private IEnemyTargetMutator _targetMutator;
        private EnemyTargetingService _targetingService;

        [Inject]
        private void Construct(
            EnemyInputSourceComponent inputSource,
            IEnemyTargetAccessor targetAccessor,
            IEnemyTargetMutator targetMutator,
            EnemyTargetingService targetingService)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            _targetAccessor = targetAccessor ?? throw new ArgumentNullException(nameof(targetAccessor));
            _targetMutator = targetMutator ?? throw new ArgumentNullException(nameof(targetMutator));
            _targetingService = targetingService ?? throw new ArgumentNullException(nameof(targetingService));
        }

        public override EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload)
        {
            _inputSource.SetInput(default);

            if (_targetingService.IsTargetAvailable(_targetAccessor.TargetEntityId))
            {
                return EnemyBehaviourStateType.Waiting;
            }

            _targetMutator.ClearTargetEntityId();
            return _targetingService.HasAvailableTarget()
                ? EnemyBehaviourStateType.TargetSelection
                : EnemyBehaviourStateType.Waiting;
        }
    }
}
