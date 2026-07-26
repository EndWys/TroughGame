using Domain;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public sealed class ResultTests
    {
        [Test]
        public void SuccessCreatesResultWithoutErrors()
        {
            Result result = Result.Success();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.IsFailure);
            Assert.AreEqual(Error.None, result.FirstError);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [Test]
        public void FailureCreatesResultWithErrors()
        {
            var firstError = new Error("Test.First", "First failure.");
            var secondError = new Error("Test.Second", "Second failure.");

            Result result = Result.Failure(new[] { firstError, secondError });

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(firstError, result.FirstError);
            CollectionAssert.AreEqual(new[] { firstError, secondError }, (ICollection<Error>)result.Errors);
        }

        [Test]
        public void GenericFailureDoesNotExposeDefaultValue()
        {
            Result<string> result = Result.Failure<string>(
                new Error("Test.Failed", "Generic result failed."));

            Assert.IsTrue(result.IsFailure);
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = result.Value;
            });
        }

        [Test]
        public void CreateConvertsNullToFailure()
        {
            Result<object> result = Result.Create<object>(null);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(Error.NullValue, result.FirstError);
        }

        [Test]
        public void FailureRequiresRealError()
        {
            Assert.Throws<InvalidOperationException>(() => Result.Failure(Error.None));
            Assert.Throws<InvalidOperationException>(() => Result.Failure(default(Error)));
            Assert.Throws<InvalidOperationException>(() => Result.Failure(Array.Empty<Error>()));
        }

        [Test]
        public void ErrorFromExceptionPreservesDiagnosticStackTrace()
        {
            var exception = new InvalidOperationException("Test exception.");

            Error error = Error.FromException(exception);

            Assert.AreEqual(typeof(InvalidOperationException).FullName, error.Code);
            Assert.AreEqual(exception.Message, error.Message);
            StringAssert.Contains(exception.ToString(), error.ExceptionDetails);
        }
    }
}
