using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class PopupComponent : MonoBehaviour
    {
        private const string PopupHostName = "PopupHost";
        private const string InputBlockerName = "PopupInputBlocker";

        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _popupHost;
        private VisualElement _inputBlocker;

        public VisualElement PopupHost => _popupHost;

        public void Initialize()
        {
            if (_uiDocument == null)
            {
                throw new InvalidOperationException("PopupComponent requires a UIDocument.");
            }

            _popupHost = _uiDocument.rootVisualElement.Q<VisualElement>(PopupHostName)
                ?? throw new InvalidOperationException(
                    $"PopupComponent requires a {PopupHostName} element in its UIDocument.");
            _inputBlocker = _popupHost.Q<VisualElement>(InputBlockerName)
                ?? throw new InvalidOperationException(
                    $"PopupComponent requires a {InputBlockerName} element in its UIDocument.");

            StretchToParent(_uiDocument.rootVisualElement);
            _uiDocument.rootVisualElement.pickingMode = PickingMode.Ignore;
            _popupHost.pickingMode = PickingMode.Ignore;
            StretchToParent(_popupHost);
            StretchToParent(_inputBlocker);
            SetInputBlocked(false);
        }

        public void SetInputBlocked(bool isBlocked)
        {
            if (_inputBlocker == null)
            {
                throw new InvalidOperationException("PopupComponent must be initialized before use.");
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
