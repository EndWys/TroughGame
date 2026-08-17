using System;
using System.Collections.Generic;
using System.Globalization;
using Domain;

namespace ProjectCore.Template
{
    public sealed class CheatArgumentConverterRegistry : ICheatArgumentConverterRegistry
    {
        private readonly Dictionary<Type, ConverterRegistration> _converters = new();

        public Result<object> Convert(string rawValue, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            Type nullableType = Nullable.GetUnderlyingType(targetType);
            Type conversionType = nullableType ?? targetType;

            try
            {
                if (nullableType != null && string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase))
                    return Result.Success<object>(null);
                if (_converters.TryGetValue(conversionType, out ConverterRegistration registration))
                    return registration.Convert(rawValue);
                if (conversionType == typeof(string))
                    return Result.Success<object>(rawValue);
                if (conversionType == typeof(bool))
                    return ConvertBoolean(rawValue);
                if (conversionType == typeof(char))
                    return ConvertCharacter(rawValue);
                if (conversionType == typeof(Guid))
                    return Result.Success<object>(Guid.Parse(rawValue));
                if (conversionType.IsEnum)
                    return Result.Success(Enum.Parse(conversionType, rawValue, true));
                if (typeof(IConvertible).IsAssignableFrom(conversionType))
                {
                    return Result.Success(
                        System.Convert.ChangeType(rawValue, conversionType, CultureInfo.InvariantCulture));
                }
            }
            catch (Exception)
            {
                return Result.Failure<object>(
                    CheatErrors.ArgumentConversionFailed(string.Empty, targetType, rawValue));
            }

            return Result.Failure<object>(
                CheatErrors.ArgumentConversionFailed(string.Empty, targetType, rawValue));
        }

        public Result<IDisposable> Register<TValue>(
            Func<string, Result<TValue>> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            Type valueType = typeof(TValue);
            if (_converters.ContainsKey(valueType))
            {
                return Result.Failure<IDisposable>(
                    CheatErrors.DuplicateConverter(valueType));
            }

            var registration = new ConverterRegistration(
                this,
                valueType,
                rawValue => ConvertCustom(rawValue, converter));
            _converters.Add(valueType, registration);
            return Result.Success<IDisposable>(registration);
        }

        private void Unregister(ConverterRegistration registration)
        {
            if (_converters.TryGetValue(registration.ValueType, out ConverterRegistration current)
                && ReferenceEquals(current, registration))
            {
                _converters.Remove(registration.ValueType);
            }
        }

        private static Result<object> ConvertCustom<TValue>(
            string rawValue,
            Func<string, Result<TValue>> converter)
        {
            Result<TValue> result = converter(rawValue);
            return result.IsSuccess
                ? Result.Success<object>(result.Value)
                : Result.Failure<object>(result.Errors);
        }

        private static Result<object> ConvertBoolean(string rawValue)
        {
            if (bool.TryParse(rawValue, out bool value))
                return Result.Success<object>(value);

            if (string.Equals(rawValue, "1", StringComparison.Ordinal)
                || string.Equals(rawValue, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "on", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Success<object>(true);
            }

            if (string.Equals(rawValue, "0", StringComparison.Ordinal)
                || string.Equals(rawValue, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "off", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Success<object>(false);
            }

            return Result.Failure<object>(
                CheatErrors.ArgumentConversionFailed(string.Empty, typeof(bool), rawValue));
        }

        private static Result<object> ConvertCharacter(string rawValue)
        {
            return rawValue?.Length == 1
                ? Result.Success<object>(rawValue[0])
                : Result.Failure<object>(
                    CheatErrors.ArgumentConversionFailed(string.Empty, typeof(char), rawValue));
        }

        private sealed class ConverterRegistration : IDisposable
        {
            private CheatArgumentConverterRegistry _owner;
            private readonly Func<string, Result<object>> _converter;

            public ConverterRegistration(
                CheatArgumentConverterRegistry owner,
                Type valueType,
                Func<string, Result<object>> converter)
            {
                _owner = owner;
                ValueType = valueType;
                _converter = converter;
            }

            public Type ValueType { get; }

            public Result<object> Convert(string rawValue)
            {
                return _converter(rawValue);
            }

            public void Dispose()
            {
                CheatArgumentConverterRegistry owner = _owner;
                if (owner == null)
                    return;

                _owner = null;
                owner.Unregister(this);
            }
        }
    }
}
