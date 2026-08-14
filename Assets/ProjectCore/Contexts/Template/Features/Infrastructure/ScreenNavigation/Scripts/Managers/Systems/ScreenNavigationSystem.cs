using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class ScreenNavigationSystem : IScreenNavigationSystem
    {
        private readonly IClassFactory _classFactory;
        private readonly Dictionary<Type, BaseScreenDefinition> _definitions =
            new Dictionary<Type, BaseScreenDefinition>();
        private readonly List<ScreenHistoryEntryData> _history =
            new List<ScreenHistoryEntryData>();

        private VisualElement _screenRoot;
        private bool _isInitialized;

        public ScreenNavigationSystem(IClassFactory classFactory)
        {
            _classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        }

        public bool CanGoBack => _history.Count > 1;
        public bool IsTransitioning { get; private set; }
        public Type CurrentScreenType => _history.Count == 0
            ? null
            : _history[_history.Count - 1].Definition.ScreenType;

        public void Initialize(ScreenNavigationComponent component, ScreenCatalogConfig screenCatalog)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("ScreenNavigationSystem is already initialized.");
            }

            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            ScreenNavigationValidation.ValidateCatalog(screenCatalog);

            _screenRoot = component.RootVisualElement;

            foreach (BaseScreenDefinition definition in screenCatalog.Definitions)
            {
                _definitions.Add(definition.ScreenType, definition);
            }

            _isInitialized = true;
        }

        public UniTask<Result> OpenRootAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseScreenView<TSettings>
            where TSettings : IScreenSettings
        {
            return OpenRootAsync(typeof(TScreen), settings, cancellationToken);
        }

        public UniTask<Result> OpenChildAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseScreenView<TSettings>
            where TSettings : IScreenSettings
        {
            return OpenChildAsync(typeof(TScreen), settings, cancellationToken);
        }

        public UniTask<Result> NavigateAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseScreenView<TSettings>
            where TSettings : IScreenSettings
        {
            return NavigateAsync(typeof(TScreen), settings, cancellationToken);
        }

        public async UniTask<Result> GoBackAsync(CancellationToken cancellationToken)
        {
            EnsureInitialized();

            if (_history.Count < 2)
            {
                return Result.Failure(ScreenNavigationErrors.CannotGoBack());
            }

            if (IsTransitioning)
            {
                return Result.Failure(ScreenNavigationErrors.TransitionInProgress());
            }

            IsTransitioning = true;

            try
            {
                ScreenHistoryEntryData currentEntry = _history[_history.Count - 1];
                ScreenHistoryEntryData previousEntry = _history[_history.Count - 2];

                await previousEntry.ScreenView.ShowAsync(cancellationToken);
                await currentEntry.ScreenView.HideAsync(cancellationToken);
                await currentEntry.ScreenView.CloseAsync(cancellationToken);

                _history.RemoveAt(_history.Count - 1);
                return Result.Success();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Result.Failure(ScreenNavigationErrors.TransitionFailed(exception));
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        private async UniTask<Result> OpenRootAsync(
            Type screenType,
            IScreenSettings settings,
            CancellationToken cancellationToken)
        {
            Result definitionResult = GetDefinition(screenType, settings, out BaseScreenDefinition definition);
            if (definitionResult.IsFailure)
            {
                return definitionResult;
            }

            Result rootResult = ScreenNavigationValidation.ValidateRoot(definition);
            if (rootResult.IsFailure)
            {
                return rootResult;
            }

            return await TransitToScreenAsync(definition, settings, 0, cancellationToken);
        }

        private async UniTask<Result> OpenChildAsync(
            Type screenType,
            IScreenSettings settings,
            CancellationToken cancellationToken)
        {
            Result definitionResult = GetDefinition(screenType, settings, out BaseScreenDefinition definition);
            if (definitionResult.IsFailure)
            {
                return definitionResult;
            }

            Result parentResult = ScreenNavigationValidation.ValidateParent(
                definition,
                CurrentScreenType);
            if (parentResult.IsFailure)
            {
                return parentResult;
            }

            return await TransitToScreenAsync(
                definition,
                settings,
                _history.Count,
                cancellationToken);
        }

        private async UniTask<Result> NavigateAsync(
            Type screenType,
            IScreenSettings settings,
            CancellationToken cancellationToken)
        {
            Result definitionResult = GetDefinition(screenType, settings, out BaseScreenDefinition definition);
            if (definitionResult.IsFailure)
            {
                return definitionResult;
            }

            if (definition.ParentScreenType == null)
            {
                return await TransitToScreenAsync(definition, settings, 0, cancellationToken);
            }

            int parentIndex = _history.FindLastIndex(entry =>
                entry.Definition.ScreenType == definition.ParentScreenType);
            if (parentIndex < 0)
            {
                return Result.Failure(ScreenNavigationErrors.NavigationParentNotFound(
                    screenType,
                    definition.ParentScreenType));
            }

            return await TransitToScreenAsync(
                definition,
                settings,
                parentIndex + 1,
                cancellationToken);
        }

        private async UniTask<Result> TransitToScreenAsync(
            BaseScreenDefinition definition,
            IScreenSettings settings,
            int retainedHistoryCount,
            CancellationToken cancellationToken)
        {
            if (IsTransitioning)
            {
                return Result.Failure(ScreenNavigationErrors.TransitionInProgress());
            }

            IsTransitioning = true;

            try
            {
                BaseScreenView nextScreenView = CreateScreenView(definition);
                await InitializeScreenAsync(nextScreenView, settings, cancellationToken);

                ScreenHistoryEntryData currentEntry = _history.Count == 0
                    ? null
                    : _history[_history.Count - 1];

                _screenRoot.Add(nextScreenView);
                await nextScreenView.ShowAsync(cancellationToken);

                if (currentEntry != null)
                {
                    await currentEntry.ScreenView.HideAsync(cancellationToken);
                }

                await CloseHistoryEntriesAsync(retainedHistoryCount, cancellationToken);
                RemoveHistoryEntries(retainedHistoryCount);

                ScreenHistoryEntryData nextEntry = CreateHistoryEntry(definition, nextScreenView);
                _history.Add(nextEntry);

                return Result.Success();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Result.Failure(ScreenNavigationErrors.TransitionFailed(exception));
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        private BaseScreenView CreateScreenView(BaseScreenDefinition definition)
        {
            BaseScreenView screenView = _classFactory.Create<BaseScreenView>(definition.ScreenType);
            definition.Layout.CloneTree(screenView);

            return screenView;
        }

        private static UniTask InitializeScreenAsync(
            BaseScreenView screenView,
            IScreenSettings settings,
            CancellationToken cancellationToken)
        {
            return screenView.InitializeAsync(settings, cancellationToken);
        }

        private static ScreenHistoryEntryData CreateHistoryEntry(
            BaseScreenDefinition definition,
            BaseScreenView screenView)
        {
            return new ScreenHistoryEntryData(definition, screenView);
        }

        private async UniTask CloseHistoryEntriesAsync(
            int startIndex,
            CancellationToken cancellationToken)
        {
            for (int index = _history.Count - 1; index >= startIndex; index--)
            {
                await _history[index].ScreenView.CloseAsync(cancellationToken);
            }
        }

        private void RemoveHistoryEntries(int startIndex)
        {
            if (_history.Count > startIndex)
            {
                _history.RemoveRange(startIndex, _history.Count - startIndex);
            }
        }

        private Result GetDefinition(
            Type screenType,
            IScreenSettings settings,
            out BaseScreenDefinition definition)
        {
            EnsureInitialized();

            if (!_definitions.TryGetValue(screenType, out definition))
            {
                return Result.Failure(ScreenNavigationErrors.DefinitionNotRegistered(screenType));
            }

            return ScreenNavigationValidation.ValidateSettings(definition, settings);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "ScreenNavigationSystem must be initialized before use.");
            }
        }
    }
}
