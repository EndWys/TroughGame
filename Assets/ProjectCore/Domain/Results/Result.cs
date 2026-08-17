using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Domain
{
    public class Result
    {
        private static readonly IReadOnlyList<Error> EmptyErrors = Array.AsReadOnly(Array.Empty<Error>());

        private readonly IReadOnlyList<Error> _errors;

        protected Result(bool isSuccess, IEnumerable<Error> errors)
        {
            IsSuccess = isSuccess;
            _errors = NormalizeErrors(isSuccess, errors);
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error FirstError => _errors.Count > 0 ? _errors[0] : Error.None;

        public IReadOnlyList<Error> Errors => _errors;

        public static Result Success()
        {
            return new Result(true, EmptyErrors);
        }

        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new Result<TValue>(value, true, EmptyErrors);
        }

        public static Result Failure(Error error)
        {
            return new Result(false, new[] { error });
        }

        public static Result Failure(IEnumerable<Error> errors)
        {
            return new Result(false, errors);
        }

        public static Result<TValue> Failure<TValue>(Error error)
        {
            return new Result<TValue>(default, false, new[] { error });
        }

        public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors)
        {
            return new Result<TValue>(default, false, errors);
        }

        public static Result<TValue> Create<TValue>(TValue value)
        {
            return value == null ? Failure<TValue>(Error.NullValue) : Success(value);
        }

        private static IReadOnlyList<Error> NormalizeErrors(bool isSuccess, IEnumerable<Error> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (isSuccess)
            {
                if (errors.Any(error => !error.IsNone))
                {
                    throw new InvalidOperationException("A successful result cannot contain errors.");
                }

                return EmptyErrors;
            }

            Error[] failureErrors = errors
                .Where(error => !error.IsNone)
                .ToArray();

            if (failureErrors.Length == 0)
            {
                throw new InvalidOperationException("A failed result must contain at least one error.");
            }

            return new ReadOnlyCollection<Error>(failureErrors);
        }
    }
}
