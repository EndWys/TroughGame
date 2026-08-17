using System.Collections.Generic;

namespace ProjectCore.Template
{
    public interface ICommandLineArgumentsProvider
    {
        IReadOnlyList<string> GetArguments();
    }
}
