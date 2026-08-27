using System.Linq;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEditor;
using UnityEditor.Animations;
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
        private const string AnimationsPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/" +
            "Implementations/Player/GraphicResources/Animations";
        private const float DodgeDurationSeconds = 0.2f;

        [Test]
        public void PlayerPrefabComposesNetworkAndMovementModules()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.name, Is.EqualTo("Prefab_Player_NetworkEntity"));
            Assert.That(AssetDatabase.GetLabels(prefab).Contains("FusionPrefab"), Is.True);
            Assert.That(HasComponentNamed(prefab, "NetworkObject"), Is.True);
            Assert.That(HasComponentNamed(prefab, "NetworkTransform"), Is.True);
            Assert.That(prefab.GetComponent<GameObjectContext>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerNetworkEntityInstaller>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerNetworkEntityComponent>(), Is.Not.Null);
            Assert.That(
                prefab.GetComponent<PlayerNetworkEntityComponent>(),
                Is.InstanceOf<IPlayerFacingDirectionAccessor>());
            Assert.That(
                prefab.GetComponent<PlayerNetworkEntityComponent>(),
                Is.InstanceOf<IPlayerFacingDirectionMutator>());
            Assert.That(prefab.GetComponent<PlayerMediatorComponent>(), Is.Not.Null);

            SerializedObject playerEntity = new(prefab.GetComponent<PlayerNetworkEntityComponent>());
            Assert.That(playerEntity.FindProperty("_inputSourceComponent").objectReferenceValue, Is.Not.Null);
            Assert.That(
                playerEntity.FindProperty("_playerMediatorComponent")
                    .objectReferenceValue,
                Is.Not.Null);
            Assert.That(playerEntity.FindProperty("_movementStateMachine").objectReferenceValue, Is.Not.Null);
            Assert.That(
                playerEntity.FindProperty("_movementBodyComponent")
                    .objectReferenceValue,
                Is.Not.Null);
            Assert.That(
                playerEntity.FindProperty("_cameraTargetComponent")
                    .objectReferenceValue,
                Is.Not.Null);

            SerializedObject playerMediator = new(prefab.GetComponent<PlayerMediatorComponent>());
            Assert.That(
                playerMediator.FindProperty("_playerAnimatorComponent")
                    .objectReferenceValue,
                Is.Not.Null);
            Assert.That(prefab.GetComponent<CameraTargetComponent>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerInputSourceComponent>(), Is.Not.Null);

            Transform movement = prefab.transform.Find("Movement");

            Assert.That(movement, Is.Not.Null);
            Assert.That(movement.GetComponent<PlayerMovementStateMachine>(), Is.Not.Null);
            Assert.That(movement.GetComponent<IdleState>(), Is.Not.Null);
            Assert.That(movement.GetComponent<LocomotionState>(), Is.Not.Null);
            Assert.That(movement.GetComponent<DodgeState>(), Is.Not.Null);

            SerializedObject movementStateMachine = new(movement.GetComponent<PlayerMovementStateMachine>());
            Assert.That(movementStateMachine.FindProperty("_idleState").objectReferenceValue, Is.Not.Null);

            TransformMovementBodyComponent movementBody =
                prefab.GetComponent<TransformMovementBodyComponent>();

            Assert.That(movementBody, Is.Not.Null);
            Assert.That(movementBody.Offset, Is.EqualTo(new Vector2(0f, 0.2f)));
            Assert.That(movementBody.CollisionRadius, Is.EqualTo(0.2f));
            Assert.That(
                movementBody.CollisionMask.value,
                Is.EqualTo(1 << LayerMask.NameToLayer("LevelCollision")));
            Assert.That(prefab.GetComponent<Rigidbody>(), Is.Null);
            Assert.That(prefab.GetComponent<Rigidbody2D>(), Is.Null);
            Assert.That(prefab.transform.childCount, Is.EqualTo(2));
            Assert.That(prefab.transform.GetChild(0).name, Is.EqualTo("Visual"));
            Assert.That(prefab.transform.GetChild(1).name, Is.EqualTo("Movement"));
            Transform visual = prefab.transform.GetChild(0);
            Assert.That(visual.GetComponent<SpriteRenderer>(), Is.Not.Null);

            PlayerAnimatorComponent playerAnimator = visual.GetComponent<PlayerAnimatorComponent>();
            Animator animator = visual.GetComponent<Animator>();
            Assert.That(playerAnimator, Is.Not.Null);
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);

            SerializedObject serializedPlayerAnimator = new(playerAnimator);
            Assert.That(
                serializedPlayerAnimator.FindProperty("_animator").objectReferenceValue,
                Is.SameAs(animator));
            Assert.That(serializedPlayerAnimator.FindProperty("_positionSource"), Is.Null);

            SerializedObject locomotionState = new(movement.GetComponent<LocomotionState>());
            Assert.That(
                locomotionState.FindProperty("_locomotionMovementConfig")
                    .objectReferenceValue,
                Is.Not.Null);
            Assert.That(locomotionState.FindProperty("_movementBody").objectReferenceValue, Is.Not.Null);
            SerializedObject dodgeState = new(movement.GetComponent<DodgeState>());
            Assert.That(dodgeState.FindProperty("_movementBody").objectReferenceValue, Is.Not.Null);
            Assert.That(dodgeState.FindProperty("_dodgeMovementConfig").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void PlayerAnimationAssetsHaveExpectedSetup()
        {
            string framesPath = AnimationsPath + "/Frames";
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { framesPath });
            Assert.That(textureGuids, Has.Length.EqualTo(87));

            foreach (string textureGuid in textureGuids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(textureGuid);
                string frameName = System.IO.Path.GetFileNameWithoutExtension(texturePath);
                string[] frameNameSegments = frameName.Split('_');
                Assert.That(frameNameSegments, Has.Length.EqualTo(5), texturePath);
                Assert.That(frameNameSegments[0], Is.EqualTo("Sprite"), texturePath);
                Assert.That(frameNameSegments[1], Is.EqualTo("Player"), texturePath);
                CollectionAssert.Contains(
                    new[] { "Idle", "Walk", "Dodge" },
                    frameNameSegments[2],
                    texturePath);
                CollectionAssert.Contains(
                    new[] { "NorthEast", "NorthWest", "SouthEast", "SouthWest" },
                    frameNameSegments[3],
                    texturePath);
                Assert.That(frameNameSegments[4], Has.Length.EqualTo(3), texturePath);
                Assert.That(int.TryParse(frameNameSegments[4], out _), Is.True, texturePath);

                TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                Assert.That(importer, Is.Not.Null, texturePath);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), texturePath);
                Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), texturePath);
                Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(100f), texturePath);
                Assert.That(importer.mipmapEnabled, Is.False, texturePath);
                Assert.That(
                    importer.textureCompression,
                    Is.EqualTo(TextureImporterCompression.Uncompressed),
                    texturePath);
                Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp), texturePath);
            }

            string[] clipGuids = AssetDatabase.FindAssets(
                "t:AnimationClip",
                new[] { AnimationsPath + "/Clips" });
            Assert.That(clipGuids, Has.Length.EqualTo(12));

            foreach (string clipGuid in clipGuids)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    AssetDatabase.GUIDToAssetPath(clipGuid));

                if (!clip.name.Contains("_Dodge"))
                {
                    continue;
                }

                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                Assert.That(clip.length, Is.EqualTo(DodgeDurationSeconds).Within(0.001f), clip.name);
                Assert.That(settings.loopTime, Is.False, clip.name);
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                AnimationsPath + "/Controller_Player.controller");
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.parameters.Select(parameter => parameter.name),
                Is.EquivalentTo(new[] { "MovementState", "DirectionX", "DirectionY" }));
            Assert.That(controller.layers[0].stateMachine.states.Select(state => state.state.name),
                Is.EquivalentTo(new[] { "Idle", "Locomotion", "Dodge" }));
        }

        private static bool HasComponentNamed(GameObject gameObject, string componentTypeName)
        {
            Component[] components = gameObject.GetComponents<Component>();

            for (int index = 0; index < components.Length; index++)
            {
                if (components[index] != null && components[index].GetType().Name == componentTypeName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
