using System;
using UnityEngine;

namespace ProjectCore.Template
{
    [Serializable]
    public sealed class DebugConsoleSettings
    {
        [SerializeField] private bool _openOnStart = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        [SerializeField, Min(1)] private int _maxLogs = 200;

        public bool OpenOnStart => _openOnStart;
        public KeyCode ToggleKey => _toggleKey;
        public int MaxLogs => _maxLogs;
    }
}
