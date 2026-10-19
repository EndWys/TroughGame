using System;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class CombatHurtboxComponent : BaseNetworkEntityComponent, ICombatHurtbox
    {
        [SerializeField] private CombatShapeSettings _shape;

        private IEntityPositionAccessor _origin;

        [Inject]
        private void Construct(IEntityPositionAccessor origin)
        {
            _origin = origin ?? throw new ArgumentNullException(nameof(origin));
        }

        public bool IsAvailable => IsComponentAvailable;

        protected override bool IsComponentValid => _origin != null;

        public CombatShapeData WorldShape
        {
            get
            {
                if (!IsComponentAvailable)
                {
                    throw new InvalidOperationException("Hurtbox is unavailable.");
                }

                return CombatGeometryUtility.CreateWorldShape(
                    _shape, _origin.Position, ParentNetworkBehaviour.transform.right);
            }
        }

        protected override void OnInitialize()
        {
            if (!IsComponentValid) throw new InvalidOperationException("Hurtbox requires an injected movement origin.");
            CombatShapeValidation.ValidateSettings(_shape);
        }
    }
}
