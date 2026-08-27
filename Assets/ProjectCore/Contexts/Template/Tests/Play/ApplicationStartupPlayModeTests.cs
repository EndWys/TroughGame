using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;
using Fusion;
using NUnit.Framework;
using ProjectCore.GameCore;
using ProjectCore.Preloader;
using ProjectCore.TechnicalPrototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class ApplicationStartupPlayModeTests
    {
        [UnityTest]
        public IEnumerator PreloaderBootstrapsTechnicalPrototypeAndPreservesProjectContext()
        {
            yield return LoadPreloaderAndWaitForTechnicalPrototype();

            ProjectContext projectContext = ProjectContext.Instance;
            ISceneFlowService sceneFlowService =
                projectContext.Container.Resolve<ISceneFlowService>();
            SceneContext sceneContext = Object.FindFirstObjectByType<SceneContext>();

            Assert.That(sceneContext, Is.Not.Null);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_TechnicalPrototype"));
            Assert.That(
                sceneFlowService.CurrentSceneDefinition,
                Is.TypeOf<TechnicalPrototypeSceneDefinition>());

            IGameSceneLifecycle sceneLifecycle =
                sceneContext.Container.Resolve<IGameSceneLifecycle>();
            INetworkObjectProvider objectProvider =
                sceneContext.Container.Resolve<INetworkObjectProvider>();
            ZenjectNetworkObjectProvider providerComponent =
                Object.FindFirstObjectByType<ZenjectNetworkObjectProvider>();

            Assert.That(sceneLifecycle, Is.TypeOf<TechnicalPrototypeContextInitializer>());
            Assert.That(objectProvider, Is.SameAs(providerComponent));
            AssertTechnicalPrototypeHierarchy(sceneContext, providerComponent);
            Assert.That(sceneContext.Container.Resolve<IClassFactory>(), Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<ILocalInputReader>(),
                Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IScreenNavigationSystem>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IPopupSystem>(), Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<IMovementSimulationService>(),
                Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<IMovementCollisionStrategy>(),
                Is.TypeOf<LevelCollisionService>());
            Assert.That(sceneContext.Container.Resolve<IMovementSystem>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IDamageableSystem>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IDamageSourceSystem>(), Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<List<INetworkEntityFactory>>(),
                Has.Exactly(1).TypeOf<PlayerNetworkEntityFactory>());
            Assert.That(Camera.main, Is.Not.Null,
                "The technical prototype needs a camera to clear UI Toolkit and IMGUI frames.");

            AssertNetworkBootstrapIsReady();

            Assert.That(ProjectContext.Instance, Is.SameAs(projectContext));
            Assert.That(Object.FindFirstObjectByType<ApplicationEntryPoint>(), Is.Null,
                "The temporary Preloader entry point must be destroyed after startup.");
        }

        [UnityTest]
        public IEnumerator NavigationRecreatesTechnicalPrototypeSceneContext()
        {
            yield return LoadPreloaderAndWaitForTechnicalPrototype();

            ProjectContext projectContext = ProjectContext.Instance;
            ISceneFlowService sceneFlowService =
                projectContext.Container.Resolve<ISceneFlowService>();
            ILoadingScreenSystem loadingScreenSystem =
                projectContext.Container.Resolve<ILoadingScreenSystem>();
            SceneContext firstSceneContext = Object.FindFirstObjectByType<SceneContext>();

            UniTask<Result> navigationOperation =
                sceneFlowService.LoadAsync<TechnicalPrototypeScene, EmptySceneSettings>(
                    EmptySceneSettings.Instance,
                    CancellationToken.None);

            Assert.That(loadingScreenSystem.IsVisible, Is.True);

            Result navigationResult = null;
            yield return navigationOperation.ToCoroutine(result => navigationResult = result);

            SceneContext secondSceneContext = Object.FindFirstObjectByType<SceneContext>();

            Assert.That(navigationResult, Is.Not.Null);
            Assert.That(navigationResult.IsSuccess, Is.True);
            Assert.That(secondSceneContext, Is.Not.Null);
            Assert.That(secondSceneContext, Is.Not.SameAs(firstSceneContext));
            Assert.That(ProjectContext.Instance, Is.SameAs(projectContext));
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_TechnicalPrototype"));
            Assert.That(sceneFlowService.IsTransitioning, Is.False);
            Assert.That(loadingScreenSystem.IsVisible, Is.False);
            Assert.That(loadingScreenSystem.IsTransitioning, Is.False);

            AssertNetworkBootstrapIsReady();
        }

        private static IEnumerator LoadPreloaderAndWaitForTechnicalPrototype()
        {
            AsyncOperation loadPreloader =
                SceneManager.LoadSceneAsync("Scene_Preloader", LoadSceneMode.Single);

            while (!loadPreloader.isDone)
            {
                yield return null;
            }

            ProjectContext projectContext = ProjectContext.Instance;
            ISceneFlowService sceneFlowService =
                projectContext.Container.Resolve<ISceneFlowService>();
            ILoadingScreenSystem loadingScreenSystem =
                projectContext.Container.Resolve<ILoadingScreenSystem>();

            float timeout = 10f;
            while ((sceneFlowService.CurrentSceneDefinition == null ||
                    loadingScreenSystem.IsVisible ||
                    loadingScreenSystem.IsTransitioning) &&
                   timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.That(timeout, Is.GreaterThan(0f),
                "Application startup did not complete within the timeout.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_TechnicalPrototype"));
        }

        private static void AssertNetworkBootstrapIsReady()
        {
            FusionBootstrap bootstrap = Object.FindFirstObjectByType<FusionBootstrap>();
            FusionBootstrapDebugGUI debugGUI =
                Object.FindFirstObjectByType<FusionBootstrapDebugGUI>();
            NetworkRunner runner = Object.FindFirstObjectByType<NetworkRunner>();

            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(debugGUI, Is.Not.Null);
            Assert.That(runner, Is.Not.Null);
            Assert.That(bootstrap.RunnerPrefab, Is.SameAs(runner));
            Assert.That(bootstrap.StartMode, Is.EqualTo(FusionBootstrap.StartModes.UserInterface));
            Assert.That(bootstrap.CurrentStage, Is.EqualTo(FusionBootstrap.Stage.Disconnected));
            Assert.That(bootstrap.DefaultRoomName, Is.Empty);
            Assert.That(debugGUI.enabled, Is.True);

            NetworkSceneManagerDefault sceneManager =
                runner.GetComponent<NetworkSceneManagerDefault>();

            Assert.That(sceneManager, Is.Not.Null);
            Assert.That(sceneManager.IsSceneTakeOverEnabled, Is.True);
            Assert.That(runner.GetComponent<NetworkCallbacksDebuggerComponent>(), Is.Not.Null);
            Assert.That(HasComponentNamed(runner.gameObject, "NetworkEvents"), Is.True);
            Assert.That(HasComponentNamed(runner.gameObject, "RunnerEnableVisibility"), Is.True);
            Assert.That(HasComponentNamed(runner.gameObject, "RunnerSimulatePhysics3D"), Is.True);
        }

        private static void AssertTechnicalPrototypeHierarchy(
            SceneContext sceneContext,
            ZenjectNetworkObjectProvider providerComponent)
        {
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            string[] expectedRootNames =
            {
                "===== CONTEXT =====",
                "===== NETWORK =====",
                "===== CAMERAS =====",
                "===== UI =====",
                "===== GAMEPLAY =====",
                "===== ENVIRONMENT =====",
            };

            Assert.That(roots.Length, Is.EqualTo(expectedRootNames.Length));

            for (int i = 0; i < expectedRootNames.Length; i++)
            {
                Assert.That(roots[i].name, Is.EqualTo(expectedRootNames[i]));
            }

            Transform contextRoot = roots[0].transform;
            Transform networkRoot = roots[1].transform;
            Transform camerasRoot = roots[2].transform;
            Transform uiRoot = roots[3].transform;
            Transform gameplayRoot = roots[4].transform;
            Transform environmentRoot = roots[5].transform;

            AssertDirectChildren(contextRoot, "SceneContext", "SceneFeatures");
            AssertDirectChildren(
                networkRoot,
                "NetworkBootstrap",
                "NetworkRunner",
                "NetworkObjectProvider");
            AssertDirectChildren(camerasRoot, "Main Camera");
            AssertDirectChildren(
                uiRoot,
                "EventSystem",
                "ScreenNavigationUI",
                "PopupUI");
            AssertDirectChildren(gameplayRoot, "Player");
            AssertDirectChildren(environmentRoot, "Level");

            Transform sceneFeatures = contextRoot.GetChild(1);
            Transform screenNavigationUI = uiRoot.GetChild(1);
            Transform popupUI = uiRoot.GetChild(2);
            Transform player = gameplayRoot.GetChild(0);
            Transform playerSpawnController = player.GetChild(0);
            Transform spawnPoints = player.GetChild(1);
            Transform level = environmentRoot.GetChild(0);

            Assert.That(sceneContext.transform, Is.SameAs(contextRoot.GetChild(0)));
            Assert.That(providerComponent.transform, Is.SameAs(networkRoot.GetChild(2)));
            Assert.That(
                sceneFeatures.GetComponent<TechnicalPrototypeContextInstaller>(),
                Is.Not.Null);
            Assert.That(sceneFeatures.GetComponent<InputFeature>(), Is.Not.Null);
            Assert.That(sceneFeatures.GetComponent<ScreenNavigationFeature>(), Is.Not.Null);
            Assert.That(sceneFeatures.GetComponent<PopupFeature>(), Is.Not.Null);
            Assert.That(sceneContext.GetComponent<TechnicalPrototypeContextInstaller>(), Is.Null);
            Assert.That(sceneContext.GetComponent<InputFeature>(), Is.Null);
            Assert.That(sceneContext.GetComponent<ScreenNavigationFeature>(), Is.Null);
            Assert.That(sceneContext.GetComponent<PopupFeature>(), Is.Null);
            Assert.That(
                HasComponentNamed(screenNavigationUI.gameObject, "UIDocument"),
                Is.True);
            Assert.That(
                HasComponentNamed(screenNavigationUI.gameObject, "ScreenNavigationComponent"),
                Is.True);
            Assert.That(HasComponentNamed(popupUI.gameObject, "UIDocument"), Is.True);
            Assert.That(HasComponentNamed(popupUI.gameObject, "PopupComponent"), Is.True);
            AssertDirectChildren(player, "PlayerSpawnController", "SpawnPoints", "Entities");
            Assert.That(
                playerSpawnController.GetComponent<PlayerSpawnComponent>(),
                Is.Not.Null);
            Assert.That(
                playerSpawnController.GetComponent<PlayerNetworkInputComponent>(),
                Is.Not.Null);
            Assert.That(
                HasComponentNamed(playerSpawnController.gameObject, "NetworkObject"),
                Is.True);
            AssertDirectChildren(
                spawnPoints,
                "SpawnPoint_01",
                "SpawnPoint_02",
                "SpawnPoint_03",
                "SpawnPoint_04");
            AssertDirectChildren(level, "Visual", "Collision");
        }

        private static void AssertDirectChildren(Transform parent, params string[] childNames)
        {
            Assert.That(parent.childCount, Is.EqualTo(childNames.Length));

            for (int i = 0; i < childNames.Length; i++)
            {
                Assert.That(parent.GetChild(i).name, Is.EqualTo(childNames[i]));
            }
        }

        private static bool HasComponentNamed(GameObject gameObject, string componentTypeName)
        {
            Component[] components = gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].GetType().Name == componentTypeName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
