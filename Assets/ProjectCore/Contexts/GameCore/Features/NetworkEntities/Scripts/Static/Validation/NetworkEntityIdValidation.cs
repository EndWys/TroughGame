using System;

namespace ProjectCore.GameCore
{
    public static class NetworkEntityIdValidation
    {
        public static bool IsSupportedType(NetworkEntityType type)
        {
            return type == NetworkEntityType.Player || type == NetworkEntityType.Enemy;
        }

        public static bool IsValid(uint value)
        {
            return IsSupportedType((NetworkEntityType)(value >> NetworkEntityIdConstants.IndexBits));
        }

        public static void ValidateType(NetworkEntityType type)
        {
            if (!IsSupportedType(type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type), NetworkEntityIdErrors.UnsupportedType(type).Message);
            }
        }

        public static void ValidateIndex(uint index)
        {
            if (index > NetworkEntityIdConstants.MaximumIndex)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), NetworkEntityIdErrors.IndexOutOfRange(index).Message);
            }
        }

        public static void ValidateValue(uint value)
        {
            if (value != 0u && !IsValid(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), NetworkEntityIdErrors.InvalidValue(value).Message);
            }
        }

        public static void ValidateAssignedId(NetworkEntityIdData entityId, string parameterName)
        {
            if (!entityId.IsValid)
            {
                throw new ArgumentException(NetworkEntityIdErrors.IdRequired().Message, parameterName);
            }
        }

        public static void ValidateNextIndex(NetworkEntityType type, uint nextIndex)
        {
            if (nextIndex > NetworkEntityIdConstants.MaximumIndex)
            {
                throw new InvalidOperationException(NetworkEntityIdErrors.IndexesExhausted(type).Message);
            }
        }
    }
}
