using NUnit.Framework;

namespace ProjectCore.Template
{
    public sealed class SceneDefinitionAuthoringTests
    {
        private const string SceneDirectory =
            "Assets/ProjectCore/Contexts/TechnicalPrototype/Assets/Scenes";

        [TestCase("Scene_TechnicalPrototype", true)]
        [TestCase("Scene_TechnicalPrototype.unity", true)]
        [TestCase("Scene_TechnicalPrototype_Example", true)]
        [TestCase("Scene_Project", false)]
        [TestCase("NewScene", false)]
        [TestCase("../Scene_TechnicalPrototype", false)]
        [TestCase("", false)]
        public void NewSceneNameMustMatchSelectedContext(string name, bool expected)
        {
            bool actual = SceneDefinitionAuthoringValidation.TryValidateNewScene(
                SceneDirectory, name, out string error);

            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(error, Is.EqualTo(expected ? null : SceneDefinitionAuthoringErrors.SceneNameRequired));
        }

        [TestCase("")]
        [TestCase("Assets")]
        [TestCase("Assets/ProjectCore/Contexts/Prototype/GraphicResources/Scenes")]
        [TestCase("Assets/ProjectCore/Contexts/TechnicalPrototype/Assets/Missing/Scenes")]
        public void NewSceneRequiresAnExistingOwnedSceneFolder(string directory)
        {
            Assert.That(SceneDefinitionAuthoringValidation.TryValidateNewScene(
                directory, "Scene_TechnicalPrototype", out string error), Is.False);
            Assert.That(error, Is.EqualTo(SceneDefinitionAuthoringErrors.SceneDirectoryRequired));
        }
    }
}
