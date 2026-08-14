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

            _uiDocument.rootVisualElement.pickingMode = PickingMode.Ignore;
            _popupHost.pickingMode = PickingMode.Ignore;
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
    }
}
