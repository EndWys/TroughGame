using System;

namespace ProjectCore.GameCore
{
    public static class CombatActionProfileValidation
    {
        public static ActionTimingData CompileTiming(CombatActionProfileConfig config, float tickDeltaSeconds)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.ActionId == 0)
                throw new ArgumentException("An action profile must have a nonzero gameplay ID.", nameof(config));
            if (config.Kind != CombatActionKind.Attack && config.Kind != CombatActionKind.Dodge)
                throw new ArgumentException("The action kind is unsupported.", nameof(config));

            return ActionTimingValidation.Compile(config.PreparationSeconds, config.ActiveSeconds,
                config.RecoverySeconds, tickDeltaSeconds, config.ComboWindowStartSeconds, config.ComboWindowEndSeconds);
        }
    }
}
