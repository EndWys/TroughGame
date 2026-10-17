using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class CombatContactProcessor
    {
        private readonly HashSet<NetworkEntityIdData> _targets = new();
        private NetworkEntityIdData _source;
        private uint _executionId;

        public void Process(NetworkEntityIdData source, uint executionId, int tick,
            IReadOnlyList<NetworkEntityIdData> candidates, ICollection<CombatContactData> contacts)
        {
            if (!source.IsValid || executionId == 0)
                throw new ArgumentException("Contacts require a valid source and execution.");
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            if (contacts == null) throw new ArgumentNullException(nameof(contacts));
            if (_source != source || _executionId != executionId)
            {
                Reset();
                _source = source;
                _executionId = executionId;
            }
            foreach (NetworkEntityIdData target in candidates)
            {
                if (target.IsValid && target != source && _targets.Add(target))
                    contacts.Add(new CombatContactData(source, target, executionId, tick));
            }
        }

        public void Reset()
        {
            _targets.Clear();
            _source = default;
            _executionId = 0;
        }
    }
}
