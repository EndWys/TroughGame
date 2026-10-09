using System;
using Shared;

namespace ProjectCore.Template
{
    public interface ICheatRegistry
    {
        public Result<IDisposable> Register(ICheatHandler handler);
    }
}
