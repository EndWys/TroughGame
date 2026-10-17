using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class CombatTargetQueryService : ICombatTargetQuery
    {
        private readonly IGameEntityComponentService _gameEntityComponentService;

        public CombatTargetQueryService(IGameEntityComponentService gameEntityComponentService)
        {
            _gameEntityComponentService = gameEntityComponentService ??
                throw new ArgumentNullException(nameof(gameEntityComponentService));
        }

        public void FillTargetIds(
            NetworkEntityIdData sourceEntityId,
            in CombatShapeData attackShape,
            ICollection<NetworkEntityIdData> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            CombatShapeData shape = attackShape;
            var candidates = new List<NetworkEntityIdData>();
            _gameEntityComponentService.FillEntityIds<ICombatHurtbox>(
                hurtbox => hurtbox != null && hurtbox.IsAvailable
                    && CombatGeometryUtility.Overlaps(shape, hurtbox.WorldShape),
                candidates);

            var uniqueIds = new HashSet<NetworkEntityIdData>(results);
            foreach (NetworkEntityIdData entityId in candidates)
            {
                if (entityId.IsValid && entityId != sourceEntityId && uniqueIds.Add(entityId))
                {
                    results.Add(entityId);
                }
            }
        }
    }
}
