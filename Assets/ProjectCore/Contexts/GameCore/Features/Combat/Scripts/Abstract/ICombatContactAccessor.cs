using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public interface ICombatContactAccessor
    {
        int ContactTick { get; }
        IReadOnlyList<CombatContactData> Contacts { get; }
    }
}
