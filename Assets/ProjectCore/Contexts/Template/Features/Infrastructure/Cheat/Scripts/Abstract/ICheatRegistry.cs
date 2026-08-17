using System;
using Domain;

namespace ProjectCore.Template
{
    public interface ICheatRegistry
    {
        public Result<IDisposable> Register(ICheatHandler handler);
    }
}
