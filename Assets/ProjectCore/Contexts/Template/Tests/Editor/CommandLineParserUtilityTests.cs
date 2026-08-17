using System.Linq;
using NUnit.Framework;

namespace ProjectCore.Template
{
    public sealed class CommandLineParserUtilityTests
    {
        [Test]
        public void ParseSupportsFlagsValuesDuplicatesAndPositionals()
        {
            string[] arguments =
            {
                "--flag",
                "--port", "7777",
                "--name=Test Server",
                "--offset", "-10",
                "--port", "8888",
                "--empty=",
                "positional",
                "--trailing",
                "-batchmode",
                "--",
                "--literal"
            };

            var result = CommandLineParserUtility.Parse(arguments);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Flags, Does.Contain("--flag"));
            Assert.That(result.Value.Flags, Does.Contain("--trailing"));
            Assert.That(result.Value.Values["--port"], Is.EqualTo(new[] { "7777", "8888" }));
            Assert.That(result.Value.Values["--name"].Single(), Is.EqualTo("Test Server"));
            Assert.That(result.Value.Values["--offset"].Single(), Is.EqualTo("-10"));
            Assert.That(result.Value.Values["--empty"].Single(), Is.Empty);
            Assert.That(
                result.Value.PositionalArguments,
                Is.EqualTo(new[] { "positional", "-batchmode", "--literal" }));
        }
    }
}
