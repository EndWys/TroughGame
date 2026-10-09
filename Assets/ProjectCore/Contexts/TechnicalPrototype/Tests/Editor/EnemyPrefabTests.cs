using System;
using System.Linq;
using System.Reflection;
using Fusion;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyPrefabTests
    {
        private const string PrefabPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Implementations/Enemy/" +
            "GraphicResources/Prefabs/Prefab_Enemy_NetworkEntity.prefab";
        private const string DashPrefabPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Implementations/Enemy/" +
            "GraphicResources/Prefabs/Prefab_Enemy_Dash_NetworkEntity.prefab";
        private const string DashAbilityConfigPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Implementations/Enemy/" +
            "GraphicResources/Configs/Config_TechnicalPrototype_Enemy_DashAbility.asset";
        private const string DashMovementConfigPath =
            "Assets/ProjectCore/Contexts/GameCore/Features/Modules/Movement/GraphicResources/Configs/" +
            "Config_GameCore_Movement_Dash.asset";
        private const string TechnicalPrototypeScenePath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/GraphicResources/Scenes/" +
            "Scene_TechnicalPrototype.unity";

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
            EnemyAbilityCollectionComponent abilityCollection =
                prefab.GetComponent<EnemyAbilityCollectionComponent>();
            Assert.That(abilityCollection, Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyMovementStateComponent>(), Is.Not.Null);

            SerializedObject serializedEnemyEntity = new(enemyEntity);
            Assert.That(
                serializedEnemyEntity.FindProperty("_inputSourceComponent").objectReferenceValue,
                Is.SameAs(prefab.GetComponent<EnemyInputSourceComponent>()));
            Assert.That(
                serializedEnemyEntity.FindProperty("_behaviourStateMachine").objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                serializedEnemyEntity.FindProperty("_abilityCollectionComponent").objectReferenceValue,
                Is.SameAs(abilityCollection));
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

            SerializedObject serializedAbilityCollection = new(abilityCollection);
            Assert.That(
                serializedAbilityCollection.FindProperty("_abilityComponents").arraySize,
                Is.Zero);

            Assert.That(prefab.transform.childCount, Is.EqualTo(3));
            Transform visual = prefab.transform.GetChild(0);
            Assert.That(visual.name, Is.EqualTo("Visual"));
            Assert.That(visual.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(visual.GetComponent<Animator>(), Is.Null);
        }

        [Test]
        public void DashEnemyPrefabComposesDashMovement()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DashPrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(AssetDatabase.GetLabels(prefab), Does.Contain("FusionPrefab"));

            Transform movement = prefab.transform.Find("Movement");
            Assert.That(movement, Is.Not.Null);
            EnemyDashState dashState = movement.GetComponent<EnemyDashState>();
            EnemyNetworkEntityInstaller installer = prefab.GetComponent<EnemyNetworkEntityInstaller>();
            Transform behaviour = prefab.transform.Find("Behaviour");
            Assert.That(behaviour, Is.Not.Null);
            EnemyBehaviourChasingState chasingState = behaviour.GetComponent<EnemyBehaviourChasingState>();
            Transform abilities = prefab.transform.Find("Abilities");
            Assert.That(abilities, Is.Not.Null);
            EnemyAbilityCollectionComponent abilityCollection =
                prefab.GetComponent<EnemyAbilityCollectionComponent>();
            EnemyNetworkEntityComponent networkEntity = prefab.GetComponent<EnemyNetworkEntityComponent>();
            TransformMovementBodyComponent movementBody =
                prefab.GetComponent<TransformMovementBodyComponent>();
            DashMovementConfig dashMovementConfig =
                AssetDatabase.LoadAssetAtPath<DashMovementConfig>(DashMovementConfigPath);
            EnemyDashAbilityConfig dashAbilityConfig =
                AssetDatabase.LoadAssetAtPath<EnemyDashAbilityConfig>(DashAbilityConfigPath);
            EnemyDashAbilityComponent dashAbility = abilities.GetComponent<EnemyDashAbilityComponent>();

            Assert.That(dashState, Is.Not.Null);
            Assert.That(abilityCollection, Is.Not.Null);
            Assert.That(dashMovementConfig, Is.Not.Null);
            Assert.That(dashAbilityConfig, Is.Not.Null);
            Assert.That(dashAbility, Is.Not.Null);

            SerializedObject serializedInstaller = new(installer);
            Assert.That(
                serializedInstaller.FindProperty("_abilityCollectionComponent").objectReferenceValue,
                Is.SameAs(abilityCollection));

            SerializedObject serializedNetworkEntity = new(networkEntity);
            Assert.That(
                serializedNetworkEntity.FindProperty("_abilityCollectionComponent")
                    .objectReferenceValue,
                Is.SameAs(abilityCollection));

            SerializedObject serializedAbilityCollection = new(abilityCollection);
            SerializedProperty abilityComponents =
                serializedAbilityCollection.FindProperty("_abilityComponents");
            Assert.That(abilityComponents.arraySize, Is.EqualTo(1));
            Assert.That(
                abilityComponents.GetArrayElementAtIndex(0).objectReferenceValue,
                Is.SameAs(dashAbility));

            SerializedObject serializedDashState = new(dashState);
            Assert.That(
                serializedDashState.FindProperty("_movementBody").objectReferenceValue,
                Is.SameAs(movementBody));
            Assert.That(
                serializedDashState.FindProperty("_dashMovementConfig").objectReferenceValue,
                Is.SameAs(dashMovementConfig));

            SerializedObject serializedDashAbility = new(dashAbility);
            Assert.That(
                serializedDashAbility.FindProperty("_dashState").objectReferenceValue,
                Is.SameAs(dashState));
            Assert.That(
                serializedDashAbility.FindProperty("_movementBody").objectReferenceValue,
                Is.SameAs(movementBody));
            Assert.That(
                serializedDashAbility.FindProperty("_dashAbilityConfig").objectReferenceValue,
                Is.SameAs(dashAbilityConfig));

            SerializedObject serializedChasingState = new(chasingState);
            Assert.That(serializedChasingState.FindProperty("_chaseAbilityDefinitions"), Is.Null);
        }

        [Test]
        public void TechnicalPrototypeSceneConfiguresDashEnemySpawnReference()
        {
            Scene scene = EditorSceneManager.OpenScene(
                TechnicalPrototypeScenePath,
                OpenSceneMode.Additive);

            try
            {
                EnemySpawnComponent spawnComponent = FindEnemySpawnComponent(scene);
                FieldInfo dashPrefabField = typeof(EnemySpawnComponent).GetField(
                    "_dashEnemyPrefab",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                NetworkPrefabRef dashPrefab = (NetworkPrefabRef)dashPrefabField.GetValue(spawnComponent);
                Guid dashPrefabGuid = new(AssetDatabase.AssetPathToGUID(DashPrefabPath));

                Assert.That(dashPrefab.IsValid, Is.True);
                Assert.That((Guid)dashPrefab, Is.EqualTo(dashPrefabGuid));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
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

        private static EnemySpawnComponent FindEnemySpawnComponent(Scene scene)
        {
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                EnemySpawnComponent spawnComponent =
                    rootObject.GetComponentInChildren<EnemySpawnComponent>(true);

                if (spawnComponent != null)
                {
                    return spawnComponent;
                }
            }

            Assert.Fail("Technical Prototype scene requires an EnemySpawnComponent.");
            return null;
        }
    }
}
