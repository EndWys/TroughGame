using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UIElements;

namespace ProjectCore.Template
{
    public sealed class PopupSystem : IPopupSystem, IDisposable
    {
        private readonly IClassFactory _classFactory;
        private readonly Dictionary<Type, BasePopupDefinition> _definitions =
            new Dictionary<Type, BasePopupDefinition>();
        private readonly List<PopupEntryData> _entries =
            new List<PopupEntryData>();

        private PopupComponent _popupComponent;
        private VisualElement _popupHost;
        private bool _isInitialized;
        private bool _isDisposed;

        public PopupSystem(IClassFactory classFactory)
        {
            _classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        }

        public void Initialize(PopupComponent popupComponent, PopupCatalogConfig popupCatalog)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("PopupSystem is already initialized.");
            }

            if (popupComponent == null)
            {
                throw new ArgumentNullException(nameof(popupComponent));
            }

            PopupValidation.ValidateCatalog(popupCatalog);
            popupComponent.Initialize();

            _popupComponent = popupComponent;
            _popupHost = popupComponent.PopupHost;

            foreach (BasePopupDefinition definition in popupCatalog.Definitions)
            {
                _definitions.Add(definition.PopupType, definition);
            }

            _isInitialized = true;
        }

        public async UniTask<Result<TResponse>> OpenAsync<TPopup, TPayload, TResponse>(
            TPayload payload,
            CancellationToken cancellationToken)
            where TPopup : BasePopupView<TPayload, TResponse>
            where TPayload : IPopupPayload
        {
            EnsureAvailable();
            cancellationToken.ThrowIfCancellationRequested();

            Result definitionResult = GetDefinition(
                typeof(TPopup),
                payload,
                typeof(TResponse),
                out BasePopupDefinition definition);
            if (definitionResult.IsFailure)
            {
                return Result.Failure<TResponse>(definitionResult.Errors);
            }

            BasePopupView popupView;

            try
            {
                popupView = _classFactory.Create<BasePopupView>(definition.PopupType);
                definition.Layout.CloneTree(popupView.ContentRoot);
            }
            catch (Exception exception)
            {
                return Result.Failure<TResponse>(
                    PopupErrors.LifecycleFailed(definition.PopupType, exception));
            }

            CancellationTokenSource lifetimeCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            PopupEntryData entry = new PopupEntryData(
                definition,
                popupView,
                cancellationToken,
                lifetimeCancellation);

            popupView.SetCompletionHandler(completion =>
                entry.CompletionSource.TrySetResult(completion));

            RegisterEntry(entry);
            ProcessPopupAsync(entry, payload).Forget();

            PopupCompletionData result = await entry.ResultSource.Task;

            return result.IsSuccess
                ? Result.Success((TResponse)result.Response)
                : Result.Failure<TResponse>(result.Error);
        }

        public async UniTask AbortAsync<TPopup>() where TPopup : BasePopupView
        {
            EnsureAvailable();

            int entryIndex = _entries.FindLastIndex(entry =>
                entry.State != PopupStates.Closed &&
                entry.Definition.PopupType == typeof(TPopup));
            if (entryIndex < 0)
            {
                return;
            }

            PopupEntryData entry = _entries[entryIndex];
            RequestAbort(entry, PopupErrors.Aborted(entry.Definition.PopupType));
            await entry.ResultSource.Task.SuppressCancellationThrow();
        }

        public async UniTask AbortAllAsync()
        {
            EnsureAvailable();

            PopupEntryData[] entries = _entries.ToArray();

            foreach (PopupEntryData entry in entries)
            {
                RequestAbort(entry, PopupErrors.Aborted(entry.Definition.PopupType));
            }

            foreach (PopupEntryData entry in entries)
            {
                await entry.ResultSource.Task.SuppressCancellationThrow();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            PopupEntryData[] entries = _entries.ToArray();

            foreach (PopupEntryData entry in entries)
            {
                AbortImmediately(
                    entry,
                    PopupErrors.ContextDisposed(entry.Definition.PopupType));
            }
        }

        private async UniTask ProcessPopupAsync(
            PopupEntryData entry,
            IPopupPayload payload)
        {
            try
            {
                CancellationToken cancellationToken = entry.LifetimeCancellation.Token;

                await entry.PopupView.InitializeAsync(payload, cancellationToken);

                if (entry.CompletionSource.Task.Status == UniTaskStatus.Pending)
                {
                    await entry.PopupView.ShowAsync(cancellationToken);
                    entry.State = PopupStates.Opened;
                    RefreshInputState();
                }

                PopupCompletionData completion = await entry.CompletionSource.Task
                    .AttachExternalCancellation(cancellationToken);

                entry.State = PopupStates.Closing;
                RefreshInputState();

                await entry.PopupView.HideAsync(cancellationToken);
                await entry.PopupView.CloseAsync(cancellationToken);

                FinalizeEntry(entry, completion);
            }
            catch (OperationCanceledException)
            {
                if (entry.State == PopupStates.Closed)
                {
                    return;
                }

                await CloseImmediatelySafelyAsync(entry);

                if (!entry.AbortError.IsNone)
                {
                    FinalizeEntry(entry, new PopupCompletionData(null, entry.AbortError));
                }
                else if (entry.CallerCancellationToken.IsCancellationRequested)
                {
                    FinalizeCancelledEntry(entry);
                }
                else
                {
                    FinalizeEntry(entry, new PopupCompletionData(
                        null,
                        PopupErrors.Aborted(entry.Definition.PopupType)));
                }
            }
            catch (Exception exception)
            {
                if (entry.State == PopupStates.Closed)
                {
                    return;
                }

                await CloseImmediatelySafelyAsync(entry);
                FinalizeEntry(entry, new PopupCompletionData(
                    null,
                    PopupErrors.LifecycleFailed(entry.Definition.PopupType, exception)));
            }
        }

        private void RegisterEntry(PopupEntryData entry)
        {
            entry.PopupView.SetInteractionEnabled(false);
            _entries.Add(entry);
            _popupComponent.SetInputBlocked(true);
            _popupHost.Add(entry.PopupView);
            RefreshInputState();
        }

        private void RequestAbort(PopupEntryData entry, Error error)
        {
            if (entry.State == PopupStates.Closed)
            {
                return;
            }

            entry.AbortError = error;
            entry.State = PopupStates.Closing;
            entry.PopupView.SetInteractionEnabled(false);
            RefreshInputState();
            entry.LifetimeCancellation.Cancel();
        }

        private void AbortImmediately(PopupEntryData entry, Error error)
        {
            RequestAbort(entry, error);

            if (entry.State == PopupStates.Closed)
            {
                return;
            }

            entry.PopupView.DisposeImmediately();
            FinalizeEntry(entry, new PopupCompletionData(null, error));
        }

        private async UniTask CloseImmediatelySafelyAsync(PopupEntryData entry)
        {
            try
            {
                await entry.PopupView.CloseImmediatelyAsync();
            }
            catch
            {
                entry.PopupView.RemoveFromHierarchy();
            }
        }

        private void FinalizeEntry(PopupEntryData entry, PopupCompletionData completion)
        {
            if (entry.State == PopupStates.Closed)
            {
                return;
            }

            RemoveEntry(entry);
            entry.ResultSource.TrySetResult(completion);
        }

        private void FinalizeCancelledEntry(PopupEntryData entry)
        {
            if (entry.State == PopupStates.Closed)
            {
                return;
            }

            RemoveEntry(entry);
            entry.ResultSource.TrySetCanceled(entry.CallerCancellationToken);
        }

        private void RemoveEntry(PopupEntryData entry)
        {
            _entries.Remove(entry);
            entry.State = PopupStates.Closed;
            entry.LifetimeCancellation.Dispose();
            RefreshInputState();
        }

        private void RefreshInputState()
        {
            if (_isDisposed)
            {
                return;
            }

            _popupComponent.SetInputBlocked(_entries.Count > 0);

            if (_entries.Count == 0)
            {
                return;
            }

            int topEntryIndex = _entries.Count - 1;

            for (int index = 0; index < topEntryIndex; index++)
            {
                _entries[index].PopupView.SetInteractionEnabled(false);
            }

            PopupEntryData topEntry = _entries[topEntryIndex];
            topEntry.PopupView.SetInteractionEnabled(
                topEntry.State == PopupStates.Opened);
        }

        private Result GetDefinition(
            Type popupType,
            IPopupPayload payload,
            Type responseType,
            out BasePopupDefinition definition)
        {
            if (!_definitions.TryGetValue(popupType, out definition))
            {
                return Result.Failure(PopupErrors.DefinitionNotRegistered(popupType));
            }

            return PopupValidation.ValidateOperation(definition, payload, responseType);
        }

        private void EnsureAvailable()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("PopupSystem must be initialized before use.");
            }

            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(PopupSystem));
            }
        }
    }
}
