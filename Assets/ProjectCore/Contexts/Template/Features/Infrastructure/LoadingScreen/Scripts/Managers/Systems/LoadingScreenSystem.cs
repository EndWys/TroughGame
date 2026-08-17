using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class LoadingScreenSystem : ILoadingScreenSystem, IDisposable
    {
        private readonly IClassFactory _classFactory;
        private readonly CancellationTokenSource _disposeCancellation =
            new CancellationTokenSource();
        private readonly Dictionary<Type, BaseLoadingScreenDefinition> _definitions =
            new Dictionary<Type, BaseLoadingScreenDefinition>();

        private LoadingScreenComponent _loadingScreenComponent;
        private BaseLoadingScreenDefinition _defaultDefinition;
        private VisualElement _loadingScreenHost;
        private LoadingScreenEntryData _currentEntry;
        private bool _isInitialized;
        private bool _isDisposed;

        public LoadingScreenSystem(IClassFactory classFactory)
        {
            _classFactory = classFactory
                ?? throw new ArgumentNullException(nameof(classFactory));
        }

        public bool IsVisible => _currentEntry != null;
        public bool IsTransitioning { get; private set; }

        public void Initialize(
            LoadingScreenComponent loadingScreenComponent,
            LoadingScreenCatalogConfig loadingScreenCatalog)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    "LoadingScreenSystem is already initialized.");
            }

            if (loadingScreenComponent == null)
            {
                throw new ArgumentNullException(nameof(loadingScreenComponent));
            }

            LoadingScreenValidation.ValidateCatalog(loadingScreenCatalog);
            loadingScreenComponent.Initialize();

            _loadingScreenComponent = loadingScreenComponent;
            _loadingScreenHost = loadingScreenComponent.LoadingScreenHost;
            _defaultDefinition = loadingScreenCatalog.DefaultDefinition;

            foreach (BaseLoadingScreenDefinition definition in
                     loadingScreenCatalog.Definitions)
            {
                _definitions.Add(definition.ScreenType, definition);
            }

            _isInitialized = true;
        }

        public UniTask<Result> ShowAsync<TScreen, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScreen : BaseLoadingScreenView<TSettings>
            where TSettings : ILoadingScreenSettings
        {
            EnsureAvailable();

            Result definitionResult = GetDefinition(
                typeof(TScreen),
                settings,
                out BaseLoadingScreenDefinition definition);
            if (definitionResult.IsFailure)
            {
                return UniTask.FromResult(definitionResult);
            }

            return ShowAsync(definition, settings, cancellationToken);
        }

        public UniTask<Result> ShowDefaultAsync(CancellationToken cancellationToken)
        {
            EnsureAvailable();

            return ShowAsync(
                _defaultDefinition,
                EmptyLoadingScreenSettings.Instance,
                cancellationToken);
        }

        public async UniTask<Result> HideAsync(CancellationToken cancellationToken)
        {
            EnsureAvailable();
            cancellationToken.ThrowIfCancellationRequested();

            if (_currentEntry == null)
            {
                return Result.Success();
            }

            if (IsTransitioning)
            {
                return Result.Failure(LoadingScreenErrors.TransitionInProgress());
            }

            IsTransitioning = true;
            LoadingScreenEntryData entry = _currentEntry;

            using CancellationTokenSource linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _disposeCancellation.Token);

            try
            {
                entry.State = LoadingScreenStates.Closing;
                await entry.ScreenView.HideAsync(linkedCancellation.Token);
                await entry.ScreenView.CloseAsync(linkedCancellation.Token);
                RemoveEntry(entry);

                return Result.Success();
            }
            catch (OperationCanceledException) when (
                cancellationToken.IsCancellationRequested)
            {
                await CloseImmediatelySafelyAsync(entry);
                throw;
            }
            catch (OperationCanceledException) when (_isDisposed)
            {
                await CloseImmediatelySafelyAsync(entry);
                return Result.Failure(
                    LoadingScreenErrors.ContextDisposed(entry.Definition.ScreenType));
            }
            catch (Exception exception)
            {
                await CloseImmediatelySafelyAsync(entry);
                return Result.Failure(LoadingScreenErrors.LifecycleFailed(
                    entry.Definition.ScreenType,
                    exception));
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _disposeCancellation.Cancel();

            if (_currentEntry != null)
            {
                LoadingScreenEntryData entry = _currentEntry;
                entry.ScreenView.DisposeImmediately();
                RemoveEntry(entry);
            }

            _disposeCancellation.Dispose();
        }

        private async UniTask<Result> ShowAsync(
            BaseLoadingScreenDefinition definition,
            ILoadingScreenSettings settings,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_currentEntry != null)
            {
                return Result.Failure(LoadingScreenErrors.AlreadyVisible(
                    _currentEntry.Definition.ScreenType));
            }

            if (IsTransitioning)
            {
                return Result.Failure(LoadingScreenErrors.TransitionInProgress());
            }

            IsTransitioning = true;
            LoadingScreenEntryData entry = null;

            using CancellationTokenSource linkedCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    _disposeCancellation.Token);

            try
            {
                BaseLoadingScreenView screenView =
                    _classFactory.Create<BaseLoadingScreenView>(definition.ScreenType);
                definition.Layout.CloneTree(screenView.ContentRoot);

                entry = new LoadingScreenEntryData(definition, screenView);
                RegisterEntry(entry);

                await screenView.InitializeAsync(
                    settings,
                    linkedCancellation.Token);
                await screenView.ShowAsync(linkedCancellation.Token);

                entry.State = LoadingScreenStates.Opened;
                return Result.Success();
            }
            catch (OperationCanceledException) when (
                cancellationToken.IsCancellationRequested)
            {
                await CloseImmediatelySafelyAsync(entry);
                throw;
            }
            catch (OperationCanceledException) when (_isDisposed)
            {
                await CloseImmediatelySafelyAsync(entry);
                return Result.Failure(LoadingScreenErrors.ContextDisposed(
                    definition.ScreenType));
            }
            catch (Exception exception)
            {
                await CloseImmediatelySafelyAsync(entry);
                return Result.Failure(LoadingScreenErrors.LifecycleFailed(
                    definition.ScreenType,
                    exception));
            }
            finally
            {
                IsTransitioning = false;
            }
        }

        private void RegisterEntry(LoadingScreenEntryData entry)
        {
            _currentEntry = entry;
            _loadingScreenComponent.SetInputBlocked(true);
            _loadingScreenHost.Add(entry.ScreenView);
        }

        private async UniTask CloseImmediatelySafelyAsync(
            LoadingScreenEntryData entry)
        {
            if (entry == null || entry.State == LoadingScreenStates.Closed)
            {
                return;
            }

            try
            {
                await entry.ScreenView.CloseImmediatelyAsync();
            }
            catch
            {
                entry.ScreenView.RemoveFromHierarchy();
            }

            RemoveEntry(entry);
        }

        private void RemoveEntry(LoadingScreenEntryData entry)
        {
            if (entry == null || entry.State == LoadingScreenStates.Closed)
            {
                return;
            }

            entry.State = LoadingScreenStates.Closed;

            if (ReferenceEquals(_currentEntry, entry))
            {
                _currentEntry = null;
                _loadingScreenComponent.SetInputBlocked(false);
            }
        }

        private Result GetDefinition(
            Type screenType,
            ILoadingScreenSettings settings,
            out BaseLoadingScreenDefinition definition)
        {
            if (!_definitions.TryGetValue(screenType, out definition))
            {
                return Result.Failure(
                    LoadingScreenErrors.DefinitionNotRegistered(screenType));
            }

            return LoadingScreenValidation.ValidateOperation(definition, settings);
        }

        private void EnsureAvailable()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "LoadingScreenSystem must be initialized before use.");
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(LoadingScreenSystem));
            }
        }
    }
}
