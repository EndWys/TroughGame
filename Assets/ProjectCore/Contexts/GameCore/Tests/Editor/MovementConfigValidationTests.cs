using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCore.GameCore
{
    public sealed class MovementConfigValidationTests
    {
        private DodgeMovementConfig _dodgeConfig;
        private DashMovementConfig _dashConfig;

        [SetUp]
        public void SetUp()
        {
            _dodgeConfig = ScriptableObject.CreateInstance<DodgeMovementConfig>();
            _dodgeConfig.name = "TestDodge";
            _dashConfig = ScriptableObject.CreateInstance<DashMovementConfig>();
            _dashConfig.name = "TestDash";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_dodgeConfig);
            Object.DestroyImmediate(_dashConfig);
        }

        [TestCase("_durationSeconds", 0f, "DurationSeconds")]
        [TestCase("_durationSeconds", -1f, "DurationSeconds")]
        [TestCase("_durationSeconds", float.NaN, "DurationSeconds")]
        [TestCase("_durationSeconds", float.PositiveInfinity, "DurationSeconds")]
        [TestCase("_durationSeconds", float.NegativeInfinity, "DurationSeconds")]
        [TestCase("_speed", -1f, "Speed")]
        [TestCase("_speed", float.NaN, "Speed")]
        [TestCase("_speed", float.PositiveInfinity, "Speed")]
        public void RejectsInvalidDodgeValueWithConfigAndField(string field, float value, string parameter)
        {
            SetValue(_dodgeConfig, field, value);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => MovementConfigValidation.ValidateDodge(_dodgeConfig));

            Assert.That(exception.Message, Does.Contain(_dodgeConfig.name).And.Contain(parameter));
        }

        [TestCase("_durationSeconds", 0f, "DurationSeconds")]
        [TestCase("_durationSeconds", -1f, "DurationSeconds")]
        [TestCase("_durationSeconds", float.NaN, "DurationSeconds")]
        [TestCase("_durationSeconds", float.PositiveInfinity, "DurationSeconds")]
        [TestCase("_durationSeconds", float.NegativeInfinity, "DurationSeconds")]
        [TestCase("_distance", 0f, "Distance")]
        [TestCase("_distance", -1f, "Distance")]
        [TestCase("_distance", float.NaN, "Distance")]
        [TestCase("_distance", float.PositiveInfinity, "Distance")]
        [TestCase("_cooldownSeconds", -1f, "CooldownSeconds")]
        [TestCase("_cooldownSeconds", float.NaN, "CooldownSeconds")]
        [TestCase("_cooldownSeconds", float.PositiveInfinity, "CooldownSeconds")]
        public void RejectsInvalidDashValueWithConfigAndField(string field, float value, string parameter)
        {
            SetValue(_dashConfig, field, value);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => MovementConfigValidation.ValidateDash(_dashConfig));

            Assert.That(exception.Message, Does.Contain(_dashConfig.name).And.Contain(parameter));
        }

        [Test]
        public void RejectsOverflowedDashSpeedWithFiniteDistanceAndDuration()
        {
            SetValue(_dashConfig, "_distance", float.MaxValue);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => MovementConfigValidation.ValidateDash(_dashConfig));

            Assert.That(exception.Message, Does.Contain(_dashConfig.name).And.Contain("Speed"));
        }

        [Test]
        public void AcceptsDefaultsAndAllowedZeroSpeedAndCooldown()
        {
            Assert.DoesNotThrow(() => MovementConfigValidation.ValidateDodge(_dodgeConfig));
            Assert.DoesNotThrow(() => MovementConfigValidation.ValidateDash(_dashConfig));

            SetValue(_dodgeConfig, "_speed", 0f);
            SetValue(_dashConfig, "_cooldownSeconds", 0f);

            Assert.DoesNotThrow(() => MovementConfigValidation.ValidateDodge(_dodgeConfig));
            Assert.DoesNotThrow(() => MovementConfigValidation.ValidateDash(_dashConfig));
        }

        private static void SetValue(ScriptableObject config, string field, float value)
        {
            config.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(config, value);
        }
    }
}
