using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public interface ICombatTargetQuery
    {
        void FillTargetIds(
            NetworkEntityIdData sourceEntityId,
            in CombatShapeData attackShape,
            ICollection<NetworkEntityIdData> results);
    }
}
