using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerFacingDirectionProcessorTests
    {
        [Test]
        public void ProcessorConvertsDirectionalPayloadToFacingDirection()
        {
            FacingDirectionMutatorStub mutator = new(PlayerFacingDirection.SouthEast);
            FourDirectionalFacingProcessor<PlayerMovementState, PlayerMovementPayload> processor =
                new(mutator);

            bool hasTransition = processor.Execute(
                new PlayerMovementPayload(new Vector2(-1f, 1f)),
                out PlayerMovementState resultState);

            Assert.That(hasTransition, Is.False);
            Assert.That(resultState, Is.EqualTo(default(PlayerMovementState)));
            Assert.That(mutator.FacingDirection, Is.EqualTo(PlayerFacingDirection.NorthWest));
        }

        [Test]
        public void ProcessorPreservesFacingAxisMissingFromInput()
        {
            FacingDirectionMutatorStub mutator = new(PlayerFacingDirection.SouthWest);
            FourDirectionalFacingProcessor<PlayerMovementState, PlayerMovementPayload> processor =
                new(mutator);

            processor.Execute(new PlayerMovementPayload(Vector2.up), out _);

            Assert.That(mutator.FacingDirection, Is.EqualTo(PlayerFacingDirection.NorthWest));
        }

        [Test]
        public void ProcessorIgnoresDirectionBelowAxisThreshold()
        {
            FacingDirectionMutatorStub mutator = new(PlayerFacingDirection.SouthWest);
            FourDirectionalFacingProcessor<PlayerMovementState, PlayerMovementPayload> processor =
                new(mutator);

            processor.Execute(new PlayerMovementPayload(new Vector2(0.001f, 0.001f)), out _);

            Assert.That(mutator.FacingDirection, Is.EqualTo(PlayerFacingDirection.SouthWest));
            Assert.That(mutator.SetCalls, Is.Zero);
        }

        private sealed class FacingDirectionMutatorStub : IPlayerFacingDirectionMutator
        {
            public FacingDirectionMutatorStub(PlayerFacingDirection facingDirection)
            {
                FacingDirection = facingDirection;
            }

            public PlayerFacingDirection FacingDirection { get; private set; }
            public int SetCalls { get; private set; }

            public void SetFacingDirection(PlayerFacingDirection facingDirection)
            {
                FacingDirection = facingDirection;
                SetCalls++;
            }
        }
    }
}
