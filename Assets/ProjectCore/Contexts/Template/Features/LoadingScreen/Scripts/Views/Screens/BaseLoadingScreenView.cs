using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public abstract class BaseLoadingScreenView : VisualElement, ILoadingScreenView
    {
        private const int TransitionDurationMilliseconds = 180;

        private readonly VisualElement _contentRoot;

        private bool _isClosed;

        protected BaseLoadingScreenView()
        {
            AddToClassList("loading-screen__view");
            StretchToParent(this);
            style.display = DisplayStyle.None;

            _contentRoot = new VisualElement
            {
                name = "ContentRoot"
            };
            _contentRoot.AddToClassList("loading-screen__content-root");
            StretchToParent(_contentRoot);
            Add(_contentRoot);
        }

        internal VisualElement ContentRoot => _contentRoot;

        public async UniTask InitializeAsync(
            ILoadingScreenSettings settings,
            CancellationToken cancellationToken)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            await OnInitializeAsync(settings, cancellationToken);
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            style.display = DisplayStyle.Flex;
            style.opacity = 0f;

            await UniTask.Yield(cancellationToken);

            style.opacity = 1f;
            await UniTask.Delay(
                TransitionDurationMilliseconds,
                cancellationToken: cancellationToken);
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            style.opacity = 0f;

            await UniTask.Delay(
                TransitionDurationMilliseconds,
                cancellationToken: cancellationToken);

            style.display = DisplayStyle.None;
        }

        public async UniTask CloseAsync(CancellationToken cancellationToken)
        {
            if (_isClosed)
            {
                return;
            }

            _isClosed = true;
            await OnCloseAsync(cancellationToken);
            RemoveFromHierarchy();
        }

        internal async UniTask CloseImmediatelyAsync()
        {
            style.display = DisplayStyle.None;
            await CloseAsync(CancellationToken.None);
        }

        internal void DisposeImmediately()
        {
            if (_isClosed)
            {
                return;
            }

            _isClosed = true;
            style.display = DisplayStyle.None;
            RemoveFromHierarchy();
            OnCloseAsync(CancellationToken.None).Forget();
        }

        protected abstract UniTask OnInitializeAsync(
            ILoadingScreenSettings settings,
            CancellationToken cancellationToken);

        protected virtual UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
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
