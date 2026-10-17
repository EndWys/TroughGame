using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class CombatActionProfileValidationTests
    {
        private CombatActionProfileConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<CombatActionProfileConfig>();
            SetField("_actionId", (ushort)300);
            SetField("_preparationSeconds", 0.25f);
            SetField("_activeSeconds", 0.125f);
            SetField("_recoverySeconds", 0.5f);
            SetField("_comboWindowStartSeconds", 0.125f);
            SetField("_comboWindowEndSeconds", 0.375f);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_config);
        }

        [Test]
        public void CompileUsesCeilingTicksAndIndependentUshortGameplayIds()
        {
            SetField("_actionId", ushort.MaxValue);
            SetField("_preparationSeconds", 0.26f);
            SetField("_activeSeconds", float.Epsilon);
            CombatActionTimingData timing = CombatActionProfileValidation.CompileTiming(_config, 0.125f);

            Assert.That(timing.PreparationTicks, Is.EqualTo(3));
            Assert.That(timing.ActiveTicks, Is.EqualTo(1));
            Assert.That(timing.RecoveryTicks, Is.EqualTo(4));
            Assert.That(timing.ComboWindowStartTicks, Is.EqualTo(1));
            Assert.That(timing.ComboWindowEndTicks, Is.EqualTo(3));
        }

        [TestCase("_preparationSeconds", 0f)]
        [TestCase("_activeSeconds", -1f)]
        [TestCase("_recoverySeconds", float.NaN)]
        [TestCase("_preparationSeconds", float.PositiveInfinity)]
        [TestCase("_activeSeconds", float.NegativeInfinity)]
        public void CompileRejectsNonpositiveOrNonfinitePhases(string field, float seconds)
        {
            SetField(field, seconds);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, 0.125f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void CompileRejectsInvalidTickDelta(float tickDelta)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, tickDelta));
        }

        [TestCase(-0.1f, 0.25f)]
        [TestCase(0.375f, 0.125f)]
        [TestCase(0.125f, 0.75f)]
        [TestCase(float.NaN, 0.25f)]
        [TestCase(0.125f, float.PositiveInfinity)]
        public void CompileRejectsInvalidWindowBounds(float start, float end)
        {
            SetField("_comboWindowStartSeconds", start);
            SetField("_comboWindowEndSeconds", end);
            Assert.Throws<ArgumentException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, 0.125f));
        }

        [Test]
        public void CompileRejectsCollapsedNonemptyWindowButAcceptsAuthoredDisabledWindow()
        {
            SetField("_comboWindowStartSeconds", 0.13f);
            SetField("_comboWindowEndSeconds", 0.14f);
            Assert.Throws<ArgumentException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, 0.125f));

            SetField("_comboWindowEndSeconds", 0.13f);
            CombatActionTimingData timing = CombatActionProfileValidation.CompileTiming(_config, 0.125f);
            Assert.That(timing.ComboWindowStartTicks, Is.EqualTo(2));
            Assert.That(timing.ComboWindowEndTicks, Is.EqualTo(2));
        }

        [Test]
        public void WindowMayReachTheQuantizedRecoveryEnd()
        {
            SetField("_recoverySeconds", 0.51f);
            SetField("_comboWindowStartSeconds", 0.5f);
            SetField("_comboWindowEndSeconds", 0.51f);
            CombatActionTimingData timing = CombatActionProfileValidation.CompileTiming(_config, 0.125f);
            Assert.That(timing.RecoveryTicks, Is.EqualTo(5));
            Assert.That(timing.ComboWindowStartTicks, Is.EqualTo(4));
            Assert.That(timing.ComboWindowEndTicks, Is.EqualTo(5));
        }

        [Test]
        public void CompileRejectsIndividualAndCombinedTickOverflow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, float.Epsilon));

            SetField("_preparationSeconds", 1073741824f);
            SetField("_activeSeconds", 1073741824f);
            Assert.Throws<ArgumentException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, 1f));
        }

        [Test]
        public void CompileRejectsNullZeroIdAndUnknownKind()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CombatActionProfileValidation.CompileTiming(null, 0.125f));
            SetField("_actionId", (ushort)0);
            Assert.Throws<ArgumentException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, 0.125f));
            SetField("_actionId", (ushort)300);
            SetField("_kind", (CombatActionKind)255);
            Assert.Throws<ArgumentException>(() =>
                CombatActionProfileValidation.CompileTiming(_config, 0.125f));
        }

        private void SetField<T>(string name, T value)
        {
            typeof(CombatActionProfileConfig).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_config, value);
        }
    }
}
