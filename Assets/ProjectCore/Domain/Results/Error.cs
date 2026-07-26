using System;

namespace Domain
{
    public readonly struct Error : IEquatable<Error>
    {
        public static readonly Error None = new Error(string.Empty, string.Empty);
        public static readonly Error NullValue = new Error(
            "Error.NullValue",
            "The specified result value is null.");

        public Error(string code, string message, string exceptionDetails = null)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            ExceptionDetails = exceptionDetails;
        }

        public string Code { get; }

        public string Message { get; }

        public string ExceptionDetails { get; }

        public bool IsNone => string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(Message);

        public static Error FromException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return new Error(
                exception.GetType().FullName ?? exception.GetType().Name,
                exception.Message,
                exception.ToString());
        }

        public bool Equals(Error other)
        {
            return Code == other.Code && Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            return obj is Error other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Message);
        }

        public override string ToString()
        {
            return Message;
        }

        public static bool operator ==(Error left, Error right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Error left, Error right)
        {
            return !left.Equals(right);
        }
    }
}
