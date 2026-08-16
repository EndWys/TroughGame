using System;
using System.Collections.Generic;
using System.Threading;
using Domain;

namespace ProjectCore.Template
{
    public sealed class CommandLineService : ICommandLineService, IDisposable
    {
        private readonly ICommandLineArgumentsProvider _argumentsProvider;

        private CommandLineArgumentsData _arguments;
        private volatile ServiceState _state;

#if UNITY_SERVER
        private SynchronizationContext _mainThreadContext;
        private int _cancelRequestQueued;
#endif

        public CommandLineService(ICommandLineArgumentsProvider argumentsProvider)
        {
            _argumentsProvider = argumentsProvider
                ?? throw new ArgumentNullException(nameof(argumentsProvider));
        }

#if UNITY_SERVER
        public event Action CancelRequested;
#else
        public event Action CancelRequested
        {
            add { }
            remove { }
        }
#endif

        public IReadOnlyList<string> PositionalArguments
        {
            get
            {
                EnsureInitialized();
                return _arguments.PositionalArguments;
            }
        }

        public void Initialize()
        {
            if (_state == ServiceState.Initialized)
                return;

            if (_state == ServiceState.Disposed)
                throw new ObjectDisposedException(nameof(CommandLineService));

            Result<CommandLineArgumentsData> parseResult =
                CommandLineParserUtility.Parse(_argumentsProvider.GetArguments());
            if (parseResult.IsFailure)
                throw new InvalidOperationException(parseResult.FirstError.Message);

            _arguments = parseResult.Value;

#if UNITY_SERVER
            _mainThreadContext = SynchronizationContext.Current
                ?? throw new InvalidOperationException(
                    "CommandLineService must be initialized on the Unity main thread.");
#endif

            _state = ServiceState.Initialized;

#if UNITY_SERVER
            Console.CancelKeyPress += OnCancelKeyPress;
#endif
        }

        public bool HasArgument(string key)
        {
            EnsureInitialized();
            ValidateKey(key);
            return HasFlagInternal(key) || _arguments.Values.ContainsKey(key);
        }

        public bool HasFlag(string key)
        {
            EnsureInitialized();
            ValidateKey(key);
            return HasFlagInternal(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            IReadOnlyList<string> values = GetValues(key);
            if (values.Count == 0)
            {
                value = null;
                return false;
            }

            value = values[values.Count - 1];
            return true;
        }

        public IReadOnlyList<string> GetValues(string key)
        {
            EnsureInitialized();
            ValidateKey(key);

            return _arguments.Values.TryGetValue(key, out IReadOnlyList<string> values)
                ? values
                : Array.Empty<string>();
        }

        public void Dispose()
        {
            if (_state == ServiceState.Disposed)
                return;

            ServiceState previousState = _state;
            _state = ServiceState.Disposed;

#if UNITY_SERVER
            if (previousState == ServiceState.Initialized)
                Console.CancelKeyPress -= OnCancelKeyPress;

            _mainThreadContext = null;
            Interlocked.Exchange(ref _cancelRequestQueued, 0);
#endif

#if UNITY_SERVER
            CancelRequested = null;
#endif
            _arguments = null;
        }

#if UNITY_SERVER
        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            if (_state != ServiceState.Initialized
                || Interlocked.Exchange(ref _cancelRequestQueued, 1) != 0)
            {
                return;
            }

            SynchronizationContext mainThreadContext = _mainThreadContext;
            mainThreadContext?.Post(DispatchCancelRequested, null);
        }

        private void DispatchCancelRequested(object state)
        {
            Interlocked.Exchange(ref _cancelRequestQueued, 0);

            if (_state == ServiceState.Initialized)
                CancelRequested?.Invoke();
        }
#endif

        private bool HasFlagInternal(string key)
        {
            for (var i = 0; i < _arguments.Flags.Count; i++)
            {
                if (_arguments.Flags[i] == key)
                    return true;
            }

            return false;
        }

        private void EnsureInitialized()
        {
            if (_state == ServiceState.Disposed)
                throw new ObjectDisposedException(nameof(CommandLineService));

            if (_state != ServiceState.Initialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(CommandLineService)} has not been initialized.");
            }
        }

        private static void ValidateKey(string key)
        {
            if (!CommandLineValidation.IsValidKey(key))
            {
                throw new ArgumentException(
                    CommandLineErrors.InvalidArgument(key).Message,
                    nameof(key));
            }
        }

        private enum ServiceState
        {
            Created,
            Initialized,
            Disposed
        }
    }
}
