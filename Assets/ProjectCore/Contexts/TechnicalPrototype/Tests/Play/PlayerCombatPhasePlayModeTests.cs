using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fusion;
using NUnit.Framework;
using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;
using Object = UnityEngine.Object;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerCombatPhasePlayModeTests
    {
        private NetworkRunner _runner;
        private ContactObserver _observer;
        private ForwardTickDriver _tickDriver;

        [UnityTest]
        public IEnumerator SoloAttackLocksPositionExpiresEarlyInputAndChainsWithNewDirection()
        {
            yield return StartSolo();
            SceneContext scene = Object.FindFirstObjectByType<SceneContext>();
            var registry = scene.Container.Resolve<NetworkEntityRegistry>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var container = player.GetComponent<GameObjectContext>().Container;
            Assert.That(container.Resolve<IEntityPositionAccessor>(), Is.SameAs(player));
            Assert.That(container.Resolve<ICombatActionAccessor>(), Is.SameAs(player));
            Assert.That(container.Resolve<ICombatActionMutator>(), Is.SameAs(player));
            Assert.That(container.Resolve<ICombatActionSnapshotAccessor>(), Is.SameAs(player));
            Assert.That(container.Resolve<ICombatActionStateAccessor>(), Is.SameAs(player));
            var inputAccessor = container.Resolve<IPlayerInputAccessor>();
            Assert.That(inputAccessor, Is.SameAs(player.GetComponent<PlayerInputSourceComponent>()));
            var buffer = container.Resolve<IInputBufferController>();
            var action = player.GetComponent<PlayerCombatActionComponent>();
            player.SetLatestInputDirection(Vector2.right);
            Vector3 position = player.transform.position;
            uint firstExecution = 0;
            uint beforeChain = 0;
            PlayerFacingDirection facing = default;
            int step = 0;
            yield return RunTickSequence(() =>
            {
                switch (step)
                {
                    case 0:
                        buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                        step = 1;
                        break;
                    case 1:
                        if (player.ActionState.ActionId == 0) break;
                        Assert.That(player.ActionSnapshot.Phase, Is.EqualTo(ActionPhase.Preparation),
                            "Early input must be injected at attack onset, not when a frame coroutine catches up.");
                        firstExecution = player.ActionState.ExecutionId;
                        facing = player.FacingDirection;
                        PlayerInputSnapshotData previousInput = inputAccessor.Input;
                        Assert.That(previousInput.MoveDirection, Is.EqualTo(Vector2.zero));
                        Assert.That(previousInput.LastNonZeroDirection, Is.EqualTo(Vector2.right));
                        player.SetLatestInputDirection(Vector2.up);
                        Assert.That(inputAccessor.Input.LastNonZeroDirection, Is.EqualTo(Vector2.up),
                            "Input must read the restored network direction rather than a cached copy.");
                        Assert.That(inputAccessor.Input.MoveDirection, Is.EqualTo(Vector2.zero));
                        Assert.That(previousInput.LastNonZeroDirection, Is.EqualTo(Vector2.right),
                            "A snapshot already passed to a state must remain unchanged.");
                        Assert.That(player.FacingDirection, Is.EqualTo(facing));
                        buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                        step = 2;
                        break;
                    case 2:
                        Assert.That(player.ActionState.ExecutionId, Is.EqualTo(firstExecution), "Expired early input chained later.");
                        Assert.That(Vector3.Distance(player.transform.position, position), Is.LessThan(0.001f));
                        Assert.That(player.FacingDirection, Is.EqualTo(facing));
                        if (player.ActionState.ActionId != 0) break;
                        Assert.That(buffer.IsLocked, Is.False);
                        Assert.That(player.BufferedCommand.CommandId, Is.Zero);
                        buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                        step = 3;
                        break;
                    case 3:
                        if (!player.ActionSnapshot.CanStartNextAction) break;
                        beforeChain = player.ActionState.ExecutionId;
                        player.SetLatestInputDirection(Vector2.left);
                        buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                        step = 4;
                        break;
                    case 4:
                        if (player.ActionState.ExecutionId == beforeChain) break;
                        Assert.That(player.ActionState.Direction, Is.EqualTo(Vector2.left));
                        player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                        Assert.That(player.ActionState.ActionId, Is.Zero);
                        Assert.That(buffer.IsLocked, Is.False);
                        Assert.That(player.BufferedCommand.CommandId, Is.Zero);
                        step = 5;
                        break;
                }
            }, () => step == 5);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            StopTickDriver();
            if (_observer != null)
            {
                if (_runner != null && _runner.IsRunning) _runner.RemoveGlobal(_observer);
                if (_observer != null) Object.Destroy(_observer.gameObject);
                _observer = null;
            }
            if (_runner != null && _runner.IsRunning)
            {
                var shutdown = _runner.Shutdown();
                while (!shutdown.IsCompleted) yield return null;
            }
            _runner = null;
        }

        [UnityTest]
        public IEnumerator SoloContactsAreUniqueAndDespawnRespawnClearActionState()
        {
            yield return StartSolo();
            var scene = Object.FindFirstObjectByType<SceneContext>();
            var registry = scene.Container.Resolve<NetworkEntityRegistry>();
            var query = scene.Container.Resolve<ICombatTargetQuery>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var probe = player.GetComponent<CombatHitProbeComponent>();
            var spawn = Object.FindFirstObjectByType<EnemySpawnComponent>();
            spawn.SpawnEnemy();
            spawn.SpawnEnemy();
            var enemies = registry.GetByType(EnemyNetworkEntityConstants.Enemy).ToArray();
            Assert.That(enemies.Length, Is.EqualTo(2));
            foreach (var enemy in enemies)
            {
                _runner.SetIsSimulated(enemy.Object, false);
                enemy.transform.position = player.transform.position + Vector3.right * 0.65f;
                Assert.That(enemy.GetComponent<CombatHurtboxComponent>().IsAvailable, Is.True);
            }
            _observer = new GameObject("Combat tick observer").AddComponent<ContactObserver>();
            _observer.Probe = probe;
            _observer.Player = player;
            _runner.AddGlobal(_observer);
            var contacts = _observer.RecordedContacts;
            player.SetLatestInputDirection(Vector2.right);
            yield return CompleteAttack(player, buffer);
            Assert.That(contacts.Count, Is.EqualTo(2), "Each overlapping enemy should emit one contact.");
            Assert.That(_observer.AllContactsWereActive, Is.True);
            yield return CompleteAttack(player, buffer);
            Assert.That(contacts.Count, Is.EqualTo(4), "The next execution can hit the same targets.");
            Assert.That(contacts.GroupBy(hit => (hit.ExecutionId, hit.TargetEntityId)).All(group => group.Count() == 1), Is.True);
            CombatShapeData lastShape = probe.LastShape;
            NetworkEntityIdData oldEnemyId = enemies[0].EntityId;
            _runner.Despawn(enemies[0].Object);
            var targets = new List<NetworkEntityIdData>();
            query.FillTargetIds(player.EntityId, lastShape, targets);
            Assert.That(targets, Has.No.Member(oldEnemyId));
            Assert.That(targets, Does.Contain(enemies[1].EntityId));

            NetworkEntityIdData oldPlayerId = player.EntityId;
            Vector3 originalPosition = player.transform.position;
            buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
            float deadline = Time.realtimeSinceStartup + 3f;
            while (player.ActionState.ActionId == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(player.ActionState.ActionId, Is.Not.Zero);
            buffer.BufferCommand(new InputBufferCommandDescriptor(MovementInputCommandConstants.DodgeCommandId), 0.15f);
            var spawner = scene.Container.Resolve<NetworkEntitySpawner>();
            var playerSpawn = Object.FindFirstObjectByType<PlayerSpawnComponent>();
            spawner.Despawn(player);
            var fresh = (PlayerNetworkEntityComponent)spawner.Spawn(_runner, playerSpawn.PlayerPrefab,
                new PlayerSpawnPayload(originalPosition, Quaternion.identity), _runner.LocalPlayer);
            _runner.SetPlayerObject(_runner.LocalPlayer, fresh.Object);
            Assert.That(fresh.EntityId, Is.Not.EqualTo(oldPlayerId));
            Assert.That(registry.GetByType(PlayerNetworkEntityConstants.Player).Count, Is.EqualTo(1));
            Assert.That(fresh.ActionState.ActionId, Is.Zero);
            Assert.That(fresh.ActionState.ExecutionId, Is.Zero);
            Assert.That(fresh.BufferedCommand.CommandId, Is.Zero);
            Assert.That(fresh.IsLocked, Is.False);
            Assert.That(fresh.GetComponent<TransformMovementBodyComponent>().Velocity, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator SoloLocalResetGatesCallbacksAndReinitializesTheSameComponents()
        {
            yield return StartSolo();
            var scene = Object.FindFirstObjectByType<SceneContext>();
            var registry = scene.Container.Resolve<NetworkEntityRegistry>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var action = player.GetComponent<PlayerCombatActionComponent>();
            var probe = player.GetComponent<CombatHitProbeComponent>();
            var hurtbox = player.GetComponent<CombatHurtboxComponent>();
            var body = player.GetComponent<TransformMovementBodyComponent>();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            int step = 0;
            yield return RunTickSequence(() =>
            {
                if (step == 0)
                {
                    buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                    step = 1;
                }
                else if (step == 1 && player.ActionState.ActionId != 0)
                {
                    buffer.BufferCommand(new InputBufferCommandDescriptor(MovementInputCommandConstants.DodgeCommandId), 0.15f);
                    CombatActionStateModel before = player.ActionState;
                    Vector3 position = player.transform.position;
                    action.enabled = false;
                    try
                    {
                        Assert.That(action.IsInitialized && action.HasValidNetworkContext, Is.True);
                        Assert.That(action.IsComponentAvailable, Is.False);
                        Assert.That(action.ActionSnapshot, Is.EqualTo(default(CombatActionSnapshotData)));
                        Assert.That(action.TryGetAttack(before.ActionId, out _), Is.False);
                        Assert.That(player.ActionState, Is.EqualTo(before),
                            "Disabling action access must preserve the replicated execution.");
                    }
                    finally { action.enabled = true; }
                    Assert.That(action.ActionSnapshot.ActionId, Is.EqualTo(before.ActionId));
                    Assert.That(action.TryGetAttack(before.ActionId, out _), Is.True);

                    var attackConfigField = typeof(PlayerCombatActionComponent).GetField(
                        "_attackConfig", BindingFlags.Instance | BindingFlags.NonPublic);
                    object attackConfig = attackConfigField.GetValue(action);
                    attackConfigField.SetValue(action, null);
                    try
                    {
                        Assert.That(action.IsComponentAvailable, Is.False);
                        Assert.That(action.ActionSnapshot, Is.EqualTo(default(CombatActionSnapshotData)));
                        Assert.That(action.TryGetAttack(before.ActionId, out _), Is.False);
                        Assert.That(player.ActionState, Is.EqualTo(before));
                    }
                    finally { attackConfigField.SetValue(action, attackConfig); }
                    Assert.That(action.IsComponentAvailable, Is.True);
                    Assert.That(action.ActionSnapshot.ActionId, Is.EqualTo(before.ActionId));

                    action.ResetLocalRuntime();
                    probe.ResetLocalRuntime();
                    hurtbox.ResetLocalRuntime();
                    body.ResetLocalRuntime();
                    Assert.That(action.IsInitialized || probe.IsInitialized || hurtbox.IsInitialized || body.IsInitialized, Is.False);
                    Assert.That(probe.Contacts, Is.Empty);
                    Assert.That(probe.ContactTick, Is.EqualTo(-1));
                    Assert.That(hurtbox.IsAvailable, Is.False);
                    Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
                    action.NetworkTick();
                    action.ClientRender();
                    probe.NetworkTick();
                    body.NetworkTick();
                    Assert.That(player.ActionState.ActionId, Is.EqualTo(before.ActionId));
                    Assert.That(player.ActionState.ExecutionId, Is.EqualTo(before.ExecutionId));
                    Assert.That(player.BufferedCommand.CommandId, Is.EqualTo(MovementInputCommandConstants.DodgeCommandId));
                    Assert.That(player.transform.position, Is.EqualTo(position));
                    body.Init();
                    hurtbox.Init();
                    action.Init();
                    probe.Init();
                    Assert.That(action.IsInitialized && probe.IsInitialized && hurtbox.IsInitialized && body.IsInitialized, Is.True);
                    Assert.That(hurtbox.IsAvailable, Is.True);
                    Assert.That(player.TryGetAttack(before.ActionId, out CombatAttackConfig attack), Is.True);
                    Assert.That(attack.ActionProfile.ActionId, Is.EqualTo(before.ActionId));
                    Assert.That(player.TryGetAttack(ushort.MaxValue, out _), Is.False);
                    player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                    Assert.That(player.BufferedCommand.CommandId, Is.Zero);
                    step = 2;
                }
            }, () => step == 2);
            yield return CompleteAttack(player, buffer);
        }

        [UnityTest]
        public IEnumerator SoloAvailabilityGatesCallbacksWithoutResettingLocalState()
        {
            yield return StartSolo();
            var registry = Object.FindFirstObjectByType<SceneContext>().Container.Resolve<NetworkEntityRegistry>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var probe = player.gameObject.AddComponent<AvailabilityProbe>();
            player.GetComponent<GameObjectContext>().Container.Inject(probe);
            var body = player.GetComponent<TransformMovementBodyComponent>();
            var velocity = player.GetComponent<IMovementBodyVelocityMutator>();
            bool complete = false;
            yield return RunTickSequence(() =>
            {
                probe.enabled = false;
                Assert.That(probe.IsInitialized, Is.False);
                Assert.That(((INetworkEntityComponent)probe).HasSimulationAuthority, Is.True);
                probe.Init();
                Assert.That(probe.IsInitialized && probe.HasValidNetworkContext, Is.True);
                Assert.That(probe.IsComponentAvailable, Is.False);
                probe.NetworkTick();
                probe.ClientRender();
                Assert.That(probe.Ticks + probe.Renders + probe.Invalidations, Is.Zero);
                probe.enabled = true;
                Assert.That(probe.IsComponentAvailable, Is.True);
                probe.NetworkTick();
                probe.ClientRender();
                Assert.That(probe.Ticks, Is.EqualTo(1));
                Assert.That(probe.Renders, Is.EqualTo(1));
                probe.LocalValid = false;
                Assert.That(probe.IsComponentAvailable, Is.False);
                Assert.That(probe.HasSimulationAuthority, Is.True);
                probe.NetworkTick();
                probe.ClientRender();
                Assert.That(probe.Ticks + probe.Renders, Is.EqualTo(2));
                Assert.That(probe.Invalidations, Is.Zero);
                probe.LocalValid = true;
                Assert.That(probe.IsComponentAvailable, Is.True);

                var parent = typeof(BaseNetworkEntityComponent).GetProperty(nameof(BaseNetworkEntityComponent.ParentNetworkBehaviour));
                parent.SetValue(probe, null);
                try
                {
                    Assert.That(probe.HasValidNetworkContext, Is.False);
                    Assert.That(probe.HasSimulationAuthority, Is.False);
                    Assert.That(probe.IsComponentAvailable, Is.False);
                    Assert.That(probe.Invalidations, Is.Zero, "Availability getters must be pure.");
                    probe.NetworkTick();
                    probe.ClientRender();
                    Assert.That(probe.Invalidations, Is.EqualTo(2));
                }
                finally { parent.SetValue(probe, player); }
                probe.enabled = false;
                probe.ResetLocalRuntime();
                Assert.That(probe.IsInitialized, Is.False);
                probe.Init();
                Assert.That(probe.IsInitialized, Is.True);
                Assert.That(probe.IsComponentAvailable, Is.False);
                probe.enabled = true;
                Assert.That(probe.IsComponentAvailable, Is.True);

                body.enabled = false;
                try
                {
                    body.Init();
                    Assert.That(body.IsInitialized && body.HasValidNetworkContext, Is.True);
                    Assert.That(body.HasSimulationAuthority, Is.True);
                    Assert.That(body.IsComponentAvailable, Is.False);
                    velocity.Velocity = Vector2.one;
                    Vector3 position = player.transform.position;
                    body.NetworkTick();
                    body.ClientRender();
                    Assert.That(player.transform.position, Is.EqualTo(position));
                    Assert.That(body.Velocity, Is.EqualTo(Vector2.one));
                    body.ResetLocalRuntime();
                    Assert.That(body.IsInitialized, Is.False);
                    Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
                    body.Init();
                    Assert.That(body.IsInitialized, Is.True);
                    Assert.That(body.IsComponentAvailable, Is.False);
                }
                finally { body.enabled = true; }
                Assert.That(body.IsComponentAvailable, Is.True);
                complete = true;
            }, () => complete);
        }

        [UnityTest]
        public IEnumerator SoloHurtboxAvailabilityAndGeometryUseSpawnedMovementOrigin()
        {
            yield return StartSolo();
            var registry = Object.FindFirstObjectByType<SceneContext>().Container.Resolve<NetworkEntityRegistry>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var hurtbox = player.GetComponent<CombatHurtboxComponent>();
            var origin = player.GetComponent<IEntityPositionAccessor>();
            var originField = typeof(CombatHurtboxComponent).GetField("_origin", BindingFlags.Instance | BindingFlags.NonPublic);
            Transform visual = player.transform.Find("Visual");
            Assert.That(visual, Is.Not.Null);
            bool complete = false;
            yield return RunTickSequence(() =>
            {
                Assert.That(hurtbox.IsComponentAvailable, Is.True);
                hurtbox.enabled = false;
                try
                {
                    Assert.That(hurtbox.IsAvailable, Is.False);
                    Assert.Throws<System.InvalidOperationException>(() => { _ = hurtbox.WorldShape; });
                }
                finally { hurtbox.enabled = true; }
                originField.SetValue(hurtbox, null);
                try
                {
                    Assert.That(hurtbox.IsInitialized && hurtbox.HasValidNetworkContext, Is.True);
                    Assert.That(hurtbox.IsComponentAvailable, Is.False);
                    Assert.Throws<System.InvalidOperationException>(() => { _ = hurtbox.WorldShape; });
                }
                finally { originField.SetValue(hurtbox, origin); }
                Assert.That(hurtbox.IsAvailable, Is.True);
                Vector3 position = player.transform.position;
                Quaternion rotation = player.transform.rotation;
                Vector3 scale = player.transform.localScale;
                Vector3 visualPosition = visual.localPosition;
                Quaternion visualRotation = visual.localRotation;
                Vector3 visualScale = visual.localScale;
                try
                {
                    player.transform.SetPositionAndRotation(new Vector3(3f, 4f, 10f), Quaternion.identity);
                    visual.localPosition = new Vector3(100f, -80f, 20f);
                    visual.localRotation = Quaternion.Euler(0f, 0f, 75f);
                    visual.localScale = Vector3.one * 0.15f;
                    Assert.That(Vector2.Distance(hurtbox.WorldShape.Center, new Vector2(3f, 4.2f)), Is.LessThan(0.001f));
                    Assert.That(hurtbox.WorldShape.Radius, Is.EqualTo(0.25f));
                    player.transform.position = new Vector3(3f, 4f, -30f);
                    player.transform.localScale = new Vector3(4f, 5f, 6f);
                    visual.localScale = Vector3.one * 20f;
                    Assert.That(Vector2.Distance(hurtbox.WorldShape.Center, new Vector2(3f, 4.2f)), Is.LessThan(0.001f));
                    Assert.That(hurtbox.WorldShape.Radius, Is.EqualTo(0.25f));
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    Assert.That(Vector2.Distance(hurtbox.WorldShape.Center, new Vector2(2.8f, 4f)), Is.LessThan(0.001f));
                }
                finally
                {
                    player.transform.SetPositionAndRotation(position, rotation);
                    player.transform.localScale = scale;
                    visual.localPosition = visualPosition;
                    visual.localRotation = visualRotation;
                    visual.localScale = visualScale;
                }
                complete = true;
            }, () => complete);
        }

        [UnityTest]
        public IEnumerator SoloDisabledProbeResumesWithoutRepeatingContactsInTheSameExecution()
        {
            yield return StartSolo();
            var registry = Object.FindFirstObjectByType<SceneContext>().Container.Resolve<NetworkEntityRegistry>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var probe = player.GetComponent<CombatHitProbeComponent>();
            var action = player.GetComponent<PlayerCombatActionComponent>();
            Object.FindFirstObjectByType<EnemySpawnComponent>().SpawnEnemy();
            var enemy = registry.GetByType(EnemyNetworkEntityConstants.Enemy).Single();
            _runner.SetIsSimulated(enemy.Object, false);
            enemy.transform.position = player.transform.position + Vector3.right * 0.65f;
            player.SetLatestInputDirection(Vector2.right);
            int step = 0;
            int contactTick = -1;
            uint execution = 0;
            yield return RunTickSequence(() =>
            {
                switch (step)
                {
                    case 0:
                        buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                        step = 1;
                        break;
                    case 1:
                        if (probe.ContactTick != _runner.Tick.Raw || probe.Contacts.Count == 0) break;
                        Assert.That(probe.Contacts.Count, Is.EqualTo(1));
                        Assert.That(player.ActionSnapshot.Phase, Is.EqualTo(ActionPhase.Active));
                        execution = player.ActionState.ExecutionId;
                        contactTick = probe.ContactTick;
                        probe.enabled = false;
                        probe.NetworkTick();
                        probe.ClientRender();
                        Assert.That(probe.Contacts.Count, Is.EqualTo(1));
                        Assert.That(probe.ContactTick, Is.EqualTo(contactTick));
                        step = 2;
                        break;
                    case 2:
                        Assert.That(_runner.Tick.Raw, Is.GreaterThan(contactTick));
                        Assert.That(probe.ContactTick, Is.EqualTo(contactTick));
                        Assert.That(probe.Contacts.Count, Is.EqualTo(1));
                        probe.enabled = true;
                        step = 3;
                        break;
                    case 3:
                        Assert.That(player.ActionState.ExecutionId, Is.EqualTo(execution));
                        Assert.That(player.ActionSnapshot.Phase, Is.EqualTo(ActionPhase.Active));
                        Assert.That(probe.ContactTick, Is.EqualTo(_runner.Tick.Raw));
                        Assert.That(probe.Contacts, Is.Empty, "Disable must preserve the per-execution contact ledger.");
                        player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                        step = 4;
                        break;
                }
            }, () => step == 4);
        }

        [UnityTest]
        public IEnumerator SoloDodgeUsesMovementTimerAndWaitsForFullRecoveryBeforeAttack()
        {
            yield return StartSolo();
            var player = FindPlayer();
            UseDirectionalInput();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var dodge = player.GetComponentInChildren<PlayerDodgeState>();
            var body = player.GetComponent<TransformMovementBodyComponent>();
            int step = 0;
            int endTick = 0;
            bool preparation = false, active = false, recovery = false, queuedLate = false;
            yield return RunTickSequence(() =>
            {
                if (step == 0)
                {
                    buffer.BufferCommand(new InputBufferCommandDescriptor(MovementInputCommandConstants.DodgeCommandId), 0.15f);
                    step = 1;
                }
                else if (step == 1)
                {
                    AssertDodgeStarted(player);
                    endTick = player.StateTimerEndTick.Value;
                    step = 2;
                }
                if (step == 2)
                {
                    Assert.That(player.ActionState.ActionId, Is.Zero, "Dodge must not be a Combat execution.");
                    if (player.CurrentState != PlayerEntityState.Dodge)
                    {
                        Assert.That(_runner.Tick.Raw, Is.GreaterThanOrEqualTo(endTick));
                        Assert.That(preparation && active && recovery && queuedLate, Is.True);
                        step = 3;
                        return;
                    }
                    Assert.That(player.StateTimerEndTick, Is.EqualTo(endTick), "No early restart of Dodge.");
                    switch (dodge.Phase)
                    {
                        case ActionPhase.Preparation:
                            preparation = true;
                            Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
                            Assert.That(player.IsInvulnerable, Is.False);
                            break;
                        case ActionPhase.Active:
                            Assert.That(player.IsInvulnerable, Is.True);
                            if (!active)
                            {
                                var machine = player.GetComponentInChildren<PlayerEntityStateMachine>();
                                typeof(BaseEntityStateMachine<PlayerEntityState, BaseEntityState<PlayerEntityState, PlayerEntityPayload>, PlayerEntityPayload>).GetField("_enteredState", BindingFlags.Instance | BindingFlags.NonPublic)
                                    .SetValue(machine, null);
                                machine.NetworkTick();
                                Assert.That(player.StateTimerEndTick, Is.EqualTo(endTick), "Local activation restore must not restart the network Dodge timer.");
                                Assert.That(buffer.IsLocked, Is.True, "Local restore must not unlock the replicated command buffer.");
                                buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                            }
                            active = true;
                            break;
                        case ActionPhase.Recovery:
                            Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
                            Assert.That(player.IsInvulnerable, Is.False);
                            if (!recovery) Assert.That(player.BufferedCommand.CommandId, Is.Zero, "Early command must expire during Dodge.");
                            recovery = true;
                            if (!queuedLate && endTick - _runner.Tick.Raw <= 4)
                            {
                                buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                                queuedLate = true;
                            }
                            break;
                    }
                }
                else if (step == 3 && player.ActionState.ActionId != 0)
                {
                    Assert.That(_runner.Tick.Raw, Is.GreaterThanOrEqualTo(endTick));
                    Assert.That(body.Velocity, Is.EqualTo(Vector2.zero), "Attack Enter must stop the previous movement immediately.");
                    player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                    step = 4;
                }
            }, () => step == 4);
        }

        [UnityTest]
        public IEnumerator SoloRepeatedDodgeWaitsUntilRecoveryFinishes()
        {
            yield return StartSolo();
            var player = FindPlayer();
            UseDirectionalInput();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var dodge = player.GetComponentInChildren<PlayerDodgeState>();
            int step = 0, firstEnd = 0;
            bool queued = false;
            yield return RunTickSequence(() =>
            {
                if (step == 0)
                {
                    Assert.That(player.StateTimerEndTick, Is.Null,
                        "An absent movement timer must not be exposed as an existing deadline.");
                    buffer.BufferCommand(new InputBufferCommandDescriptor(MovementInputCommandConstants.DodgeCommandId), 0.15f);
                    step = 1;
                }
                else if (step == 1)
                {
                    AssertDodgeStarted(player);
                    firstEnd = player.StateTimerEndTick.Value;
                    step = 2;
                }
                else if (step == 2)
                {
                    if (_runner.Tick.Raw < firstEnd)
                    {
                        Assert.That(player.CurrentState, Is.EqualTo(PlayerEntityState.Dodge));
                        Assert.That(player.StateTimerEndTick, Is.EqualTo(firstEnd));
                        if (!queued && dodge.Phase == ActionPhase.Recovery)
                        {
                            buffer.BufferCommand(new InputBufferCommandDescriptor(MovementInputCommandConstants.DodgeCommandId), 0.15f);
                            queued = true;
                        }
                    }
                    else if (player.CurrentState == PlayerEntityState.Dodge && player.StateTimerEndTick > firstEnd)
                    {
                        Assert.That(queued, Is.True);
                        Assert.That(player.ActionState.ActionId, Is.Zero);
                        step = 3;
                    }
                }
                else if (step == 3 && player.CurrentState != PlayerEntityState.Dodge)
                {
                    Assert.That(player.StateTimerEndTick, Is.Null, "Dodge completion must clear its movement deadline.");
                    Assert.That(player.IsInvulnerable, Is.False);
                    Assert.That(player.IsLocked, Is.False);
                    step = 4;
                }
            }, () => step == 4, () =>
                $"step={step}, tick={_runner.Tick.Raw}, state={player.CurrentState}, " +
                $"timer={player.StateTimerEndTick}, firstEnd={firstEnd}, queued={queued}, " +
                $"buffer={player.BufferedCommand.CommandId}, locked={player.IsLocked}, " +
                $"mediator={player.GetComponent<PlayerMediatorComponent>().IsComponentAvailable}, " +
                $"movement={player.GetComponentInChildren<PlayerEntityStateMachine>().IsComponentAvailable}");
        }

        [UnityTest]
        public IEnumerator SoloInvulnerabilityExpiresWithoutTheDodgeEndNotification()
        {
            yield return StartSolo();
            var player = FindPlayer();
            UseDirectionalInput();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var dodge = player.GetComponentInChildren<PlayerDodgeState>();
            var movement = player.GetComponentInChildren<PlayerEntityStateMachine>();
            var body = player.GetComponent<TransformMovementBodyComponent>();
            int step = 0, effectEnd = 0;
            yield return RunTickSequence(() =>
            {
                if (step == 0)
                {
                    buffer.BufferCommand(new InputBufferCommandDescriptor(MovementInputCommandConstants.DodgeCommandId), 0.15f);
                    step = 1;
                }
                else if (step == 1)
                {
                    AssertDodgeStarted(player);
                    if (dodge.Phase != ActionPhase.Active) return;
                    Assert.That(player.IsInvulnerable, Is.True);
                    effectEnd = player.InvulnerabilityTimer.TargetTick.Value;
                    movement.enabled = false;
                    body.enabled = false;
                    step = 2;
                }
                else if (step == 2)
                {
                    Assert.That(player.InvulnerabilityTimer.TargetTick, Is.EqualTo(effectEnd), "No end notification should have cleared the stored deadline.");
                    if (_runner.Tick.Raw < effectEnd) Assert.That(player.IsInvulnerable, Is.True);
                    else
                    {
                        Assert.That(player.IsInvulnerable, Is.False);
                        movement.enabled = true;
                        body.enabled = true;
                        step = 3;
                    }
                }
                else if (step == 3 && player.CurrentState != PlayerEntityState.Dodge) step = 4;
            }, () => step == 4);
        }

        [UnityTest]
        public IEnumerator SoloMediatorTickDoesNotConsumeCommandsAndEntityStateStartsAttack()
        {
            yield return StartSolo();
            var player = FindPlayer();
            var mediator = player.GetComponent<PlayerMediatorComponent>();
            var controller = player.GetComponentInChildren<PlayerEntityStateMachine>();
            var action = player.GetComponent<PlayerCombatActionComponent>();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.IsComponentAvailable, Is.True);
            int step = 0;
            yield return RunTickSequence(() =>
            {
                if (step != 0) return;
                Assert.That(player.ActionState.ActionId, Is.Zero);
                buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                mediator.NetworkTick();
                Assert.That(player.ActionState.ActionId, Is.Zero, "Mediator tick must not start an attack.");
                Assert.That(player.BufferedCommand.CommandId, Is.EqualTo(PlayerInputActionConstants.AttackCommandId),
                    "Mediator tick must not consume the pending command.");
                Assert.That(buffer.IsLocked, Is.False);

                controller.NetworkTick();
                Assert.That(player.ActionState.ActionId, Is.Not.Zero);
                Assert.That(player.CurrentState, Is.EqualTo(PlayerEntityState.Attack),
                    "Entity state must start the attack without mediator gameplay routing.");
                Assert.That(player.BufferedCommand.CommandId, Is.Zero);
                Assert.That(buffer.IsLocked, Is.True);
                player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                step = 1;
            }, () => step == 1);
        }

        [UnityTest]
        public IEnumerator SoloAttackCommandResolvesGameplayActionIdOutsideBufferRange()
        {
            yield return StartSolo();
            var player = FindPlayer();
            var action = player.GetComponent<PlayerCombatActionComponent>();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var attackField = typeof(PlayerCombatActionComponent).GetField("_attackConfig",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var originalAttack = (CombatAttackConfig)attackField.GetValue(action);
            var attack = Object.Instantiate(originalAttack);
            var profile = Object.Instantiate(originalAttack.ActionProfile);
            try
            {
                typeof(CombatActionProfileConfig).GetField("_actionId", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, (ushort)300);
                typeof(CombatAttackConfig).GetField("_actionProfile", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(attack, profile);
                attackField.SetValue(action, attack);
                action.Init();
                int step = 0;
                yield return RunTickSequence(() =>
                {
                    if (step == 0)
                    {
                        player.SetLatestInputDirection(Vector2.right);
                        buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                        step = 1;
                    }
                    else if (step == 1 && player.ActionState.ActionId != 0)
                    {
                        Assert.That(PlayerInputActionConstants.AttackCommandId, Is.InRange(1, 127));
                        Assert.That(player.ActionState.ActionId, Is.EqualTo(300));
                        Assert.That(player.ActionSnapshot.ActionId, Is.EqualTo(300));
                        Assert.That(player.TryGetAttack(300, out CombatAttackConfig resolved), Is.True);
                        Assert.That(resolved, Is.SameAs(attack));
                        Assert.That(player.BufferedCommand.CommandId, Is.Zero);
                        Assert.That(buffer.IsLocked, Is.True);
                        player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                        step = 2;
                    }
                }, () => step == 2);
            }
            finally
            {
                if (action != null)
                {
                    if (action.HasValidNetworkContext) player.GetComponentInChildren<PlayerEntityStateMachine>().CancelCurrentAction();
                    attackField.SetValue(action, originalAttack);
                    if (action.HasValidNetworkContext) action.Init();
                }
                Object.Destroy(attack);
                Object.Destroy(profile);
            }
        }

        private PlayerNetworkEntityComponent FindPlayer()
        {
            var registry = Object.FindFirstObjectByType<SceneContext>().Container.Resolve<NetworkEntityRegistry>();
            return (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
        }

        private void AssertDodgeStarted(PlayerNetworkEntityComponent player)
        {
            Assert.That(player.CurrentState, Is.EqualTo(PlayerEntityState.Dodge),
                "The buffered Dodge must enter its movement state on the next simulation tick.");
            Assert.That(player.ActionState.ActionId, Is.Zero, "Dodge must not start a Combat execution.");
            Assert.That(player.IsLocked, Is.True, "The independent input buffer must remain locked during Dodge.");
            Assert.That(player.StateTimerEndTick, Is.Not.Null.And.GreaterThan(_runner.Tick.Raw),
                "Dodge must start the existing movement timer with a future deadline.");
        }

        private static void UseDirectionalInput()
        {
            var callbacks = Object.FindFirstObjectByType<PlayerNetworkInputComponent>();
            Assert.That(callbacks, Is.Not.Null);
            typeof(PlayerNetworkInputComponent).GetField("_localInputReader", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(callbacks, new DirectionalInputReader());
        }

        private sealed class DirectionalInputReader : ILocalInputReader
        {
            public void Capture() { }
            public T ReadValue<T>(string actionPath) where T : struct =>
                typeof(T) == typeof(Vector2) ? (T)(object)Vector2.right : default;
            public LocalButtonStateData ReadButton(string actionPath) => default;
        }

        public sealed class AvailabilityProbe : BaseNetworkEntityComponent
        {
            public bool LocalValid = true;
            public int Ticks;
            public int Renders;
            public int Invalidations;
            protected override bool IsComponentValid => LocalValid;
            protected override void OnNetworkTick() { Ticks++; }
            protected override void OnClientRender() { Renders++; }
            protected override void OnNetworkContextInvalidated() { Invalidations++; }
        }

        public sealed class ContactObserver : SimulationBehaviour, IAfterTick
        {
            public CombatHitProbeComponent Probe;
            public PlayerNetworkEntityComponent Player;
            public readonly List<CombatContactData> RecordedContacts = new();
            public bool AllContactsWereActive = true;
            private int _lastTick = -1;

            public void AfterTick()
            {
                if (!Runner.IsForward || !Runner.IsServer || Probe == null || Player == null ||
                    Probe.ContactTick != Runner.Tick.Raw || _lastTick == Probe.ContactTick) return;
                _lastTick = Probe.ContactTick;
                foreach (CombatContactData contact in Probe.Contacts)
                {
                    AllContactsWereActive &= Player.ActionSnapshot.Phase == ActionPhase.Active;
                    RecordedContacts.Add(contact);
                }
            }
        }

        [UnityTest]
        public IEnumerator SoloHurtboxHighlightCapturesContactBeforeItClearsAndResetsOnDespawn()
        {
            yield return StartSolo();
            var scene = Object.FindFirstObjectByType<SceneContext>();
            var registry = scene.Container.Resolve<NetworkEntityRegistry>();
            var player = (PlayerNetworkEntityComponent)registry.GetByType(PlayerNetworkEntityConstants.Player).Single();
            var buffer = player.GetComponent<GameObjectContext>().Container.Resolve<IInputBufferController>();
            var probe = player.GetComponent<CombatHitProbeComponent>();
            var spawn = Object.FindFirstObjectByType<EnemySpawnComponent>();
            spawn.SpawnEnemy();
            var enemy = registry.GetByType(EnemyNetworkEntityConstants.Enemy).Single();
            var debug = enemy.GetComponent<CombatDebugVisualizationComponent>();
            enemy.GetComponent<TransformMovementBodyComponent>().enabled = false;
            var enemyTransform = enemy.GetComponent<NetworkTransform>();
            enemyTransform.Teleport(player.transform.position + Vector3.right * 0.65f);
            Assert.That(debug.IsComponentAvailable, Is.True);
            Assert.That(debug.IsContactHighlighted, Is.False);
            Assert.That(debug.LastContactTick, Is.EqualTo(-1));
            player.SetLatestInputDirection(Vector2.right);
            int step = 0;
            int contactTick = -1;
            double contactTime = 0d;
            yield return RunTickSequence(() =>
            {
                if (step == 0)
                {
                    buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                    step = 1;
                }
                else if (step == 1 && probe.Contacts.Count > 0)
                {
                    contactTick = probe.ContactTick;
                    contactTime = Time.unscaledTimeAsDouble;
                    Assert.That(debug.LastContactTick, Is.EqualTo(contactTick), "Capture must run in the contact tick.");
                    Assert.That(debug.LastContactExecutionId, Is.EqualTo(player.ActionState.ExecutionId));
                    Assert.That(debug.LastContactSourceEntityId, Is.EqualTo(player.EntityId));
                    Assert.That(debug.IsContactHighlighted, Is.True);
                    step = 2;
                }
                else if (step == 2 && probe.ContactTick != contactTick)
                {
                    Assert.That(probe.Contacts, Is.Empty);
                    Assert.That(debug.LastContactTick, Is.EqualTo(contactTick));
                    if (Time.unscaledTimeAsDouble - contactTime < 0.20d)
                        Assert.That(debug.IsContactHighlighted, Is.True, "A cleared tick list must not clear the flash.");
                    step = 3;
                }
            }, () => step == 3);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(debug.IsContactHighlighted, Is.False);
            Assert.That(debug.LastContactTick, Is.EqualTo(contactTick), "Receipt remains inspectable after the flash.");
            while (player.ActionState.ActionId != 0) yield return null;
            uint firstExecution = debug.LastContactExecutionId;
            yield return CompleteAttack(player, buffer);
            Assert.That(debug.LastContactExecutionId, Is.GreaterThan(firstExecution));
            int lastContactTick = debug.LastContactTick;
            enemyTransform.Teleport(player.transform.position + Vector3.right * 4f);
            yield return CompleteAttack(player, buffer);
            Assert.That(debug.LastContactTick, Is.EqualTo(lastContactTick), "A miss must not create a receipt.");
            debug.enabled = false;
            Assert.That(debug.LastContactTick, Is.EqualTo(-1));
            Assert.That(debug.IsContactHighlighted, Is.False);
            debug.enabled = true;
            Assert.That(debug.IsComponentAvailable, Is.True);
            var oldId = enemy.EntityId;
            _runner.Despawn(enemy.Object);
            Assert.That(registry.TryGet(oldId, out _), Is.False);
            spawn.SpawnEnemy();
            var fresh = registry.GetByType(EnemyNetworkEntityConstants.Enemy).Single()
                .GetComponent<CombatDebugVisualizationComponent>();
            Assert.That(fresh.LastContactTick, Is.EqualTo(-1));
            Assert.That(fresh.IsContactHighlighted, Is.False);
        }

        private IEnumerator CompleteAttack(PlayerNetworkEntityComponent player, IInputBufferController buffer)
        {
            int step = 0;
            yield return RunTickSequence(() =>
            {
                if (step == 0)
                {
                    buffer.BufferCommand(new InputBufferCommandDescriptor(PlayerInputActionConstants.AttackCommandId), 0.15f);
                    step = 1;
                }
                else if (step == 1 && player.ActionState.ActionId != 0) step = 2;
                else if (step == 2 && player.ActionState.ActionId == 0) step = 3;
            }, () => step == 3);
        }

        private IEnumerator RunTickSequence(System.Action work, System.Func<bool> complete,
            System.Func<string> describeState = null)
        {
            Assert.That(_tickDriver, Is.Null);
            _tickDriver = new GameObject("Combat forward tick driver").AddComponent<ForwardTickDriver>();
            // Several simulation ticks may run before the awaiting frame resumes.
            _tickDriver.Work = () => { if (!complete()) work(); };
            _runner.AddGlobal(_tickDriver);
            float deadline = Time.realtimeSinceStartup + 5f;
            while (!complete() && _tickDriver.Error == null && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_tickDriver.Error, Is.Null, _tickDriver.Error?.ToString());
            Assert.That(complete(), Is.True, "Tick-driven combat scenario timed out. " + describeState?.Invoke());
            StopTickDriver();
        }

        private void StopTickDriver()
        {
            if (_tickDriver == null) return;
            if (_runner != null && _runner.IsRunning) _runner.RemoveGlobal(_tickDriver);
            if (_tickDriver != null) Object.Destroy(_tickDriver.gameObject);
            _tickDriver = null;
        }

        public sealed class ForwardTickDriver : SimulationBehaviour, IAfterTick
        {
            public System.Action Work;
            public System.Exception Error;

            public void AfterTick()
            {
                if (Error != null || !Runner.IsForward || !Runner.IsServer) return;
                try { Work?.Invoke(); }
                catch (System.Exception error) { Error = error; }
            }
        }

        private IEnumerator StartSolo()
        {
            var loading = SceneManager.LoadSceneAsync("Scene_Preloader", LoadSceneMode.Single);
            while (!loading.isDone) yield return null;
            var flow = ProjectContext.Instance.Container.Resolve<ISceneFlowService>();
            var screen = ProjectContext.Instance.Container.Resolve<ILoadingScreenSystem>();
            float deadline = Time.realtimeSinceStartup + 15f;
            while ((flow.CurrentSceneDefinition == null || screen.IsVisible || screen.IsTransitioning ||
                SceneManager.GetActiveScene().name != "Scene_TechnicalPrototype") && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_TechnicalPrototype"));
            var scene = Object.FindFirstObjectByType<SceneContext>();
            _runner = Object.FindFirstObjectByType<NetworkRunner>();
            var start = _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Single,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = _runner.GetComponent<INetworkSceneManager>(),
                ObjectProvider = scene.Container.Resolve<INetworkObjectProvider>(),
            });
            deadline = Time.realtimeSinceStartup + 20f;
            while (!start.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(start.IsCompleted, Is.True);
            Assert.That(start.GetAwaiter().GetResult().Ok, Is.True);
            yield return null;
        }
    }
}
