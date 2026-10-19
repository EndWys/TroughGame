using System;

namespace ProjectCore.GameCore
{
    public static class ActionTimingValidation
    {
        public static ActionTimingData Compile(
            float preparationSeconds,
            float activeSeconds,
            float recoverySeconds,
            float tickDeltaSeconds,
            float recoveryCancelWindowStartSeconds = 0f,
            float recoveryCancelWindowEndSeconds = 0f)
        {
            ValidatePositive(tickDeltaSeconds, nameof(tickDeltaSeconds));
            ValidatePositive(preparationSeconds, nameof(preparationSeconds));
            ValidatePositive(activeSeconds, nameof(activeSeconds));
            ValidatePositive(recoverySeconds, nameof(recoverySeconds));
            if (!IsFinite(recoveryCancelWindowStartSeconds) || !IsFinite(recoveryCancelWindowEndSeconds) ||
                recoveryCancelWindowStartSeconds < 0f ||
                recoveryCancelWindowEndSeconds < recoveryCancelWindowStartSeconds ||
                recoveryCancelWindowEndSeconds > recoverySeconds)
            {
                throw new ArgumentException(ActionTimingErrors.InvalidRecoveryCancelWindow);
            }

            int preparationTicks = Quantize(preparationSeconds, tickDeltaSeconds);
            int activeTicks = Quantize(activeSeconds, tickDeltaSeconds);
            int recoveryTicks = Quantize(recoverySeconds, tickDeltaSeconds);
            int windowStartTicks = Math.Min(Quantize(recoveryCancelWindowStartSeconds, tickDeltaSeconds), recoveryTicks);
            int windowEndTicks = Math.Min(Quantize(recoveryCancelWindowEndSeconds, tickDeltaSeconds), recoveryTicks);
            if (recoveryCancelWindowStartSeconds != recoveryCancelWindowEndSeconds && windowStartTicks == windowEndTicks)
            {
                throw new ArgumentException(ActionTimingErrors.CollapsedRecoveryCancelWindow);
            }

            return new ActionTimingData(preparationTicks, activeTicks, recoveryTicks, windowStartTicks, windowEndTicks);
        }

        public static void ValidateTicks(
            int preparationTicks,
            int activeTicks,
            int recoveryTicks,
            int recoveryCancelWindowStartTicks,
            int recoveryCancelWindowEndTicks)
        {
            if (preparationTicks <= 0) throw new ArgumentOutOfRangeException(nameof(preparationTicks), ActionTimingErrors.PositiveDurationRequired);
            if (activeTicks <= 0) throw new ArgumentOutOfRangeException(nameof(activeTicks), ActionTimingErrors.PositiveDurationRequired);
            if (recoveryTicks <= 0) throw new ArgumentOutOfRangeException(nameof(recoveryTicks), ActionTimingErrors.PositiveDurationRequired);
            if ((long)preparationTicks + activeTicks + recoveryTicks > int.MaxValue)
                throw new ArgumentException(ActionTimingErrors.TotalDurationExceedsTickRange);
            if (recoveryCancelWindowStartTicks < 0 || recoveryCancelWindowEndTicks < recoveryCancelWindowStartTicks ||
                recoveryCancelWindowEndTicks > recoveryTicks)
                throw new ArgumentException(ActionTimingErrors.InvalidRecoveryCancelWindow);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void ValidatePositive(float value, string parameterName)
        {
            if (!IsFinite(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, ActionTimingErrors.PositiveDurationRequired);
        }

        private static int Quantize(float seconds, float tickDeltaSeconds)
        {
            double ticks = Math.Ceiling((double)seconds / tickDeltaSeconds);
            if (ticks > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(seconds), ActionTimingErrors.DurationExceedsTickRange);
            return (int)ticks;
        }
    }
}
