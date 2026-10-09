using System;
using Shared;

namespace ProjectCore.Template
{
    public interface ICheatArgumentConverterRegistry
    {
        public Result<object> Convert(string rawValue, Type targetType);
        public Result<IDisposable> Register<TValue>(
            Func<string, Result<TValue>> converter);
    }
}
