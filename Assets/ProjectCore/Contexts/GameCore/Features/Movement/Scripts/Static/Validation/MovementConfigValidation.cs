using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public static class MovementConfigValidation
    {
        public static void ValidateDodge(DodgeMovementConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(
                    nameof(config), MovementConfigErrors.ConfigNotConfigured(nameof(DodgeMovementConfig)).Message);
            }

            ValidateValue(config, nameof(config.DurationSeconds), config.DurationSeconds, isPositive: true);
            ValidateValue(config, nameof(config.Speed), config.Speed, isPositive: false);
            ValidateValue(config, nameof(config.PreparationSeconds), config.PreparationSeconds, isPositive: true);
            ValidateValue(config, nameof(config.RecoverySeconds), config.RecoverySeconds, isPositive: true);
        }

        public static DodgeTimingData CompileDodgeTiming(DodgeMovementConfig config, float tickDelta)
        {
            ValidateDodge(config);
            if (float.IsNaN(tickDelta) || float.IsInfinity(tickDelta) || tickDelta <= 0f)
                throw new ArgumentOutOfRangeException(nameof(tickDelta));
            double preparation = Math.Ceiling((double)config.PreparationSeconds / tickDelta);
            double active = Math.Ceiling((double)config.DurationSeconds / tickDelta);
            double recovery = Math.Ceiling((double)config.RecoverySeconds / tickDelta);
            if (preparation + active + recovery > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(config), "Dodge exceeds the tick range.");
            return new DodgeTimingData((int)preparation, (int)active, (int)recovery);
        }

        public static void ValidateDash(DashMovementConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(
                    nameof(config), MovementConfigErrors.ConfigNotConfigured(nameof(DashMovementConfig)).Message);
            }

            ValidateValue(config, nameof(config.DurationSeconds), config.DurationSeconds, isPositive: true);
            ValidateValue(config, nameof(config.Distance), config.Distance, isPositive: true);
            ValidateValue(config, nameof(config.CooldownSeconds), config.CooldownSeconds, isPositive: false);
            ValidateValue(config, nameof(config.Speed), config.Speed, isPositive: true);
        }

        private static void ValidateValue(ScriptableObject config, string fieldName, float value, bool isPositive)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || (isPositive ? value <= 0f : value < 0f))
            {
                throw new InvalidOperationException(
                    MovementConfigErrors.InvalidValue(
                        config.GetType().Name, config.name, fieldName, isPositive).Message);
            }
        }
    }
}
