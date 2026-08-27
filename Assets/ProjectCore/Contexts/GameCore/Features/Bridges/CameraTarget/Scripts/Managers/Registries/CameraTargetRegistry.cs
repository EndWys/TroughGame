using System;

namespace ProjectCore.GameCore
{
    public sealed class CameraTargetRegistry : ICameraTargetRegistry
    {
        public ICameraTarget CurrentTarget { get; private set; }

        public event Action<ICameraTarget> TargetChanged;

        public void Register(ICameraTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (ReferenceEquals(CurrentTarget, target))
            {
                return;
            }

            CurrentTarget = target;
            TargetChanged?.Invoke(CurrentTarget);
        }

        public void Unregister(ICameraTarget target)
        {
            if (!ReferenceEquals(CurrentTarget, target))
            {
                return;
            }

            CurrentTarget = null;
            TargetChanged?.Invoke(null);
        }
    }
}
