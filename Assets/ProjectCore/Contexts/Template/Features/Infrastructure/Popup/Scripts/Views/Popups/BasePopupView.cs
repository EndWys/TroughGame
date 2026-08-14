using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public abstract class BasePopupView : VisualElement, IPopupView
    {
        private const int TransitionDurationMilliseconds = 180;

        private readonly VisualElement _contentRoot;
        private readonly VisualElement _interactionBlocker;

        private Action<PopupCompletionData> _completionHandler;
        private bool _isCompletionRequested;
        private bool _isClosed;

        protected BasePopupView()
        {
            AddToClassList("popup__view");
            style.position = Position.Absolute;
            style.left = 0;
            style.right = 0;
            style.top = 0;
            style.bottom = 0;
            style.display = DisplayStyle.None;
            pickingMode = PickingMode.Ignore;

            VisualElement backdrop = new VisualElement
            {
                name = "Backdrop"
            };
            backdrop.AddToClassList("popup__backdrop");

            _contentRoot = new VisualElement
            {
                name = "ContentRoot",
                pickingMode = PickingMode.Ignore
            };
            _contentRoot.AddToClassList("popup__content-root");

            _interactionBlocker = new VisualElement
            {
                name = "InteractionBlocker"
            };
            _interactionBlocker.AddToClassList("popup__interaction-blocker");

            Add(backdrop);
            Add(_contentRoot);
            Add(_interactionBlocker);
        }

        internal VisualElement ContentRoot => _contentRoot;

        public async UniTask InitializeAsync(
            IPopupPayload payload,
            CancellationToken cancellationToken)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            await OnInitializeAsync(payload, cancellationToken);
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

        internal void SetCompletionHandler(Action<PopupCompletionData> completionHandler)
        {
            _completionHandler = completionHandler
                ?? throw new ArgumentNullException(nameof(completionHandler));
        }

        internal void SetInteractionEnabled(bool isEnabled)
        {
            _interactionBlocker.style.display = isEnabled
                ? DisplayStyle.None
                : DisplayStyle.Flex;
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

        protected void Dismiss()
        {
            RequestCompletion(new PopupCompletionData(
                null,
                PopupErrors.Dismissed(GetType())));
        }

        protected void Fail(Error error)
        {
            if (error.IsNone)
            {
                throw new ArgumentException("Popup failure requires an error.", nameof(error));
            }

            RequestCompletion(new PopupCompletionData(null, error));
        }

        protected abstract UniTask OnInitializeAsync(
            IPopupPayload payload,
            CancellationToken cancellationToken);

        protected virtual UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        private protected void RequestCompletion(PopupCompletionData completion)
        {
            if (_isCompletionRequested)
            {
                return;
            }

            _isCompletionRequested = true;
            SetInteractionEnabled(false);
            _completionHandler?.Invoke(completion);
        }
    }
}
