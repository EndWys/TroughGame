using System;

namespace ProjectCore.GameCore
{
    public interface ICameraTargetRegistry
    {
        ICameraTarget CurrentTarget { get; }

        event Action<ICameraTarget> TargetChanged;

        void Register(ICameraTarget target);

        void Unregister(ICameraTarget target);
    }
}
