#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace ProjectCore.Template
{
    public static class SceneDefinitionAuthoringValidation
    {
        public static bool TryValidateRegistration(
            SceneAsset scene, SceneCatalogConfig catalog, bool hasDefinition,
            int definitionTypeCount, out string error)
        {
            error = scene == null ? SceneDefinitionAuthoringErrors.SceneRequired
                : catalog == null ? SceneDefinitionAuthoringErrors.CatalogRequired
                : !hasDefinition && definitionTypeCount == 0
                    ? SceneDefinitionAuthoringErrors.DefinitionTypeRequired : null;
            return error == null;
        }

        public static bool TryValidateNewScene(string directory, string fileName, out string error)
        {
            Match folder = Regex.Match(directory ?? string.Empty,
                @"^Assets/ProjectCore/Contexts/([A-Za-z0-9]+)/(?:Features/[A-Za-z0-9]+/)?Assets/Scenes(?:/[A-Za-z0-9]+)*$");
            if (!folder.Success || !AssetDatabase.IsValidFolder(directory))
            {
                error = SceneDefinitionAuthoringErrors.SceneDirectoryRequired;
                return false;
            }

            string name = fileName ?? string.Empty;
            if (name.EndsWith(".unity", StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - ".unity".Length);
            }

            if (Path.GetFileName(name) != name ||
                !Regex.IsMatch(name, "^Scene_" + Regex.Escape(folder.Groups[1].Value) + @"(?:_[A-Za-z0-9]+)*$"))
            {
                error = SceneDefinitionAuthoringErrors.SceneNameRequired;
                return false;
            }

            error = null;
            return true;
        }
    }
}
#endif
