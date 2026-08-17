using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Domain;
using NUnit.Framework;

namespace ProjectCore.Template
{
    public sealed class CheatServiceTests
    {
        [Test]
        public void RegistrationValidatesConflictsAndFollowsTokenLifetime()
        {
            var debugTools = new DebugToolsServiceStub();
            var service = new CheatService(debugTools, new CheatArgumentConverterRegistry());
            service.Initialize();

            Result<IDisposable> firstRegistration = service.Register(new ValueCheatHandler());
            Result<IDisposable> duplicateRegistration = service.Register(new DuplicateCheatHandler());

            Assert.That(firstRegistration.IsSuccess, Is.True);
            Assert.That(service.Commands.Select(command => command.Name), Contains.Item("set_value"));
            Assert.That(duplicateRegistration.IsFailure, Is.True);

            firstRegistration.Value.Dispose();

            Assert.That(service.Commands, Is.Empty);
            service.Dispose();
        }

        [Test]
        public async Task ParserConversionOptionalArgumentsAndAsyncResultWorkTogether()
        {
            var handler = new ValueCheatHandler();
            var service = new CheatService(
                new DebugToolsServiceStub(),
                new CheatArgumentConverterRegistry());
            service.Initialize();
            service.Register(handler);

            Result<CheatExecutionData> result = await service.ExecuteAsync(
                "set_value \"Player One\" 12 true",
                CancellationToken.None);
            Result<CheatExecutionData> optionalResult = await service.ExecuteAsync(
                "set_value \"Player Two\"",
                CancellationToken.None);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Value, Is.EqualTo(12));
            Assert.That(handler.Name, Is.EqualTo("Player Two"));
            Assert.That(handler.Value, Is.EqualTo(5));
            Assert.That(handler.IsEnabled, Is.False);
            Assert.That(optionalResult.IsSuccess, Is.True);
            service.Dispose();
        }

        [Test]
        public async Task ExecutionReportsHandlerFailureAndHonorsAvailabilityAndCancellation()
        {
            var debugTools = new DebugToolsServiceStub();
            var service = new CheatService(debugTools, new CheatArgumentConverterRegistry());
            service.Initialize();
            service.Register(new FailureCheatHandler());

            Result<CheatExecutionData> failedResult = await service.ExecuteAsync(
                "fail",
                CancellationToken.None);
            debugTools.SetEnabled(false);
            Result<CheatExecutionData> disabledResult = await service.ExecuteAsync(
                "fail",
                CancellationToken.None);
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            Assert.That(failedResult.IsFailure, Is.True);
            Assert.That(failedResult.FirstError.Code, Is.EqualTo("Cheat.ExecutionFailed"));
            Assert.That(disabledResult.FirstError.Code, Is.EqualTo("Cheat.Disabled"));
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await service.ExecuteAsync("fail", cancellationTokenSource.Token));
            service.Dispose();
            cancellationTokenSource.Dispose();
        }

        private sealed class ValueCheatHandler : ICheatHandler
        {
            public string Name { get; private set; }
            public int Value { get; private set; }
            public bool IsEnabled { get; private set; }

            [CheatCommand("set_value", "Tests")]
            private UniTask<Result<int>> SetValueAsync(
                [CheatArgument("name")] string name,
                [CheatArgument("value")] int value = 5,
                [CheatArgument("enabled")] bool isEnabled = false)
            {
                Name = name;
                Value = value;
                IsEnabled = isEnabled;
                return UniTask.FromResult(Result.Success(value));
            }
        }

        private sealed class DuplicateCheatHandler : ICheatHandler
        {
            [CheatCommand("SET_VALUE")]
            private void SetValue()
            {
            }
        }

        private sealed class FailureCheatHandler : ICheatHandler
        {
            [CheatCommand("fail")]
            private void Fail()
            {
                throw new InvalidOperationException("Expected test failure.");
            }
        }

        private sealed class DebugToolsServiceStub : IDebugToolsService
        {
            private bool _isEnabled = true;

            public event Action<DebugToolTypes, bool> ToolStateChanged;

            public DebugConsoleSettings ConsoleSettings => null;

            public bool IsAllowed(DebugToolTypes tool)
            {
                return tool == DebugToolTypes.Cheats;
            }

            public bool IsEnabled(DebugToolTypes tool)
            {
                return IsAllowed(tool) && _isEnabled;
            }

            public Result Enable(DebugToolTypes tool)
            {
                SetEnabled(true);
                return Result.Success();
            }

            public Result Disable(DebugToolTypes tool)
            {
                SetEnabled(false);
                return Result.Success();
            }

            public void SetEnabled(bool isEnabled)
            {
                _isEnabled = isEnabled;
                ToolStateChanged?.Invoke(DebugToolTypes.Cheats, isEnabled);
            }
        }
    }
}
