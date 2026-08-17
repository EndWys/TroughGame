using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;
using NUnit.Framework;
using ProjectCore.Preloader;
using ProjectCore.Prototype;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class ApplicationStartupPlayModeTests
    {
        [UnityTest]
        public IEnumerator PreloaderBootstrapsPrototypeAndPreservesProjectContext()
        {
            // This is the application happy path: the only entry scene is the
            // Preloader, which initializes the application and then navigates
            // to the first gameplay scene.
            yield return LoadPreloaderAndWaitForPrototype();

            ProjectContext projectContext = ProjectContext.Instance;
            ISceneFlowService sceneFlowService =
                projectContext.Container.Resolve<ISceneFlowService>();
            SceneContext sceneContext = Object.FindFirstObjectByType<SceneContext>();

            Assert.That(sceneContext, Is.Not.Null);

            IScreenNavigationSystem screenNavigationSystem =
                sceneContext.Container.Resolve<IScreenNavigationSystem>();
            IPopupSystem popupSystem = sceneContext.Container.Resolve<IPopupSystem>();

            // SceneFlow reports a destination only after the gameplay scene
            // lifecycle has completed its initialization phase.
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_Prototype"));
            Assert.That(sceneFlowService.CurrentSceneDefinition, Is.Not.Null);

            // Prototype initialization opens the scene-owned HUD through the
            // ScreenNavigation lifecycle rather than a Unity callback.
            Assert.That(
                screenNavigationSystem.CurrentScreenType,
                Is.EqualTo(typeof(PrototypeGameHUDScreenView)));
            Assert.That(screenNavigationSystem.CanGoBack, Is.False);
            Assert.That(screenNavigationSystem.IsTransitioning, Is.False);

            // A popup remains pending until its own UI completes it or an
            // external emergency abort interrupts the flow. Aborting must
            // produce a structured failure and finish cleanup before returning.
            UniTask<Result<PrototypeConfirmationResponses>> popupOperation =
                popupSystem.OpenAsync<
                    PrototypeConfirmationPopupView,
                    PrototypeConfirmationPopupPayload,
                    PrototypeConfirmationResponses>(
                    new PrototypeConfirmationPopupPayload("UI lifecycle test"),
                    CancellationToken.None);

            yield return null;
            yield return popupSystem
                .AbortAsync<PrototypeConfirmationPopupView>()
                .ToCoroutine();

            Result<PrototypeConfirmationResponses> popupResult = null;
            yield return popupOperation.ToCoroutine(result => popupResult = result);

            Assert.That(popupResult, Is.Not.Null);
            Assert.That(popupResult.IsFailure, Is.True);
            Assert.That(
                popupResult.FirstError.Code,
                Is.EqualTo(PopupErrors.Aborted(
                    typeof(PrototypeConfirmationPopupView)).Code));

            // ProjectContext belongs to the whole application and must survive
            // the transition out of the temporary Preloader scene.
            Assert.That(ProjectContext.Instance, Is.SameAs(projectContext));

            // ApplicationEntryPoint is owned by PreloaderContext. Its absence
            // proves that temporary startup objects do not leak into gameplay.
            Assert.That(Object.FindFirstObjectByType<ApplicationEntryPoint>(), Is.Null,
                "The temporary Preloader entry point must be destroyed after startup.");
        }

        [UnityTest]
        public IEnumerator NavigationRecreatesSceneContextAndPreservesProjectContext()
        {
            // Start from a clean application entry path so this test also uses
            // the same initialization contract as a real player session.
            yield return LoadPreloaderAndWaitForPrototype();

            ProjectContext projectContext = ProjectContext.Instance;
            ISceneFlowService sceneFlowService =
                projectContext.Container.Resolve<ISceneFlowService>();
            ILoadingScreenSystem loadingScreenSystem =
                projectContext.Container.Resolve<ILoadingScreenSystem>();
            SceneContext firstSceneContext = Object.FindFirstObjectByType<SceneContext>();

            // A gameplay scene must provide its own SceneContext. It is scoped
            // to the loaded scene and is not a ProjectContext singleton.
            Assert.That(firstSceneContext, Is.Not.Null);

            // The persistent loading screen must become visible before the old
            // scene exits and stay alive while the scene-owned UI is destroyed.
            UniTask<Result> navigationOperation =
                sceneFlowService.LoadAsync<PrototypeScene, EmptySceneSettings>(
                    EmptySceneSettings.Instance,
                    CancellationToken.None);

            Assert.That(loadingScreenSystem.IsVisible, Is.True);

            Result navigationResult = null;
            yield return navigationOperation.ToCoroutine(
                result => navigationResult = result);

            SceneContext secondSceneContext = Object.FindFirstObjectByType<SceneContext>();

            Assert.That(secondSceneContext, Is.Not.Null);

            IScreenNavigationSystem secondScreenNavigationSystem =
                secondSceneContext.Container.Resolve<IScreenNavigationSystem>();

            // The old scene scope must be replaced by a new one, while the
            // application scope remains alive and keeps the same instance.
            Assert.That(navigationResult, Is.Not.Null);
            Assert.That(navigationResult.IsSuccess, Is.True);
            Assert.That(sceneFlowService.IsTransitioning, Is.False);
            Assert.That(sceneFlowService.CurrentSceneDefinition, Is.Not.Null);
            Assert.That(loadingScreenSystem.IsVisible, Is.False);
            Assert.That(loadingScreenSystem.IsTransitioning, Is.False);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_Prototype"));
            Assert.That(secondSceneContext, Is.Not.SameAs(firstSceneContext));
            Assert.That(ProjectContext.Instance, Is.SameAs(projectContext));
            Assert.That(
                secondScreenNavigationSystem.CurrentScreenType,
                Is.EqualTo(typeof(PrototypeGameHUDScreenView)));
        }

        private static IEnumerator LoadPreloaderAndWaitForPrototype()
        {
            // Every Play Mode scenario enters through the single application
            // entry point. Loading Preloader explicitly prevents tests from
            // depending on the scene left by a previous test.
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
                // Initialization is asynchronous. The timeout protects the
                // test runner from hanging when startup fails to reach Ready.
                timeout -= Time.deltaTime;
                yield return null;
            }

            // The helper is intentionally shared by both tests so all startup
            // assertions begin from the same fully initialized state.
            Assert.That(timeout, Is.GreaterThan(0f),
                "Application startup did not complete within the timeout.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_Prototype"));
        }
    }
}
