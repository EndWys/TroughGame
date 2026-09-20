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
        public void EnemyPrefabContainsOnlyNetworkIdentityAndPlaceholderVisual()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(AssetDatabase.GetLabels(prefab), Does.Contain("FusionPrefab"));
            Assert.That(prefab.GetComponent<NetworkObject>(), Is.Not.Null);
            NetworkTransform networkTransform = prefab.GetComponent<NetworkTransform>();
            Assert.That(networkTransform, Is.Not.Null);
            Assert.That(networkTransform.SyncParent, Is.False);
            Assert.That(networkTransform.SyncScale, Is.False);
            Assert.That(prefab.GetComponent<EnemyNetworkEntityComponent>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<GameObjectContext>(), Is.Null);
            Assert.That(prefab.GetComponents<Component>().Select(component => component.GetType().Name),
                Is.EquivalentTo(new[]
                {
                    nameof(Transform), nameof(NetworkObject), nameof(NetworkTransform),
                    nameof(EnemyNetworkEntityComponent), "NetworkObjectPrefabData",
                }));
            Assert.That(prefab.transform.childCount, Is.EqualTo(1));
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
