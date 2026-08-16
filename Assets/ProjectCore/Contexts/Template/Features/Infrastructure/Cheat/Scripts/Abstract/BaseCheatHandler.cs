using System;
using Domain;
using Zenject;

namespace ProjectCore.Template
{
    public abstract class BaseCheatHandler : ICheatHandler, IDisposable
    {
        private ICheatRegistry _cheatRegistry;
        private IDisposable _registration;
        private bool _isDisposed;

        [Inject]
        private void Construct(ICheatRegistry cheatRegistry)
        {
            _cheatRegistry = cheatRegistry;
        }

        public void Initialize()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_registration != null)
                return;
            if (_cheatRegistry == null)
                throw new InvalidOperationException($"{GetType().Name} was not injected.");

            Result<IDisposable> registrationResult = _cheatRegistry.Register(this);
            if (registrationResult.IsFailure)
                throw new InvalidOperationException(registrationResult.FirstError.Message);

            _registration = registrationResult.Value;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _registration?.Dispose();
            _registration = null;
            _isDisposed = true;
        }
    }
}
