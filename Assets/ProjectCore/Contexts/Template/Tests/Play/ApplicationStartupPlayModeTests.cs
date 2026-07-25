using System.Collections;
using NUnit.Framework;
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
            AsyncOperation loadPreloader =
                SceneManager.LoadSceneAsync("Scene_Preloader", LoadSceneMode.Single);

            while (!loadPreloader.isDone)
            {
                yield return null;
            }

            ProjectContext projectContext = ProjectContext.Instance;
            IApplicationFlowCoordinator coordinator =
                projectContext.Container.Resolve<IApplicationFlowCoordinator>();

            float timeout = 10f;
            while (!coordinator.IsSceneReady && timeout > 0f)
            {
                timeout -= UnityEngine.Time.deltaTime;
                yield return null;
            }

            Assert.That(timeout, Is.GreaterThan(0f),
                "Application startup did not complete within the timeout.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Scene_Prototype"));
            Assert.That(coordinator.CurrentSceneName, Is.EqualTo("Scene_Prototype"));
            Assert.That(ProjectContext.Instance, Is.SameAs(projectContext));
        }
    }
}
