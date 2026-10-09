using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public abstract class BaseScreenDefinition : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _layout;

        public VisualTreeAsset Layout => _layout;
        public abstract Type ScreenType { get; }
        public abstract Type SettingsType { get; }
        public virtual Type ParentScreenType => null;
    }
}
