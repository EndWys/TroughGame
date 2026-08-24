using System.Linq;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerPrefabTests
    {
        private const string PrefabPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/" +
            "Implementations/Player/GraphicResources/Prefabs/" +
            "Prefab_Player_NetworkEntity.prefab";

        [Test]
        public void PlayerPrefabComposesNetworkAndMovementModules()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.name, Is.EqualTo("Prefab_Player_NetworkEntity"));
            Assert.That(
                AssetDatabase.GetLabels(prefab).Contains("FusionPrefab"),
                Is.True);
            Assert.That(HasComponentNamed(prefab, "NetworkObject"), Is.True);
            Assert.That(HasComponentNamed(prefab, "NetworkTransform"), Is.True);
            Assert.That(prefab.GetComponent<GameObjectContext>(), Is.Not.Null);
            Assert.That(
                prefab.GetComponent<PlayerNetworkEntityInstaller>(),
                Is.Not.Null);
            Assert.That(
                prefab.GetComponent<PlayerNetworkEntityComponent>(),
                Is.Not.Null);
            Assert.That(
                prefab.GetComponent<CameraTargetComponent>(),
                Is.Not.Null);

            Transform movement = prefab.transform.Find("Movement");

            Assert.That(movement, Is.Not.Null);
            Assert.That(
                movement.GetComponent<PlayerMovementStateMachine>(),
                Is.Not.Null);
            Assert.That(movement.GetComponent<LocomotionState>(), Is.Not.Null);

            TransformMovementBodyComponent movementBody =
                prefab.GetComponent<TransformMovementBodyComponent>();

            Assert.That(movementBody, Is.Not.Null);
            Assert.That(movementBody.Offset, Is.EqualTo(new Vector2(0f, 0.2f)));
            Assert.That(movementBody.CollisionRadius, Is.EqualTo(0.4f));
            Assert.That(
                movementBody.CollisionMask.value,
                Is.EqualTo(1 << LayerMask.NameToLayer("LevelCollision")));
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Null);
            Assert.That(prefab.GetComponent<Rigidbody2D>(), Is.Null);
            Assert.That(prefab.transform.childCount, Is.EqualTo(2));
            Assert.That(prefab.transform.GetChild(0).name, Is.EqualTo("Visual"));
            Assert.That(prefab.transform.GetChild(1).name, Is.EqualTo("Movement"));
            Assert.That(
                prefab.transform.GetChild(0).GetComponent<SpriteRenderer>(),
                Is.Not.Null);

            SerializedObject locomotionState = new SerializedObject(
                movement.GetComponent<LocomotionState>());
            Assert.That(
                locomotionState.FindProperty("_locomotionConfig")
                    .objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                locomotionState.FindProperty("_movementBody")
                    .objectReferenceValue,
                Is.Not.Null);
        }

        private static bool HasComponentNamed(
            GameObject gameObject,
            string componentTypeName)
        {
            Component[] components = gameObject.GetComponents<Component>();

            for (int index = 0; index < components.Length; index++)
            {
                if (components[index] != null &&
                    components[index].GetType().Name == componentTypeName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
