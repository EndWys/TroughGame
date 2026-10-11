using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class LoadingScreenComponent : MonoBehaviour
    {
        private const string LoadingScreenHostName = "LoadingScreenHost";
        private const string InputBlockerName = "LoadingScreenInputBlocker";

        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _loadingScreenHost;
        private VisualElement _inputBlocker;

        public VisualElement LoadingScreenHost => _loadingScreenHost;

        public void Initialize()
        {
            if (_uiDocument == null)
            {
                throw new InvalidOperationException(
                    "LoadingScreenComponent requires a UIDocument.");
            }

            _loadingScreenHost = _uiDocument.rootVisualElement
                .Q<VisualElement>(LoadingScreenHostName)
                ?? throw new InvalidOperationException(
                    $"LoadingScreenComponent requires a {LoadingScreenHostName} element " +
                    "in its UIDocument.");
            _inputBlocker = _loadingScreenHost.Q<VisualElement>(InputBlockerName)
                ?? throw new InvalidOperationException(
                    $"LoadingScreenComponent requires a {InputBlockerName} element " +
                    "in its UIDocument.");

            StretchToParent(_uiDocument.rootVisualElement);
            _uiDocument.rootVisualElement.pickingMode = PickingMode.Ignore;
            _loadingScreenHost.pickingMode = PickingMode.Ignore;
            StretchToParent(_loadingScreenHost);
            StretchToParent(_inputBlocker);
            SetInputBlocked(false);
        }

        public void SetInputBlocked(bool isBlocked)
        {
            if (_inputBlocker == null)
            {
                throw new InvalidOperationException(
                    "LoadingScreenComponent must be initialized before use.");
            }

            _inputBlocker.style.display = isBlocked
                ? DisplayStyle.Flex
                : DisplayStyle.None;
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
