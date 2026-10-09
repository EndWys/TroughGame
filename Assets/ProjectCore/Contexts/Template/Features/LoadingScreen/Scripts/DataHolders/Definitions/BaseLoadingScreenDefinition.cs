using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public abstract class BaseLoadingScreenDefinition : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _layout;

        public VisualTreeAsset Layout => _layout;
        public abstract Type ScreenType { get; }
        public abstract Type SettingsType { get; }
    }
}
