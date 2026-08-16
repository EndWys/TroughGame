using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugToolsServiceTests
    {
        [Test]
        public void RuntimeStateCanChangeOnlyForAllowedTools()
        {
            DebugToolsSettingsConfig settings = ScriptableObject.CreateInstance<DebugToolsSettingsConfig>();

            try
            {
                var service = new DebugToolsService();
                service.Initialize(settings, new CommandLineServiceStub());

                Assert.That(service.IsEnabled(DebugToolTypes.Console), Is.True);
                Assert.That(service.Disable(DebugToolTypes.Console).IsSuccess, Is.True);
                Assert.That(service.IsEnabled(DebugToolTypes.Console), Is.False);
                Assert.That(service.Enable(DebugToolTypes.Console).IsSuccess, Is.True);
                Assert.That(service.IsEnabled(DebugToolTypes.Console), Is.True);
                Assert.That(service.Enable(DebugToolTypes.None).IsFailure, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void CommandLineCanDisableAllDebugTools()
        {
            DebugToolsSettingsConfig settings = ScriptableObject.CreateInstance<DebugToolsSettingsConfig>();

            try
            {
                var service = new DebugToolsService();
                service.Initialize(
                    settings,
                    new CommandLineServiceStub("--disable-debug-tools"));

                Assert.That(service.IsEnabled(DebugToolTypes.Console), Is.False);
                Assert.That(service.IsEnabled(DebugToolTypes.Cheats), Is.False);
                Assert.That(service.IsEnabled(DebugToolTypes.Visualization), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(settings);
            }
        }

        private sealed class CommandLineServiceStub : ICommandLineService
        {
            private readonly HashSet<string> _arguments;

            public CommandLineServiceStub(params string[] arguments)
            {
                _arguments = new HashSet<string>(arguments);
            }

            public event Action CancelRequested
            {
                add { }
                remove { }
            }

            public IReadOnlyList<string> PositionalArguments => Array.Empty<string>();

            public bool HasArgument(string key)
            {
                return _arguments.Contains(key);
            }

            public bool HasFlag(string key)
            {
                return _arguments.Contains(key);
            }

            public bool TryGetValue(string key, out string value)
            {
                value = null;
                return false;
            }

            public IReadOnlyList<string> GetValues(string key)
            {
                return Array.Empty<string>();
            }
        }
    }
}
