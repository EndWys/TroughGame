using System;

namespace Domain
{
    public interface IModifierCollection<T>
    {
        IDisposable Add(Func<T, T> modifier, int priority = 0);

        void Remove(Func<T, T> modifier);

        T ApplyTo(T value);
    }
}
