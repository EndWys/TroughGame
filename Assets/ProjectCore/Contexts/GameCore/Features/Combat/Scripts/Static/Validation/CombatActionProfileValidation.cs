using System;

namespace ProjectCore.GameCore
{
    public static class CombatActionProfileValidation
    {
        public static CombatActionTimingData CompileTiming(
            CombatActionProfileConfig config,
            float tickDeltaSeconds)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.ActionId == 0)
            {
                throw new ArgumentException("An action profile must have a nonzero gameplay ID.", nameof(config));
            }

            if (config.Kind != CombatActionKind.Attack && config.Kind != CombatActionKind.Dodge)
            {
                throw new ArgumentException("The action kind is unsupported.", nameof(config));
            }

            ValidatePositive(tickDeltaSeconds, nameof(tickDeltaSeconds));
            ValidatePositive(config.PreparationSeconds, nameof(config.PreparationSeconds));
            ValidatePositive(config.ActiveSeconds, nameof(config.ActiveSeconds));
            ValidatePositive(config.RecoverySeconds, nameof(config.RecoverySeconds));

            float windowStart = config.ComboWindowStartSeconds;
            float windowEnd = config.ComboWindowEndSeconds;
            if (!IsFinite(windowStart) || !IsFinite(windowEnd) ||
                windowStart < 0f || windowEnd < windowStart || windowEnd > config.RecoverySeconds)
            {
                throw new ArgumentException("The combo window must lie within recovery.", nameof(config));
            }

            int preparationTicks = Quantize(config.PreparationSeconds, tickDeltaSeconds);
            int activeTicks = Quantize(config.ActiveSeconds, tickDeltaSeconds);
            int recoveryTicks = Quantize(config.RecoverySeconds, tickDeltaSeconds);
            if ((long)preparationTicks + activeTicks + recoveryTicks > int.MaxValue)
            {
                throw new ArgumentException("The action duration must be below half the tick-counter range.", nameof(config));
            }

            int windowStartTicks = Math.Min(Quantize(windowStart, tickDeltaSeconds), recoveryTicks);
            int windowEndTicks = Math.Min(Quantize(windowEnd, tickDeltaSeconds), recoveryTicks);
            if (windowStart != windowEnd && windowStartTicks == windowEndTicks)
            {
                throw new ArgumentException("The authored combo window collapses at the simulation tick rate.", nameof(config));
            }

            return new CombatActionTimingData(
                preparationTicks,
                activeTicks,
                recoveryTicks,
                windowStartTicks,
                windowEndTicks);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            if (!IsFinite(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName, "A finite positive duration is required.");
            }
        }

        private static int Quantize(float seconds, float tickDeltaSeconds)
        {
            double ticks = Math.Ceiling((double)seconds / tickDeltaSeconds);
            if (ticks > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "The duration exceeds the tick range.");
            }

            return (int)ticks;
        }
    }
}
