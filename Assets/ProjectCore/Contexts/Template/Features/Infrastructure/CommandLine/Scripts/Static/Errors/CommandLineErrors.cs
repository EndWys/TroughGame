using Domain;

namespace ProjectCore.Template
{
    public static class CommandLineErrors
    {
        public static Error InvalidArgument(string argument)
        {
            return new Error(
                "CommandLine.InvalidArgument",
                $"'{argument}' is not a valid command-line argument key.");
        }
    }
}
