using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
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

            // SceneFlow reports a destination only after the gameplay scene
            // lifecycle has completed its initialization phase.
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_Prototype"));
            Assert.That(sceneFlowService.CurrentSceneDefinition, Is.Not.Null);

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
            SceneContext firstSceneContext = Object.FindFirstObjectByType<SceneContext>();

            // A gameplay scene must provide its own SceneContext. It is scoped
            // to the loaded scene and is not a ProjectContext singleton.
            Assert.That(firstSceneContext, Is.Not.Null);

            // Reloading through the typed SceneFlow API validates payload
            // binding and scene lifecycle initialization as one transition.
            yield return sceneFlowService.LoadAsync<PrototypeScene, EmptySceneSettings>(
                    EmptySceneSettings.Instance,
                    CancellationToken.None)
                .ToCoroutine();

            SceneContext secondSceneContext = Object.FindFirstObjectByType<SceneContext>();

            // The old scene scope must be replaced by a new one, while the
            // application scope remains alive and keeps the same instance.
            Assert.That(sceneFlowService.IsTransitioning, Is.False);
            Assert.That(sceneFlowService.CurrentSceneDefinition, Is.Not.Null);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_Prototype"));
            Assert.That(secondSceneContext, Is.Not.Null);
            Assert.That(secondSceneContext, Is.Not.SameAs(firstSceneContext));
            Assert.That(ProjectContext.Instance, Is.SameAs(projectContext));
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

            float timeout = 10f;
            while (sceneFlowService.CurrentSceneDefinition == null && timeout > 0f)
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
