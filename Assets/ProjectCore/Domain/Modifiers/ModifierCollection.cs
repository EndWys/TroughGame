using System;
using System.Collections.Generic;

namespace Domain
{
    public sealed class ModifierCollection<T> : IModifierCollection<T>
    {
        private readonly List<ModifierRegistration> _registrations = new List<ModifierRegistration>();
        private long _nextRegistrationOrder;

        public IDisposable Add(Func<T, T> modifier, int priority = 0)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            ModifierRegistration registration = new ModifierRegistration(
                this,
                modifier,
                priority,
                _nextRegistrationOrder++);

            _registrations.Add(registration);
            SortRegistrations();
            return registration;
        }

        public void Remove(Func<T, T> modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(nameof(modifier));
            }

            int registrationIndex = _registrations.FindIndex(
                registration => registration.Modifier == modifier);

            if (registrationIndex >= 0)
            {
                _registrations.RemoveAt(registrationIndex);
            }
        }

        public T ApplyTo(T value)
        {
            for (int i = 0; i < _registrations.Count; i++)
            {
                value = _registrations[i].Modifier(value);
            }

            return value;
        }

        private void Remove(ModifierRegistration registration)
        {
            _registrations.Remove(registration);
        }

        private void SortRegistrations()
        {
            _registrations.Sort((left, right) =>
            {
                int priorityComparison = left.Priority.CompareTo(right.Priority);
                return priorityComparison != 0
                    ? priorityComparison
                    : left.RegistrationOrder.CompareTo(right.RegistrationOrder);
            });
        }

        private sealed class ModifierRegistration : IDisposable
        {
            private ModifierCollection<T> _owner;

            public ModifierRegistration(
                ModifierCollection<T> owner,
                Func<T, T> modifier,
                int priority,
                long registrationOrder)
            {
                _owner = owner;
                Modifier = modifier;
                Priority = priority;
                RegistrationOrder = registrationOrder;
            }

            public Func<T, T> Modifier { get; }

            public int Priority { get; }

            public long RegistrationOrder { get; }
            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }

                _owner.Remove(this);
                _owner = null;
            }
        }
    }
}
