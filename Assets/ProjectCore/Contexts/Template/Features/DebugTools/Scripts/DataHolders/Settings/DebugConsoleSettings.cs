using System;
using UnityEngine;

namespace ProjectCore.Template
{
    [Serializable]
    public sealed class DebugConsoleSettings
    {
        [UnityEngine.Serialization.FormerlySerializedAs("_openOnStart")]
        [SerializeField] private bool _shouldOpenOnStart = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        [SerializeField, Min(1)] private int _maxLogs = 200;

        public bool ShouldOpenOnStart => _shouldOpenOnStart;
        public KeyCode ToggleKey => _toggleKey;
        public int MaxLogs => _maxLogs;
    }
}
