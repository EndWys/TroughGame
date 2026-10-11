using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public abstract class BasePopupDefinition : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _layout;

        public VisualTreeAsset Layout => _layout;
        public abstract Type PopupType { get; }
        public abstract Type PayloadType { get; }
        public abstract Type ResponseType { get; }
    }
}
