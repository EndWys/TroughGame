using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;
using UnityEngine;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleSystem : IDebugConsoleSystem, ILogHandler, IDisposable
    {
        private static readonly IReadOnlyList<CheatCommandDescriptor> EmptyCommands =
            Array.AsReadOnly(Array.Empty<CheatCommandDescriptor>());

        private readonly ICheatService _cheatService;
        private readonly ILogService _logService;

        private BoundedBuffer<DebugConsoleLogData> _logs;
        private BoundedBuffer<string> _commandHistory;
        private int _nextLogID;
        private bool _isAvailable;
        private bool _isInitialized;

        public DebugConsoleSystem(ICheatService cheatService, ILogService logService)
        {
            _cheatService = cheatService;
            _logService = logService;
        }

        public event Action LogsChanged;
        public event Action CommandsChanged;

        public IReadOnlyList<DebugConsoleLogData> Logs => _logs;
        public IReadOnlyList<string> CommandHistory => _commandHistory;
        public IReadOnlyList<CheatCommandDescriptor> Commands =>
            _isAvailable ? _cheatService.Commands : EmptyCommands;

        public void Initialize(DebugConsoleSettings settings, bool isAvailable)
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{nameof(DebugConsoleSystem)} is already initialized.");
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            int capacity = Mathf.Max(1, settings.MaxLogs);
            _logs = new BoundedBuffer<DebugConsoleLogData>(capacity);
            _commandHistory = new BoundedBuffer<string>(capacity);
            _isAvailable = isAvailable;
            _isInitialized = true;

            if (!_isAvailable)
                return;

            _logService.AddLogHandler(this);
            _cheatService.CommandsChanged += OnCommandsChanged;
        }

        public async UniTask ExecuteCommandAsync(
            string command,
            CancellationToken cancellationToken)
        {
            if (!_isAvailable || string.IsNullOrWhiteSpace(command))
                return;

            AddCommand(command);

            try
            {
                Result<CheatExecutionData> result = await _cheatService.ExecuteAsync(
                    command,
                    cancellationToken);
                if (result.IsFailure)
                {
                    AddLog(
                        result.FirstError.Message,
                        result.FirstError.ExceptionDetails,
                        LogType.Error);
                    return;
                }

                if (result.Value.Value != null)
                    AddLog(result.Value.Value.ToString(), string.Empty, LogType.Log);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                AddLog(exception.Message, exception.StackTrace, LogType.Exception);
            }
        }

        public void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (_isAvailable)
                AddLog(condition, stackTrace, type);
        }

        public void ClearLogs()
        {
            if (_logs.Count == 0)
                return;

            _logs.Clear();
            LogsChanged?.Invoke();
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            if (_isAvailable)
            {
                _logService.RemoveLogHandler(this);
                _cheatService.CommandsChanged -= OnCommandsChanged;
            }

            _logs.Clear();
            _commandHistory.Clear();
            _isAvailable = false;
            _isInitialized = false;
        }

        private void OnCommandsChanged()
        {
            CommandsChanged?.Invoke();
        }

        private void AddCommand(string command)
        {
            _commandHistory.Add(command);
            AddLog($"> {command}", string.Empty, LogType.Log);
        }

        private void AddLog(string message, string stackTrace, LogType type)
        {
            _logs.Add(new DebugConsoleLogData(
                _nextLogID++,
                message ?? string.Empty,
                stackTrace ?? string.Empty,
                type));
            LogsChanged?.Invoke();
        }
    }
}
