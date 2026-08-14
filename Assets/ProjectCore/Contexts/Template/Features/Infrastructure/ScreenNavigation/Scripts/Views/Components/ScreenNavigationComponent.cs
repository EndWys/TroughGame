using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class ScreenNavigationComponent : MonoBehaviour
    {
        private const string ScreenHostName = "ScreenHost";

        [SerializeField] private UIDocument _uiDocument;

        public VisualElement RootVisualElement
        {
            get
            {
                if (_uiDocument == null)
                {
                    throw new InvalidOperationException(
                        "ScreenNavigationComponent requires a UIDocument.");
                }

                VisualElement screenHost = _uiDocument.rootVisualElement.Q<VisualElement>(ScreenHostName);

                return screenHost ?? throw new InvalidOperationException(
                    $"ScreenNavigationComponent requires a {ScreenHostName} element in its UIDocument.");
            }
        }
    }
}
