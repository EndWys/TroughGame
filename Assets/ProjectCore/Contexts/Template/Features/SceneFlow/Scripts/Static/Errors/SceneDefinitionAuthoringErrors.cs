#if UNITY_EDITOR
namespace ProjectCore.Template
{
    public static class SceneDefinitionAuthoringErrors
    {
        public const string SceneRequired = "Assign a Unity scene.";
        public const string CatalogRequired = "Assign a SceneCatalogConfig.";
        public const string DefinitionTypeRequired = "Create a concrete BaseSceneDefinition type first.";
        public const string SceneDirectoryRequired = "Choose an existing context or feature Assets/Scenes folder.";
        public const string SceneNameRequired = "Use Scene_<Context>[_<Detail>] and match the selected folder's context.";
    }
}
#endif
