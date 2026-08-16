using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectCore.Template
{
    public sealed class CommandLineServiceTests
    {
        [Test]
        public void LifecycleIsIdempotentAndDisposedStateIsTerminal()
        {
            var provider = new CommandLineArgumentsProviderStub(
                "--port", "7777", "--flag");
            var service = new CommandLineService(provider);

            Assert.Throws<InvalidOperationException>(() => service.HasArgument("--port"));

            service.Initialize();
            service.Initialize();

            Assert.That(service.HasArgument("--port"), Is.True);
            Assert.That(service.HasFlag("--flag"), Is.True);
            Assert.That(service.TryGetValue("--port", out string value), Is.True);
            Assert.That(value, Is.EqualTo("7777"));

            service.Dispose();
            service.Dispose();

            Assert.Throws<ObjectDisposedException>(() => service.HasArgument("--port"));
            Assert.Throws<ObjectDisposedException>(service.Initialize);
        }

        private sealed class CommandLineArgumentsProviderStub : ICommandLineArgumentsProvider
        {
            private readonly IReadOnlyList<string> _arguments;

            public CommandLineArgumentsProviderStub(params string[] arguments)
            {
                _arguments = arguments;
            }

            public IReadOnlyList<string> GetArguments()
            {
                return _arguments;
            }
        }
    }
}
