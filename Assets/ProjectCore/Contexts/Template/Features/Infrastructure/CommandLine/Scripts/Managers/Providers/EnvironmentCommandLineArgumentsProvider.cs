using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public sealed class EnvironmentCommandLineArgumentsProvider : ICommandLineArgumentsProvider
    {
        public IReadOnlyList<string> GetArguments()
        {
            string[] environmentArguments = Environment.GetCommandLineArgs();
            if (environmentArguments.Length <= 1)
                return Array.Empty<string>();

            var arguments = new string[environmentArguments.Length - 1];
            Array.Copy(environmentArguments, 1, arguments, 0, arguments.Length);
            return Array.AsReadOnly(arguments);
        }
    }
}
