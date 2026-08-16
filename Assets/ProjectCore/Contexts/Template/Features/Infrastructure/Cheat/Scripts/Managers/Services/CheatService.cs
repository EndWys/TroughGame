using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public sealed class CheatService : ICheatService, ICheatRegistry, IDisposable
    {
        private static readonly IReadOnlyList<CheatCommandDescriptor> EmptyCommands =
            Array.AsReadOnly(Array.Empty<CheatCommandDescriptor>());
        private static readonly MethodInfo AwaitGenericMethod = typeof(CheatService)
            .GetMethod(nameof(AwaitGenericAsync), BindingFlags.Static | BindingFlags.NonPublic);

        private readonly Dictionary<string, CheatCommandRegistrationData> _commandsByName =
            new Dictionary<string, CheatCommandRegistrationData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<ICheatHandler, CheatRegistration> _handlerRegistrations = new();
        private readonly IDebugToolsService _debugToolsService;
        private readonly ICheatArgumentConverterRegistry _argumentConverterRegistry;

        private IReadOnlyList<CheatCommandDescriptor> _commands = EmptyCommands;
        private bool _isInitialized;
        private bool _isDisposed;

        public CheatService(
            IDebugToolsService debugToolsService,
            ICheatArgumentConverterRegistry argumentConverterRegistry)
        {
            _debugToolsService = debugToolsService;
            _argumentConverterRegistry = argumentConverterRegistry;
        }

        public event Action CommandsChanged;

        public IReadOnlyList<CheatCommandDescriptor> Commands =>
            _debugToolsService.IsEnabled(DebugToolTypes.Cheats)
                ? _commands
                : EmptyCommands;

        public void Initialize()
        {
            ThrowIfDisposed();
            if (_isInitialized)
                return;

            _debugToolsService.ToolStateChanged += OnToolStateChanged;
            _isInitialized = true;
        }

        public Result<IDisposable> Register(ICheatHandler handler)
        {
            ThrowIfDisposed();
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            if (!_debugToolsService.IsAllowed(DebugToolTypes.Cheats))
                return Result.Success<IDisposable>(EmptyRegistration.Instance);
            if (_handlerRegistrations.ContainsKey(handler))
            {
                return Result.Failure<IDisposable>(
                    CheatErrors.HandlerAlreadyRegistered(handler.GetType()));
            }

            Result<IReadOnlyList<CheatCommandRegistrationData>> commandResult =
                CreateCommandRegistrations(handler);
            if (commandResult.IsFailure)
                return Result.Failure<IDisposable>(commandResult.Errors);

            foreach (CheatCommandRegistrationData command in commandResult.Value)
            {
                if (_commandsByName.ContainsKey(command.Descriptor.Name))
                {
                    return Result.Failure<IDisposable>(
                        CheatErrors.DuplicateCommand(command.Descriptor.Name));
                }
            }

            var registration = new CheatRegistration(this, handler, commandResult.Value);
            _handlerRegistrations.Add(handler, registration);
            foreach (CheatCommandRegistrationData command in commandResult.Value)
                _commandsByName.Add(command.Descriptor.Name, command);

            RefreshCommandCache();
            return Result.Success<IDisposable>(registration);
        }

        public async UniTask<Result<CheatExecutionData>> ExecuteAsync(
            string input,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (!_debugToolsService.IsEnabled(DebugToolTypes.Cheats))
                return Result.Failure<CheatExecutionData>(CheatErrors.Disabled());

            Result<CheatCommandData> parseResult = CheatCommandParserUtility.Parse(input);
            if (parseResult.IsFailure)
                return Result.Failure<CheatExecutionData>(parseResult.Errors);

            CheatCommandData commandData = parseResult.Value;
            if (!_commandsByName.TryGetValue(commandData.Name, out CheatCommandRegistrationData command))
            {
                return Result.Failure<CheatExecutionData>(
                    CheatErrors.UnknownCommand(commandData.Name));
            }

            Result<object[]> argumentsResult = CreateInvocationArguments(
                command,
                commandData.Arguments,
                cancellationToken);
            if (argumentsResult.IsFailure)
                return Result.Failure<CheatExecutionData>(argumentsResult.Errors);

            try
            {
                return await InvokeAsync(command, argumentsResult.Value);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TargetInvocationException exception)
            {
                Exception cause = exception.InnerException ?? exception;
                return Result.Failure<CheatExecutionData>(
                    CheatErrors.ExecutionFailed(command.Descriptor.Name, cause));
            }
            catch (Exception exception)
            {
                return Result.Failure<CheatExecutionData>(
                    CheatErrors.ExecutionFailed(command.Descriptor.Name, exception));
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_isInitialized)
                _debugToolsService.ToolStateChanged -= OnToolStateChanged;

            foreach (CheatRegistration registration in _handlerRegistrations.Values)
                registration.Detach();

            _handlerRegistrations.Clear();
            _commandsByName.Clear();
            _commands = EmptyCommands;
            _isInitialized = false;
            _isDisposed = true;
        }

        private void OnToolStateChanged(DebugToolTypes tool, bool enabled)
        {
            if (tool == DebugToolTypes.Cheats)
                CommandsChanged?.Invoke();
        }

        private void Unregister(CheatRegistration registration)
        {
            if (_isDisposed
                || !_handlerRegistrations.TryGetValue(registration.Handler, out CheatRegistration current)
                || !ReferenceEquals(current, registration))
            {
                return;
            }

            _handlerRegistrations.Remove(registration.Handler);
            foreach (CheatCommandRegistrationData command in registration.Commands)
                _commandsByName.Remove(command.Descriptor.Name);

            registration.Detach();
            RefreshCommandCache();
        }

        private void RefreshCommandCache()
        {
            CheatCommandDescriptor[] descriptors = _commandsByName.Values
                .Select(command => command.Descriptor)
                .OrderBy(descriptor => descriptor.GroupName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(descriptor => descriptor.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            _commands = Array.AsReadOnly(descriptors);
            CommandsChanged?.Invoke();
        }

        private Result<IReadOnlyList<CheatCommandRegistrationData>> CreateCommandRegistrations(
            ICheatHandler handler)
        {
            const BindingFlags flags = BindingFlags.Public
                                       | BindingFlags.NonPublic
                                       | BindingFlags.Instance;

            var registrations = new List<CheatCommandRegistrationData>();
            var localNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (MethodInfo method in handler.GetType().GetMethods(flags))
            {
                CheatCommandAttribute attribute =
                    method.GetCustomAttribute<CheatCommandAttribute>(true);
                if (attribute == null)
                    continue;

                Result validationResult = CheatValidation.ValidateMethod(method, attribute);
                if (validationResult.IsFailure)
                {
                    return Result.Failure<IReadOnlyList<CheatCommandRegistrationData>>(
                        validationResult.Errors);
                }

                if (!localNames.Add(attribute.Name))
                {
                    return Result.Failure<IReadOnlyList<CheatCommandRegistrationData>>(
                        CheatErrors.DuplicateCommand(attribute.Name));
                }

                ParameterInfo[] parameters = method.GetParameters();
                registrations.Add(new CheatCommandRegistrationData
                {
                    Handler = handler,
                    Method = method,
                    Parameters = parameters,
                    Descriptor = CreateDescriptor(attribute, parameters)
                });
            }

            return Result.Success<IReadOnlyList<CheatCommandRegistrationData>>(
                registrations.AsReadOnly());
        }

        private Result<object[]> CreateInvocationArguments(
            CheatCommandRegistrationData command,
            IReadOnlyList<string> rawArguments,
            CancellationToken cancellationToken)
        {
            int maximumCount = command.Descriptor.Arguments.Count;
            int minimumCount = command.Descriptor.Arguments.Count(argument => !argument.IsOptional);
            if (rawArguments.Count < minimumCount || rawArguments.Count > maximumCount)
            {
                return Result.Failure<object[]>(CheatErrors.InvalidArgumentCount(
                    command.Descriptor.Name,
                    minimumCount,
                    maximumCount,
                    rawArguments.Count));
            }

            var arguments = new object[command.Parameters.Length];
            int rawArgumentIndex = 0;
            foreach (ParameterInfo parameter in command.Parameters)
            {
                if (parameter.ParameterType == typeof(CancellationToken))
                {
                    arguments[parameter.Position] = cancellationToken;
                    continue;
                }

                if (rawArgumentIndex >= rawArguments.Count)
                {
                    arguments[parameter.Position] = parameter.DefaultValue;
                    continue;
                }

                Result<object> conversionResult = _argumentConverterRegistry.Convert(
                    rawArguments[rawArgumentIndex],
                    parameter.ParameterType);
                if (conversionResult.IsFailure)
                {
                    string argumentName = command.Descriptor.Arguments[rawArgumentIndex].Name;
                    return Result.Failure<object[]>(CheatErrors.ArgumentConversionFailed(
                        argumentName,
                        parameter.ParameterType,
                        rawArguments[rawArgumentIndex]));
                }

                arguments[parameter.Position] = conversionResult.Value;
                rawArgumentIndex++;
            }

            return Result.Success(arguments);
        }

        private static CheatCommandDescriptor CreateDescriptor(
            CheatCommandAttribute attribute,
            IReadOnlyList<ParameterInfo> parameters)
        {
            CheatArgumentDescriptor[] arguments = parameters
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken))
                .Select(parameter => new CheatArgumentDescriptor
                {
                    Name = parameter.GetCustomAttribute<CheatArgumentAttribute>()?.Name
                           ?? parameter.Name,
                    ValueType = parameter.ParameterType,
                    DefaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : null,
                    IsOptional = parameter.HasDefaultValue
                })
                .ToArray();

            return new CheatCommandDescriptor
            {
                Name = attribute.Name,
                GroupName = attribute.GroupName,
                Arguments = Array.AsReadOnly(arguments)
            };
        }

        private static async UniTask<Result<CheatExecutionData>> InvokeAsync(
            CheatCommandRegistrationData command,
            object[] arguments)
        {
            object value = command.Method.Invoke(command.Handler, arguments);
            Type returnType = command.Method.ReturnType;

            if (returnType == typeof(void))
                return Result.Success(new CheatExecutionData(command.Descriptor.Name, null));

            if (returnType == typeof(UniTask))
            {
                await (UniTask)value;
                return Result.Success(new CheatExecutionData(command.Descriptor.Name, null));
            }

            if (returnType.IsGenericType
                && returnType.GetGenericTypeDefinition() == typeof(UniTask<>))
            {
                MethodInfo awaitMethod = AwaitGenericMethod.MakeGenericMethod(
                    returnType.GetGenericArguments()[0]);
                object awaitResult = awaitMethod.Invoke(null, new[] { value, command.Descriptor.Name });
                return await (UniTask<Result<CheatExecutionData>>)awaitResult;
            }

            return NormalizeResult(command.Descriptor.Name, value);
        }

        private static async UniTask<Result<CheatExecutionData>> AwaitGenericAsync<TValue>(
            object task,
            string commandName)
        {
            TValue value = await (UniTask<TValue>)task;
            return NormalizeResult(commandName, value);
        }

        private static Result<CheatExecutionData> NormalizeResult(
            string commandName,
            object value)
        {
            if (value is not Result result)
                return Result.Success(new CheatExecutionData(commandName, value));
            if (result.IsFailure)
                return Result.Failure<CheatExecutionData>(result.Errors);

            Type resultType = value.GetType();
            object resultValue = resultType.IsGenericType
                && resultType.GetGenericTypeDefinition() == typeof(Result<>)
                    ? resultType.GetProperty(nameof(Result<object>.Value))?.GetValue(value)
                    : null;

            return Result.Success(new CheatExecutionData(commandName, resultValue));
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CheatService));
        }

        private sealed class CheatRegistration : IDisposable
        {
            private CheatService _owner;

            public CheatRegistration(
                CheatService owner,
                ICheatHandler handler,
                IReadOnlyList<CheatCommandRegistrationData> commands)
            {
                _owner = owner;
                Handler = handler;
                Commands = commands;
            }

            public ICheatHandler Handler { get; }
            public IReadOnlyList<CheatCommandRegistrationData> Commands { get; }

            public void Dispose()
            {
                CheatService owner = _owner;
                if (owner == null)
                    return;

                _owner = null;
                owner.Unregister(this);
            }

            public void Detach()
            {
                _owner = null;
            }
        }

        private sealed class EmptyRegistration : IDisposable
        {
            public static readonly EmptyRegistration Instance = new EmptyRegistration();

            private EmptyRegistration()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
