using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public abstract class BaseScreenView : VisualElement, IScreenView
    {
        private const int TransitionDurationMilliseconds = 180;

        protected BaseScreenView()
        {
            AddToClassList("screen-navigation__screen");
            style.position = Position.Absolute;
            style.left = 0;
            style.right = 0;
            style.top = 0;
            style.bottom = 0;
        }

        public async UniTask InitializeAsync(
            IScreenSettings settings,
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
            await UniTask.Delay(TransitionDurationMilliseconds, cancellationToken: cancellationToken);
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            style.opacity = 0f;

            await UniTask.Delay(TransitionDurationMilliseconds, cancellationToken: cancellationToken);

            style.display = DisplayStyle.None;
        }

        public async UniTask CloseAsync(CancellationToken cancellationToken)
        {
            await OnCloseAsync(cancellationToken);
            RemoveFromHierarchy();
        }

        protected abstract UniTask OnInitializeAsync(
            IScreenSettings settings,
            CancellationToken cancellationToken);

        protected virtual UniTask OnCloseAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
