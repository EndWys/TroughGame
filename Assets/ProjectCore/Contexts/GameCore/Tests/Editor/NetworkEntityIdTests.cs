using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntityIdTests
    {
        [TestCase(NetworkEntityType.Player, 0u, 67_108_864u)]
        [TestCase(NetworkEntityType.Enemy, 17u, 134_217_745u)]
        [TestCase(NetworkEntityType.Player, 67_108_863u, 134_217_727u)]
        [TestCase(NetworkEntityType.Enemy, 67_108_863u, 201_326_591u)]
        public void PackedIdRoundTripsProtocolValues(NetworkEntityType type, uint index, uint expectedValue)
        {
            var id = new NetworkEntityIdData(type, index);
            var restored = new NetworkEntityIdData(expectedValue);

            Assert.That(id.Value, Is.EqualTo(expectedValue));
            Assert.That(restored.Type, Is.EqualTo(type));
            Assert.That(restored.Index, Is.EqualTo(index));
            Assert.That(restored.IsValid, Is.True);
            Assert.That(restored, Is.EqualTo(id));
            Assert.That(restored.GetHashCode(), Is.EqualTo(id.GetHashCode()));
        }

        [Test]
        public void DefaultAndZeroAreTheSameNoneValue()
        {
            NetworkEntityIdData id = default;

            Assert.That(id, Is.EqualTo(NetworkEntityIdData.None));
            Assert.That(new NetworkEntityIdData(0u), Is.EqualTo(id));
            Assert.That(id.Value, Is.Zero);
            Assert.That(id.Type, Is.EqualTo(NetworkEntityType.None));
            Assert.That(id.Index, Is.Zero);
            Assert.That(id.IsValid, Is.False);
            Assert.Throws<ArgumentException>(() =>
                NetworkEntityIdValidation.ValidateAssignedId(id, "entityId"));
        }

        [TestCase(NetworkEntityType.None)]
        [TestCase((NetworkEntityType)3)]
        [TestCase((NetworkEntityType)63)]
        [TestCase((NetworkEntityType)64)]
        [TestCase((NetworkEntityType)255)]
        public void UnsupportedTypesCannotBePackedOrIssued(NetworkEntityType type)
        {
            var factory = new NetworkEntityIdFactory();

            Assert.Throws<ArgumentOutOfRangeException>(() => new NetworkEntityIdData(type, 0u));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create(type));
            Assert.That(factory.Create(NetworkEntityType.Player).Index, Is.Zero);
        }

        [TestCase(67_108_864u)]
        [TestCase(uint.MaxValue)]
        public void OversizedIndexesAreRejectedInsteadOfChangingTypeBits(uint index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new NetworkEntityIdData(NetworkEntityType.Player, index));
        }

        [TestCase(1u)]
        [TestCase(67_108_863u)]
        [TestCase(201_326_592u)]
        [TestCase(uint.MaxValue)]
        public void MalformedPackedValuesAreRejected(uint value)
        {
            Assert.That(NetworkEntityIdValidation.IsValid(value), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => new NetworkEntityIdData(value));
        }

        [Test]
        public void IdContainsOnlyFourBytesAndDecodedIdsUseTheSameRegistryKey()
        {
            var id = new NetworkEntityIdData(NetworkEntityType.Enemy, 17u);
            var values = new Dictionary<NetworkEntityIdData, string> { [id] = "enemy" };

            Assert.That(Marshal.SizeOf<NetworkEntityIdData>(), Is.EqualTo(sizeof(uint)));
            Assert.That(values[new NetworkEntityIdData(id.Value)], Is.EqualTo("enemy"));
            Assert.That(id == new NetworkEntityIdData(id.Value), Is.True);
            Assert.That(id != new NetworkEntityIdData(NetworkEntityType.Player, 17u), Is.True);
            Assert.That(id.ToString(), Is.EqualTo("Enemy:17"));
        }

        [Test]
        public void FactoryIssuesIndependentMonotonicIndexesPerType()
        {
            var factory = new NetworkEntityIdFactory();

            Assert.That(factory.Create(NetworkEntityType.Player).Index, Is.Zero);
            Assert.That(factory.Create(NetworkEntityType.Enemy).Index, Is.Zero);
            Assert.That(factory.Create(NetworkEntityType.Player).Index, Is.EqualTo(1u));
            Assert.That(factory.Create(NetworkEntityType.Enemy).Index, Is.EqualTo(1u));
        }

        [Test]
        public void ExhaustedFactoryDoesNotWrapOrAdvanceAndOtherTypesCanStillSpawn()
        {
            var factory = new NetworkEntityIdFactory();
            var nextIndexes = (Dictionary<NetworkEntityType, uint>)typeof(NetworkEntityIdFactory)
                .GetField("_nextIndexes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(factory);
            nextIndexes[NetworkEntityType.Enemy] = NetworkEntityIdConstants.MaximumIndex;

            NetworkEntityIdData lastId = factory.Create(NetworkEntityType.Enemy);
            Assert.That(lastId.Index, Is.EqualTo(NetworkEntityIdConstants.MaximumIndex));
            for (int attempt = 0; attempt < 2; attempt++)
            {
                Assert.Throws<InvalidOperationException>(() => factory.Create(NetworkEntityType.Enemy));
                Assert.That(nextIndexes[NetworkEntityType.Enemy],
                    Is.EqualTo(NetworkEntityIdConstants.MaximumIndex + 1u));
            }

            Assert.That(factory.Create(NetworkEntityType.Player).Index, Is.Zero);
        }

        [Test]
        public void WellFormedIdDoesNotImplyEntityIsRegistered()
        {
            var id = new NetworkEntityIdData(NetworkEntityType.Enemy, 17u);
            var registry = new NetworkEntityRegistry();

            Assert.That(id.IsValid, Is.True);
            Assert.That(registry.TryGet(id, out _), Is.False);
        }
    }
}
