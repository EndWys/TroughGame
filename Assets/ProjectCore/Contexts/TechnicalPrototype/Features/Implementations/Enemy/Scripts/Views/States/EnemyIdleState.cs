using System.Collections.Generic;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyIdleState : BaseMovementState<EnemyMovementState, EnemyMovementPayload>
    {
        public override void Enter() { }

        public override void Exit() { }

        protected override IReadOnlyList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>>
            CreateMovementProcessors()
        {
            return new IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>[]
            {
                new DirectionalLocomotionTransitionProcessor<EnemyMovementState, EnemyMovementPayload>(
                    locomotionMovementState: EnemyMovementState.Locomotion),
            };
        }

        protected override EnemyMovementState FallbackState => EnemyMovementState.Idle;
    }
}
