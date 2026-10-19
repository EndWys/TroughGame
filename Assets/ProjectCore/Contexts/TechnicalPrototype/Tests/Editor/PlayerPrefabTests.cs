using System.Linq;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerPrefabTests
    {
        private const string PrefabPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/" +
            "Player/Assets/Prefabs/" +
            "Prefab_Player_NetworkEntity.prefab";
        private const string AnimationsPath =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Features/" +
            "Player/Assets/Animations";
        private const float DodgeDurationSeconds = 0.2f;

        [Test]
        public void CombatReferencesProfilesAndAttackBindingsAreConfigured()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = prefab.GetComponent<PlayerNetworkEntityComponent>();
            var action = prefab.GetComponent<PlayerCombatActionComponent>();
            var controller = prefab.GetComponentInChildren<PlayerEntityStateMachine>();
            var probe = prefab.GetComponent<CombatHitProbeComponent>();
            var hurtbox = prefab.GetComponent<CombatHurtboxComponent>();
            Assert.That(root, Is.InstanceOf<ICombatActionMutator>());
            Assert.That(action, Is.Not.Null);
            Assert.That(controller, Is.Not.Null);
            Assert.That(probe, Is.Not.Null);
            Assert.That(hurtbox, Is.Not.Null);
            var serializedRoot = new SerializedObject(root);
            Assert.That(serializedRoot.FindProperty("_playerActionComponent").objectReferenceValue, Is.SameAs(action));
            Assert.That(serializedRoot.FindProperty("_entityStateMachine").objectReferenceValue, Is.SameAs(controller));
            var serializedController = new SerializedObject(controller);
            Assert.That(serializedController.FindProperty("_inputSource").objectReferenceValue,
                Is.SameAs(prefab.GetComponent<PlayerInputSourceComponent>()));
            Assert.That(serializedController.FindProperty("_dodgeState").objectReferenceValue,
                Is.SameAs(prefab.GetComponentInChildren<PlayerDodgeState>()));
            Assert.That(serializedRoot.FindProperty("_combatHitProbeComponent").objectReferenceValue, Is.SameAs(probe));
            Assert.That(serializedRoot.FindProperty("_combatHurtboxComponent").objectReferenceValue, Is.SameAs(hurtbox));
            var serializedAction = new SerializedObject(action);
            var attack = (CombatAttackConfig)serializedAction.FindProperty("_attackConfig").objectReferenceValue;
            var dodge = (DodgeMovementConfig)new SerializedObject(
                prefab.GetComponentInChildren<PlayerDodgeState>()).FindProperty("_dodgeMovementConfig").objectReferenceValue;
            Assert.That(attack, Is.Not.Null);
            Assert.That(dodge, Is.Not.Null);
            Assert.That(attack.ActionProfile.Kind, Is.EqualTo(CombatActionKind.Attack));
            Assert.That(serializedAction.FindProperty("_dodgePhaseProfile"), Is.Null);
            Assert.That(serializedAction.FindProperty("_movementBody"), Is.Null);
            Assert.That(root, Is.InstanceOf<IEntityPositionAccessor>());
            Assert.That(new SerializedObject(probe).FindProperty("_attackConfig"), Is.Null);
            Assert.That(root, Is.InstanceOf<ICombatAttackResolver>());
            Assert.That(probe, Is.InstanceOf<ICombatContactAccessor>());
            Assert.That(new SerializedObject(prefab.GetComponent<PlayerNetworkEntityInstaller>())
                .FindProperty("_movementBodyComponent"), Is.Null);
            Assert.DoesNotThrow(() => CombatShapeValidation.ValidateSettings(attack.Hitbox));
            Assert.DoesNotThrow(() => CombatActionProfileValidation.CompileTiming(attack.ActionProfile, 1f / 60f));
            Assert.DoesNotThrow(() => MovementConfigValidation.CompileDodgeTiming(dodge, 1f / 60f));
            var movement = prefab.transform.Find("States").GetComponent<PlayerEntityStateMachine>();
            Assert.That(new SerializedObject(movement).FindProperty("_attackState").objectReferenceValue,
                Is.SameAs(movement.GetComponent<PlayerAttackState>()));
            var inputs = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/ProjectCore/Contexts/TechnicalPrototype/Assets/InputActions/InputActions_TechnicalPrototype.inputactions");
            var attackInput = inputs.FindAction(PlayerInputActionConstants.Attack, true);
            Assert.That(attackInput.bindings.Select(binding => binding.path), Does.Contain("<Mouse>/leftButton"));
            Assert.That(attackInput.bindings.Select(binding => binding.path), Does.Contain("<Gamepad>/buttonWest"));
        }

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
            Assert.That(playerEntity.FindProperty("_entityStateMachine").objectReferenceValue, Is.Not.Null);
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
            Assert.That(prefab.GetComponent<TargetableNetworkEntityComponent>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<PlayerInputSourceComponent>(), Is.Not.Null);
            PlayerDebugVisualizationComponent playerDebugVisualization =
                prefab.GetComponent<PlayerDebugVisualizationComponent>();
            Assert.That(playerDebugVisualization, Is.Not.Null);

            SerializedObject serializedPlayerDebugVisualization = new(playerDebugVisualization);
            Assert.That(
                serializedPlayerDebugVisualization.FindProperty("_playerNetworkEntity")
                    .objectReferenceValue,
                Is.SameAs(prefab.GetComponent<PlayerNetworkEntityComponent>()));
            Assert.That(
                serializedPlayerDebugVisualization.FindProperty("_inputSource")
                    .objectReferenceValue,
                Is.SameAs(prefab.GetComponent<PlayerInputSourceComponent>()));
            Assert.That(
                serializedPlayerDebugVisualization.FindProperty("_movementBody")
                    .objectReferenceValue,
                Is.SameAs(prefab.GetComponent<TransformMovementBodyComponent>()));

            Transform movement = prefab.transform.Find("States");

            Assert.That(movement, Is.Not.Null);
            Assert.That(movement.GetComponent<PlayerEntityStateMachine>(), Is.Not.Null);
            Assert.That(movement.GetComponent<PlayerIdleState>(), Is.Not.Null);
            Assert.That(movement.GetComponent<PlayerLocomotionState>(), Is.Not.Null);
            Assert.That(movement.GetComponent<PlayerDodgeState>(), Is.Not.Null);

            SerializedObject movementStateMachine = new(movement.GetComponent<PlayerEntityStateMachine>());
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
            Assert.That(prefab.transform.GetChild(1).name, Is.EqualTo("States"));
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

            SerializedObject locomotionState = new(movement.GetComponent<PlayerLocomotionState>());
            Assert.That(
                locomotionState.FindProperty("_locomotionMovementConfig")
                    .objectReferenceValue,
                Is.Not.Null);
            var installer = new SerializedObject(prefab.GetComponent<PlayerNetworkEntityInstaller>());
            Assert.That(installer.FindProperty("_movementBody").objectReferenceValue, Is.SameAs(movementBody));
            Assert.That(installer.FindProperty("_entityStateMachine").objectReferenceValue, Is.SameAs(movement.GetComponent<PlayerEntityStateMachine>()));
            SerializedObject dodgeState = new(movement.GetComponent<PlayerDodgeState>());

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
                AnimationsPath + "/AnimatorController_Player_Movement.controller");
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
