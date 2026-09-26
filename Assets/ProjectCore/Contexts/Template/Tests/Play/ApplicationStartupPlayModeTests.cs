using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private NetworkRunner _testRunner;

        [UnityTest]
        public IEnumerator SoloEnemySpawnRegistersParentsAndClearsOnlyEnemies()
        {
            yield return LoadPreloaderAndWaitForTechnicalPrototype();

            SceneContext sceneContext = Object.FindFirstObjectByType<SceneContext>();
            NetworkEntityRegistry registry = sceneContext.Container.Resolve<NetworkEntityRegistry>();
            EnemySpawnComponent spawn = Object.FindFirstObjectByType<EnemySpawnComponent>();
            Transform enemyContainer = sceneContext.Container.ResolveId<Transform>(
                EnemyNetworkEntityConstants.EntitiesContainer);
            Transform playerContainer = sceneContext.Container.ResolveId<Transform>(
                PlayerNetworkEntityConstants.EntitiesContainer);

            spawn.SpawnEnemy();
            Assert.That(registry.GetByType(EnemyNetworkEntityConstants.Enemy), Is.Empty);

            _testRunner = Object.FindFirstObjectByType<NetworkRunner>();
            var start = _testRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Single,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = _testRunner.GetComponent<INetworkSceneManager>(),
                ObjectProvider = sceneContext.Container.Resolve<INetworkObjectProvider>(),
            });

            float deadline = Time.realtimeSinceStartup + 15f;
            while (!start.IsCompleted && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(start.IsCompleted, Is.True, "Solo runner did not start within 15 seconds.");
            Assert.That(start.GetAwaiter().GetResult().Ok, Is.True);
            yield return null;

            Assert.That(spawn.Runner, Is.SameAs(_testRunner));
            IReadOnlyList<BaseNetworkEntityRoot> players =
                registry.GetByType(PlayerNetworkEntityConstants.Player);
            Assert.That(players.Count, Is.EqualTo(1));
            BaseNetworkEntityRoot player = players[0];
            Assert.That(player.transform.parent, Is.SameAs(playerContainer));

            spawn.SpawnEnemy();
            spawn.SpawnEnemy();

            BaseNetworkEntityRoot[] enemies = registry.GetByType(EnemyNetworkEntityConstants.Enemy).ToArray();
            Assert.That(enemies.Length, Is.EqualTo(2));
            float inputDeadline = Time.realtimeSinceStartup + 5f;

            while (enemies.Any(enemy =>
                       !enemy.TryGetEntityComponent(out EnemyInputSourceComponent inputSource) ||
                       !inputSource.TryGetInput(out _)) &&
                   Time.realtimeSinceStartup < inputDeadline)
            {
                yield return null;
            }

            Assert.That(
                Time.realtimeSinceStartup,
                Is.LessThan(inputDeadline),
                "Enemy behaviour did not produce input within five seconds.");
            Assert.That(enemies[0].EntityId, Is.Not.EqualTo(enemies[1].EntityId));
            NetworkEntityIdData[] firstIds = enemies.Select(enemy => enemy.EntityId).ToArray();
            Vector3 expectedPosition = spawn.transform.parent.Find("SpawnPoints/SpawnPoint_01").position;

            foreach (BaseNetworkEntityRoot enemy in enemies)
            {
                Assert.That(enemy, Is.TypeOf<EnemyNetworkEntityComponent>());
                var enemyComponent = (EnemyNetworkEntityComponent)enemy;
                Assert.That(
                    enemy.TryGetEntityComponent(out EnemyInputSourceComponent inputSource),
                    Is.True);
                Assert.That(inputSource, Is.Not.Null);
                Assert.That(inputSource.TryGetInput(out EnemyInputFrameData input), Is.True);
                Assert.That(input.Direction, Is.EqualTo(Vector2.zero));
                Assert.That(
                    enemy.TryGetEntityComponent(out EnemyBehaviourStateMachine behaviourStateMachine),
                    Is.True);
                Assert.That(behaviourStateMachine, Is.Not.Null);
                Assert.That(enemy.transform.parent, Is.SameAs(enemyContainer));
                Assert.That(
                    Vector3.Distance(enemy.transform.position, expectedPosition), Is.LessThan(0.001f));
                Assert.That(enemy.Object.InputAuthority, Is.EqualTo(PlayerRef.None));
                Assert.That(registry.TryGet(enemy.EntityId, out BaseNetworkEntityRoot registered), Is.True);
                Assert.That(registered, Is.SameAs(enemy));
                Assert.That(
                    enemyComponent.CurrentBehaviourState,
                    Is.EqualTo(EnemyBehaviourStateType.Waiting));
                Assert.That(enemyComponent.TargetEntityId, Is.EqualTo(player.EntityId));
            }

            yield return null;
            spawn.DespawnEnemies();
            spawn.DespawnEnemies();
            yield return null;

            Assert.That(registry.GetByType(EnemyNetworkEntityConstants.Enemy), Is.Empty);
            Assert.That(enemyContainer.childCount, Is.Zero);
            Assert.That(registry.GetByType(PlayerNetworkEntityConstants.Player), Has.Count.EqualTo(1));
            Assert.That(player.Object.IsValid, Is.True);
            foreach (NetworkEntityIdData id in firstIds)
            {
                Assert.That(registry.TryGet(id, out _), Is.False);
            }

            spawn.SpawnEnemy();
            BaseNetworkEntityRoot nextEnemy = registry.GetByType(EnemyNetworkEntityConstants.Enemy).Single();
            Assert.That(firstIds, Has.No.Member(nextEnemy.EntityId));
            Assert.That(nextEnemy.transform.parent, Is.SameAs(enemyContainer));
            spawn.DespawnEnemies();
        }

        [UnityTearDown]
        public IEnumerator ShutdownTestRunner()
        {
            if (_testRunner != null)
            {
                var shutdown = _testRunner.Shutdown();
                while (!shutdown.IsCompleted)
                {
                    yield return null;
                }

                shutdown.GetAwaiter().GetResult();
                _testRunner = null;
            }
        }

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
            Assert.That(
                sceneContext.Container.Resolve<List<INetworkEntityFactory>>(),
                Has.Exactly(1).TypeOf<EnemyNetworkEntityFactory>());
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
            AssertDirectChildren(gameplayRoot, "Player", "Enemy");
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
            Assert.That(sceneFeatures.GetComponent<PlayerFeature>(), Is.Not.Null);
            Assert.That(sceneFeatures.GetComponent<EnemyFeature>(), Is.Not.Null);
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
            Transform enemy = gameplayRoot.GetChild(1);
            AssertDirectChildren(enemy, "EnemySpawnController", "SpawnPoints", "Entities");
            Assert.That(enemy.GetChild(0).GetComponent<EnemySpawnComponent>(), Is.Not.Null);
            Assert.That(HasComponentNamed(enemy.GetChild(0).gameObject, "NetworkObject"), Is.True);
            AssertDirectChildren(enemy.GetChild(1), "SpawnPoint_01");
            Assert.That(
                sceneContext.Container.ResolveId<Transform>(PlayerNetworkEntityConstants.EntitiesContainer),
                Is.SameAs(player.Find("Entities")));
            Assert.That(
                sceneContext.Container.ResolveId<Transform>(EnemyNetworkEntityConstants.EntitiesContainer),
                Is.SameAs(enemy.Find("Entities")));
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
