using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class CombatHitProbeComponent : BaseNetworkEntityComponent, ICombatContactAccessor
    {
        private readonly CombatContactProcessor _processor = new();
        private readonly List<NetworkEntityIdData> _targets = new();
        private readonly List<CombatContactData> _contacts = new();
        private ReadOnlyCollection<CombatContactData> _contactView;
        private ICombatActionSnapshotAccessor _action;
        private ICombatAttackResolver _attacks;
        private ICombatTargetQuery _query;
        private IEntityPositionAccessor _origin;

        [Inject]
        private void Construct(
            ICombatActionSnapshotAccessor action,
            ICombatAttackResolver attacks,
            ICombatTargetQuery query,
            IEntityPositionAccessor origin)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _attacks = attacks ?? throw new ArgumentNullException(nameof(attacks));
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _origin = origin ?? throw new ArgumentNullException(nameof(origin));
        }

        public int ContactTick { get; private set; } = -1;

        public IReadOnlyList<CombatContactData> Contacts => _contactView ??= _contacts.AsReadOnly();

        public int LastContactCount => _contacts.Count;

        public CombatShapeData LastShape { get; private set; }

        protected override void OnInitialize()
        {
            if (_action == null || _attacks == null || _query == null || _origin == null)
                throw new InvalidOperationException("Hit probe requires action, attack, query and origin dependencies.");
        }

        protected override void OnNetworkTick()
        {
            var parent = ParentNetworkBehaviour;
            if (!parent.Runner.IsServer || !parent.HasStateAuthority)
            {
                OnResetLocalRuntime();
                return;
            }

            // Replay cannot erase forward results or reopen the duplicate-hit ledger.
            if (!parent.Runner.IsForward) return;

            ContactTick = parent.Runner.Tick.Raw;
            _targets.Clear();
            _contacts.Clear();
            CombatActionSnapshotData snapshot = _action.ActionSnapshot;
            if (snapshot.Phase != ActionPhase.Active || snapshot.Kind != CombatActionKind.Attack)
            {
                _processor.Reset();
                return;
            }

            if (!_attacks.TryGetAttack(snapshot.ActionId, out CombatAttackConfig attack))
                throw new InvalidOperationException($"No attack definition for action ID {snapshot.ActionId}.");

            LastShape = CombatGeometryUtility.CreateWorldShape(attack.Hitbox, _origin.Position, snapshot.Direction);
            NetworkEntityIdData sourceId = EntityId;
            _query.FillTargetIds(sourceId, LastShape, _targets);
            _processor.Process(sourceId, snapshot.ExecutionId, ContactTick, _targets, _contacts);
        }

        protected override void OnResetLocalRuntime()
        {
            _processor.Reset();
            _targets.Clear();
            _contacts.Clear();
            ContactTick = -1;
            LastShape = default;
        }

        protected override void OnNetworkContextInvalidated()
        {
            OnResetLocalRuntime();
        }
    }
}
