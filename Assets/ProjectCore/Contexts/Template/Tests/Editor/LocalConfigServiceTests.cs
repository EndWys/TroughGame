using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class LocalConfigServiceTests
    {
        private readonly List<UnityEngine.Object> _createdObjects =
            new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _createdObjects.Count; i++)
            {
                UnityEngine.Object.DestroyImmediate(_createdObjects[i]);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void ChildContextOverridesLocalConfigAndFallsBackToParent()
        {
            ParentOnlyConfig parentOnlyConfig = CreateConfig<ParentOnlyConfig>();
            SharedConfig parentSharedConfig = CreateConfig<SharedConfig>();
            SharedConfig childSharedConfig = CreateConfig<SharedConfig>();
            LocalConfigCatalogConfig parentCatalog =
                CreateCatalog(parentOnlyConfig, parentSharedConfig);
            LocalConfigCatalogConfig childCatalog = CreateCatalog(childSharedConfig);

            var parentContainer = new DiContainer();
            BindLocalConfigService(parentContainer);
            LocalConfigService parentService = parentContainer.Resolve<LocalConfigService>();
            parentService.Initialize(parentCatalog);

            DiContainer childContainer = parentContainer.CreateSubContainer();
            BindLocalConfigService(childContainer);
            LocalConfigService childService = childContainer.Resolve<LocalConfigService>();
            childService.Initialize(childCatalog);

            Assert.AreSame(childSharedConfig, childService.GetRequiredConfig<SharedConfig>());
            Assert.AreSame(parentOnlyConfig, childService.GetRequiredConfig<ParentOnlyConfig>());
            Assert.IsFalse(childService.TryGetConfig(out MissingConfig _));
        }

        [Test]
        public void InitializationRejectsDuplicateConfigTypes()
        {
            LocalConfigCatalogConfig catalog = CreateCatalog(
                CreateConfig<SharedConfig>(),
                CreateConfig<SharedConfig>());
            var service = new LocalConfigService(null);

            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => service.Initialize(catalog));

            StringAssert.Contains(nameof(SharedConfig), exception.Message);
        }

        private static void BindLocalConfigService(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<LocalConfigService>().AsSingle();
        }

        private TConfig CreateConfig<TConfig>() where TConfig : BaseLocalConfig
        {
            TConfig config = ScriptableObject.CreateInstance<TConfig>();
            _createdObjects.Add(config);
            return config;
        }

        private LocalConfigCatalogConfig CreateCatalog(params BaseLocalConfig[] configs)
        {
            LocalConfigCatalogConfig catalog =
                ScriptableObject.CreateInstance<LocalConfigCatalogConfig>();
            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty configsProperty = serializedCatalog.FindProperty("_configs");
            configsProperty.arraySize = configs.Length;

            for (int i = 0; i < configs.Length; i++)
            {
                configsProperty.GetArrayElementAtIndex(i).objectReferenceValue = configs[i];
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            _createdObjects.Add(catalog);
            return catalog;
        }

        private sealed class ParentOnlyConfig : BaseLocalConfig
        {
        }

        private sealed class SharedConfig : BaseLocalConfig
        {
        }

        private sealed class MissingConfig : BaseLocalConfig
        {
        }
    }
}
