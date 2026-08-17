using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class ScreenNavigationComponent : MonoBehaviour
    {
        private const string ScreenHostName = "ScreenHost";

        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _screenHost;

        public VisualElement RootVisualElement => _screenHost
            ?? throw new InvalidOperationException(
                "ScreenNavigationComponent must be initialized before use.");

        public void Initialize()
        {
            if (_uiDocument == null)
            {
                throw new InvalidOperationException(
                    "ScreenNavigationComponent requires a UIDocument.");
            }

            _screenHost = _uiDocument.rootVisualElement.Q<VisualElement>(ScreenHostName)
                ?? throw new InvalidOperationException(
                    $"ScreenNavigationComponent requires a {ScreenHostName} element " +
                    "in its UIDocument.");

            StretchToParent(_uiDocument.rootVisualElement);
            StretchToParent(_screenHost);
        }

        private static void StretchToParent(VisualElement visualElement)
        {
            visualElement.style.position = Position.Absolute;
            visualElement.style.flexGrow = 0;
            visualElement.style.width = StyleKeyword.Auto;
            visualElement.style.height = StyleKeyword.Auto;
            visualElement.style.left = 0;
            visualElement.style.right = 0;
            visualElement.style.top = 0;
            visualElement.style.bottom = 0;
        }
    }
}
