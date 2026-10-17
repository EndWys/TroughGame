using System;
using ProjectCore.Template;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class CombatDebugVisualizationComponent : BaseNetworkEntityComponent, IDebugDrawable
    {
        private const string IdleChannel = "Combat/Hurtboxes/Idle";
        private const string ContactChannel = "Combat/Hurtboxes/Contact";

        [SerializeField] private CombatHurtboxComponent _hurtbox;
        [SerializeField] private CombatHitProbeComponent _contactSource;
        [SerializeField, Min(0.001f)] private float _contactHighlightSeconds = 0.20f;

        private NetworkEntityRegistry _registry;
        private double _highlightUntilTime;

        [Inject]
        private void Construct(NetworkEntityRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public bool IsContactHighlighted =>
            IsComponentAvailable && Time.unscaledTimeAsDouble < _highlightUntilTime;

        public int LastContactTick { get; private set; } = -1;

        public uint LastContactExecutionId { get; private set; }

        public NetworkEntityIdData LastContactSourceEntityId { get; private set; }

        protected override bool IsComponentValid => _hurtbox != null && _hurtbox.IsComponentAvailable;

        public void DrawDebug(DebugVisualizationContextAdapter context)
        {
            if (!IsComponentAvailable) return;

            string channel = IsContactHighlighted ? ContactChannel : IdleChannel;
            context.Value(channel, "Last Contact Tick", LastContactTick);
            context.Value(channel, "Last Contact Execution", LastContactExecutionId);
            context.Value(channel, "Last Contact Source", LastContactSourceEntityId);
            CombatShapeData shape = _hurtbox.WorldShape;
            if (shape.ShapeType == CombatShapeType.Circle)
            {
                context.Radius2D(shape.Center, shape.Radius, channel);
                return;
            }

            Quaternion rotation = Quaternion.Euler(0f, 0f, shape.RotationDegrees);
            Vector3 center = shape.Center;
            Vector3 x = rotation * new Vector3(shape.HalfExtents.x, 0f, 0f);
            Vector3 y = rotation * new Vector3(0f, shape.HalfExtents.y, 0f);
            context.Line(center - x - y, center + x - y, channel);
            context.Line(center + x - y, center + x + y, channel);
            context.Line(center + x + y, center - x + y, channel);
            context.Line(center - x + y, center - x - y, channel);
        }

        protected override void OnInitialize()
        {
            if (_hurtbox == null || _registry == null)
                throw new InvalidOperationException("Combat visualization requires a hurtbox and entity registry.");
            if (float.IsNaN(_contactHighlightSeconds) || float.IsInfinity(_contactHighlightSeconds) ||
                _contactHighlightSeconds <= 0f)
                throw new InvalidOperationException("Contact highlight duration must be finite and positive.");
        }

        protected override void OnNetworkTick()
        {
            var parent = ParentNetworkBehaviour;
            if (!parent.Runner.IsServer || !parent.HasStateAuthority || !parent.Runner.IsForward ||
                _contactSource == null || !_contactSource.IsComponentAvailable ||
                _contactSource.ContactTick != parent.Runner.Tick.Raw) return;

            // Capture immediately after the probe, before another tick clears its contacts.
            var contacts = _contactSource.Contacts;
            for (int i = 0; i < contacts.Count; i++)
            {
                CombatContactData contact = contacts[i];
                if (_registry.TryGetComponent(contact.TargetEntityId, out CombatDebugVisualizationComponent target) &&
                    target.IsComponentAvailable && target.ParentNetworkBehaviour.Runner == parent.Runner)
                    target.ShowContact(contact);
            }
        }

        protected override void OnResetLocalRuntime()
        {
            _highlightUntilTime = 0d;
            LastContactTick = -1;
            LastContactExecutionId = 0;
            LastContactSourceEntityId = NetworkEntityIdData.None;
        }

        protected override void OnNetworkContextInvalidated()
        {
            OnResetLocalRuntime();
        }

        private void OnEnable()
        {
            DebugVisualizationUtility.Register(this);
        }

        private void OnDisable()
        {
            DebugVisualizationUtility.Unregister(this);
            OnResetLocalRuntime();
        }

        private void ShowContact(in CombatContactData contact)
        {
            LastContactTick = contact.Tick;
            LastContactExecutionId = contact.ExecutionId;
            LastContactSourceEntityId = contact.SourceEntityId;
            _highlightUntilTime = Time.unscaledTimeAsDouble + _contactHighlightSeconds;
        }
    }
}
