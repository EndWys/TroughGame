using System;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourWaitingState : EnemyBehaviourState
    {
        private EnemyInputSourceComponent _inputSource;
        private IEnemyTargetMutator _targetMutator;
        private EnemyTargetingService _targetingService;

        [Inject]
        private void Construct(
            EnemyInputSourceComponent inputSource,
            IEnemyTargetMutator targetMutator,
            EnemyTargetingService targetingService)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            _targetMutator = targetMutator ?? throw new ArgumentNullException(nameof(targetMutator));
            _targetingService = targetingService ?? throw new ArgumentNullException(nameof(targetingService));
        }

        public override EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload)
        {
            _inputSource.SetInput(default);

            _targetMutator.ClearTargetEntityId();
            return _targetingService.HasAvailableTarget()
                ? EnemyBehaviourStateType.TargetSelection
                : EnemyBehaviourStateType.Waiting;
        }
    }
}
