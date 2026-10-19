using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerDebugVisualizationComponent : BaseDebugInspectableComponent, IDebugDrawable
    {
        private const string StateChannel = "Player/State";
        private const string MovementChannel = "Player/Movement";
        private const string InputChannel = "Player/Input";
        private const string InputLockChannel = "Player/Input/Lock";
        private const string NetworkChannel = "Player/Network";
        private const float FacingDirectionLength = 0.45f;
        private const float InputDirectionLength = 0.6f;
        private const float VelocityLengthScale = 0.2f;
        private const float WorldLabelSpacing = 0.25f;
        private static readonly Vector3 WorldLabelsBaseOffset = Vector3.up * 1.35f;

        [SerializeField] private PlayerNetworkEntityComponent _playerNetworkEntity;
        [SerializeField] private PlayerInputSourceComponent _inputSource;
        [SerializeField] private TransformMovementBodyComponent _movementBody;
        [SerializeField] private CombatHitProbeComponent _combatHitProbe;

        public void DrawDebug(DebugVisualizationContextAdapter context)
        {
            if (_playerNetworkEntity == null || _inputSource == null || _movementBody == null)
            {
                return;
            }

            Vector3 bodyPosition = ToWorldPosition(_movementBody.Position);

            DrawState(context, bodyPosition);
            DrawCombat(context);
            DrawMovement(context, bodyPosition);
            DrawInput(context, bodyPosition);
            DrawNetwork(context);
            DrawWorldLabels(context, bodyPosition);
        }

        private void DrawState(DebugVisualizationContextAdapter context, Vector3 bodyPosition)
        {
            context.Value(StateChannel, "Current", _playerNetworkEntity.CurrentState);
            context.Value(StateChannel, "Previous", _playerNetworkEntity.PreviousState);

            context.Value(
                StateChannel,
                "Action Timer",
                GetTimerStatus(_playerNetworkEntity.EntityStateTimer));
        }

        private void DrawCombat(DebugVisualizationContextAdapter context)
        {
            CombatActionSnapshotData action = _playerNetworkEntity.ActionSnapshot;

            context.Value("Player/Combat", "Phase", action.Phase);
            context.Value("Player/Combat", "Execution", action.ExecutionId);
            context.Value("Player/Combat", "Combo Window", action.CanStartNextAction);

            if (_combatHitProbe == null)
                return;

            context.Value("Player/Combat", "Contacts This Tick", _combatHitProbe.LastContactCount);

            if (action.Kind != CombatActionKind.Attack || action.Phase != ActionPhase.Active)
                return;

            CombatShapeData shape = _combatHitProbe.LastShape;

            if (shape.ShapeType == CombatShapeType.Circle)
            {
                context.Radius2D(shape.Center, shape.Radius, "Player/Combat");
                return;
            }

            Quaternion rotation = Quaternion.Euler(0f, 0f, shape.RotationDegrees);
            Vector3 center = shape.Center;
            Vector3 x = rotation * new Vector3(shape.HalfExtents.x, 0f, 0f);
            Vector3 y = rotation * new Vector3(0f, shape.HalfExtents.y, 0f);

            context.Line(center - x - y, center + x - y, "Player/Combat");
            context.Line(center + x - y, center + x + y, "Player/Combat");
            context.Line(center + x + y, center - x + y, "Player/Combat");
            context.Line(center - x + y, center - x - y, "Player/Combat");
        }

        private void DrawMovement(DebugVisualizationContextAdapter context, Vector3 bodyPosition)
        {
            Vector3 transformPosition = transform.position;
            Vector2 velocity = _movementBody.Velocity;

            Vector2 facingDirection = PlayerFacingDirectionUtility.ToVector(
                _playerNetworkEntity.FacingDirection);

            context.Value(MovementChannel, "Transform Position", transformPosition);
            context.Value(MovementChannel, "Body Position", _movementBody.Position);
            context.Value(MovementChannel, "Velocity", velocity);
            context.Value(MovementChannel, "Body Radius", _movementBody.CollisionRadius);
            context.Value(MovementChannel, "Body Offset", _movementBody.Offset);
            context.Value(MovementChannel, "Collision Mask", _movementBody.CollisionMask.value);
            context.Radius2D(bodyPosition, _movementBody.CollisionRadius, MovementChannel);
            context.Line(transformPosition, bodyPosition, MovementChannel);

            if (velocity.sqrMagnitude > 0f)
            {
                context.Line(
                    bodyPosition,
                    bodyPosition + ToWorldDirection(velocity * VelocityLengthScale),
                    MovementChannel);
            }

            context.Line(
                bodyPosition,
                bodyPosition + ToWorldDirection(facingDirection * FacingDirectionLength),
                StateChannel);
        }

        private void DrawInput(DebugVisualizationContextAdapter context, Vector3 bodyPosition)
        {
            bool hasInput = _inputSource.TryGetInput(out PlayerInputSnapshotData input);
            Vector2 direction = input.MoveDirection;

            context.Value(InputChannel, "Direction", hasInput ? direction : "Unavailable");
            context.Value(InputChannel, "Buffered Command", GetBufferedCommandLabel());

            context.Value(
                InputChannel,
                "Buffered Command Timer",
                GetTimerStatus(_playerNetworkEntity.BufferedCommandTimer));

            context.Value(InputChannel, "Locked", _playerNetworkEntity.IsLocked);

            if (direction.sqrMagnitude > 0f)
            {
                context.Line(
                    bodyPosition,
                    bodyPosition + ToWorldDirection(direction * InputDirectionLength),
                    InputChannel);
            }

            if (_playerNetworkEntity.IsLocked)
            {
                context.ZoneDisc2D(
                    bodyPosition,
                    _movementBody.CollisionRadius * 0.35f,
                    InputLockChannel);
            }
        }

        private void DrawNetwork(DebugVisualizationContextAdapter context)
        {
            context.Value(NetworkChannel, "Entity", _playerNetworkEntity.EntityId);
            context.Value(NetworkChannel, "Role", GetNetworkRole());
            context.Value(NetworkChannel, "Input Authority", GetInputAuthorityLabel());
        }

        private void DrawWorldLabels(DebugVisualizationContextAdapter context, Vector3 bodyPosition)
        {
            Vector3 labelsBasePosition = bodyPosition + WorldLabelsBaseOffset;

            context.Label(
                labelsBasePosition,
                $"Entity: {_playerNetworkEntity.EntityId} | Input Authority: {GetInputAuthorityLabel()}",
                NetworkChannel);

            context.Label(
                labelsBasePosition - Vector3.up * WorldLabelSpacing,
                $"{_playerNetworkEntity.CurrentState} <- {_playerNetworkEntity.PreviousState}",
                StateChannel);

            context.Label(
                labelsBasePosition - Vector3.up * WorldLabelSpacing * 2f,
                $"Velocity: {_movementBody.Velocity}",
                MovementChannel);

            context.Label(
                labelsBasePosition - Vector3.up * WorldLabelSpacing * 3f,
                GetInputBufferWorldLabel(),
                InputChannel);
        }

        private string GetInputBufferWorldLabel()
        {
            string lockStatus = _playerNetworkEntity.IsLocked ? "Locked" : "Unlocked";
            string timerStatus = GetTimerStatus(_playerNetworkEntity.BufferedCommandTimer);

            return $"Buffer: {GetBufferedCommandLabel()} | {timerStatus} | {lockStatus}";
        }

        private string GetBufferedCommandLabel()
        {
            return _playerNetworkEntity.BufferedCommand.CommandId == 0
                ? "None"
                : _playerNetworkEntity.BufferedCommand.CommandId.ToString();
        }

        private string GetNetworkRole()
        {
            if (_playerNetworkEntity.HasStateAuthority)
            {
                return _playerNetworkEntity.HasInputAuthority ? "Host Local" : "Host";
            }

            return _playerNetworkEntity.HasInputAuthority ? "Local Predicted" : "Proxy";
        }

        private string GetInputAuthorityLabel()
        {
            return _playerNetworkEntity.Object == null
                ? "Unspawned"
                : _playerNetworkEntity.Object.InputAuthority.ToString();
        }

        private string GetTimerStatus(Fusion.TickTimer timer)
        {
            if (!timer.IsRunning)
            {
                return "Inactive";
            }

            if (_playerNetworkEntity.Runner == null)
            {
                return "Running";
            }

            float? remainingTime = timer.RemainingTime(_playerNetworkEntity.Runner);

            return remainingTime.HasValue ? $"{remainingTime.Value:0.00}s" : "Expired";
        }

        private Vector3 ToWorldPosition(Vector2 position)
        {
            return new Vector3(position.x, position.y, transform.position.z);
        }

        private static Vector3 ToWorldDirection(Vector2 direction)
        {
            return new Vector3(direction.x, direction.y, 0f);
        }
    }
}
