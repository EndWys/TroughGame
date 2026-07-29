using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class LogService : ILogService, IDisposable
    {
        private readonly List<ILogHandler> _handlers = new List<ILogHandler>();
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            Application.logMessageReceived += HandleLog;
            _isInitialized = true;
        }

        public void AddLogHandler(ILogHandler logHandler)
        {
            if (logHandler == null)
            {
                throw new ArgumentNullException(nameof(logHandler));
            }

            if (!_handlers.Contains(logHandler))
            {
                _handlers.Add(logHandler);
            }
        }

        public void RemoveLogHandler(ILogHandler logHandler)
        {
            if (logHandler == null)
            {
                throw new ArgumentNullException(nameof(logHandler));
            }

            _handlers.Remove(logHandler);
        }

        public void Dispose()
        {
            if (!_isInitialized)
            {
                return;
            }

            Application.logMessageReceived -= HandleLog;
            _handlers.Clear();
            _isInitialized = false;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].HandleLog(condition, stackTrace, type);
            }
        }
    }
}
