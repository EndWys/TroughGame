using System;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourDisabledState : EnemyBehaviourState
    {
        private EnemyInputSourceComponent _inputSource;

        [Inject]
        private void Construct(EnemyInputSourceComponent inputSource)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
        }

        public override void Enter()
        {
            _inputSource.SetInput(default);
        }

        public override EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload)
        {
            return EnemyBehaviourStateType.Disabled;
        }
    }
}
