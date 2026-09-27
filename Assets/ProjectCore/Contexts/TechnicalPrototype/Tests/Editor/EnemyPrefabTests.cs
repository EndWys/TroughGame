using System;
using System.Linq;
using Fusion;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyPrefabTests
    {
        private const string PrefabPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Implementations/Enemy/" +
            "GraphicResources/Prefabs/Prefab_Enemy_NetworkEntity.prefab";

        [Test]
        public void EnemyPrefabComposesNetworkBehaviourFoundation()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(AssetDatabase.GetLabels(prefab), Does.Contain("FusionPrefab"));
            Assert.That(prefab.GetComponent<NetworkObject>(), Is.Not.Null);
            NetworkTransform networkTransform = prefab.GetComponent<NetworkTransform>();
            Assert.That(networkTransform, Is.Not.Null);
            Assert.That(networkTransform.SyncParent, Is.False);
            Assert.That(networkTransform.SyncScale, Is.False);
            EnemyNetworkEntityComponent enemyEntity = prefab.GetComponent<EnemyNetworkEntityComponent>();
            Assert.That(enemyEntity, Is.Not.Null);
            Assert.That(prefab.GetComponent<GameObjectContext>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyNetworkEntityInstaller>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyInputSourceComponent>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyDebugVisualizationComponent>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<TransformMovementBodyComponent>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyMovementStateComponent>(), Is.Not.Null);

            SerializedObject serializedEnemyEntity = new(enemyEntity);
            Assert.That(
                serializedEnemyEntity.FindProperty("_inputSourceComponent").objectReferenceValue,
                Is.SameAs(prefab.GetComponent<EnemyInputSourceComponent>()));
            Assert.That(
                serializedEnemyEntity.FindProperty("_behaviourStateMachine").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                serializedEnemyEntity.FindProperty("_movementStateComponent").objectReferenceValue,
                Is.SameAs(prefab.GetComponent<EnemyMovementStateComponent>()));
            Assert.That(
                serializedEnemyEntity.FindProperty("_movementStateMachine").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                serializedEnemyEntity.FindProperty("_movementBodyComponent").objectReferenceValue,
                Is.SameAs(prefab.GetComponent<TransformMovementBodyComponent>()));

            Transform behaviour = prefab.transform.Find("Behaviour");
            Assert.That(behaviour, Is.Not.Null);
            Assert.That(behaviour.GetComponent<EnemyBehaviourStateMachine>(), Is.Not.Null);
            Assert.That(behaviour.GetComponent<EnemyBehaviourDisabledState>(), Is.Not.Null);
            Assert.That(behaviour.GetComponent<EnemyBehaviourTargetSelectionState>(), Is.Not.Null);
            Assert.That(behaviour.GetComponent<EnemyBehaviourWaitingState>(), Is.Not.Null);
            Assert.That(behaviour.GetComponent<EnemyBehaviourChasingState>(), Is.Not.Null);

            SerializedObject serializedStateMachine = new(
                behaviour.GetComponent<EnemyBehaviourStateMachine>());
            Assert.That(
                serializedStateMachine.FindProperty("_disabledState").objectReferenceValue,
                Is.SameAs(behaviour.GetComponent<EnemyBehaviourDisabledState>()));
            Assert.That(
                serializedStateMachine.FindProperty("_targetSelectionState").objectReferenceValue,
                Is.SameAs(behaviour.GetComponent<EnemyBehaviourTargetSelectionState>()));
            Assert.That(
                serializedStateMachine.FindProperty("_waitingState").objectReferenceValue,
                Is.SameAs(behaviour.GetComponent<EnemyBehaviourWaitingState>()));
            Assert.That(
                serializedStateMachine.FindProperty("_chasingState").objectReferenceValue,
                Is.SameAs(behaviour.GetComponent<EnemyBehaviourChasingState>()));

            Transform movement = prefab.transform.Find("Movement");
            Assert.That(movement, Is.Not.Null);
            EnemyMovementStateMachine movementStateMachine =
                movement.GetComponent<EnemyMovementStateMachine>();
            Assert.That(movementStateMachine, Is.Not.Null);
            Assert.That(movement.GetComponent<EnemyIdleState>(), Is.Not.Null);
            Assert.That(movement.GetComponent<EnemyLocomotionState>(), Is.Not.Null);

            SerializedObject serializedMovementStateMachine = new(movementStateMachine);
            Assert.That(
                serializedMovementStateMachine.FindProperty("_idleState").objectReferenceValue,
                Is.SameAs(movement.GetComponent<EnemyIdleState>()));
            Assert.That(
                serializedMovementStateMachine.FindProperty("_locomotionState").objectReferenceValue,
                Is.SameAs(movement.GetComponent<EnemyLocomotionState>()));
            Assert.That(
                serializedMovementStateMachine.FindProperty("_inputSource").objectReferenceValue,
                Is.SameAs(prefab.GetComponent<EnemyInputSourceComponent>()));

            Assert.That(prefab.transform.childCount, Is.EqualTo(3));
            Transform visual = prefab.transform.GetChild(0);
            Assert.That(visual.name, Is.EqualTo("Visual"));
            Assert.That(visual.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(visual.GetComponent<Animator>(), Is.Null);
        }

        [Test]
        public void EnemyFactoryRejectsStoppedRunnerAndAllowsRepeatedDespawn()
        {
            var factory = new EnemyNetworkEntityFactory(new NetworkEntityIdFactory());
            var gameObject = new GameObject("EnemyFactoryTestRunner");

            try
            {
                NetworkRunner runner = gameObject.AddComponent<NetworkRunner>();
                Assert.Throws<InvalidOperationException>(() => factory.Spawn(
                    runner, default, null, new EnemySpawnPayload(Vector3.zero, Quaternion.identity)));
                Assert.DoesNotThrow(() => factory.Despawn(null));
                EnemyNetworkEntityComponent unspawned =
                    gameObject.AddComponent<EnemyNetworkEntityComponent>();
                Assert.DoesNotThrow(() => factory.Despawn(unspawned));
                Assert.DoesNotThrow(() => factory.Despawn(unspawned));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
