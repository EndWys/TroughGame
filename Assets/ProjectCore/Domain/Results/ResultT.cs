using System.Collections.Generic;

namespace Domain
{
    public sealed class Result<TValue> : Result
    {
        private readonly TValue _value;

        internal Result(TValue value, bool isSuccess, IEnumerable<Error> errors)
            : base(isSuccess, errors)
        {
            _value = value;
        }

        public TValue Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new System.InvalidOperationException(
                        "Cannot access the value of a failed result.");
                }

                return _value;
            }
        }

        public static implicit operator Result<TValue>(TValue value)
        {
            return Create(value);
        }
    }
}
