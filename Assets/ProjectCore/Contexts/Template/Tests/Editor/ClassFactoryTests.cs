using NUnit.Framework;
using System.Collections.Generic;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class ClassFactoryTests
    {
        [Test]
        public void CreateUsesDependenciesFromOwningContainer()
        {
            var projectContainer = CreateContainer("Project");
            var sceneContainer = CreateContainer("Scene");

            // Each context binds its own ClassFactory as a singleton.
            // The factory must create runtime objects from the container that owns it,
            // not from a global or manually swapped container.
            var projectFactory = projectContainer.Resolve<IClassFactory>();
            var sceneFactory = sceneContainer.Resolve<IClassFactory>();

            var projectCreated = projectFactory.Create<ClassFactoryTestConsumer>();
            var sceneCreated = sceneFactory.Create<ClassFactoryTestConsumer>();

            Assert.AreEqual("Project", projectCreated.Dependency.ContextName);
            Assert.AreEqual("Scene", sceneCreated.Dependency.ContextName);
        }

        [Test]
        public void CreateByBaseClassCreatesAllConcreteAssignableTypes()
        {
            var container = CreateContainer("Template");
            var factory = container.Resolve<IClassFactory>();

            // This mirrors the platform behavior used for runtime-selected subclasses.
            // The factory scans the base type assembly and creates every concrete child
            // through Zenject so constructor injection still works.
            List<BaseClassFactoryTestHandler> handlers = factory.CreateByBaseClass<BaseClassFactoryTestHandler>();

            Assert.AreEqual(2, handlers.Count);
            Assert.IsTrue(handlers.Exists(handler => handler.GetType() == typeof(FirstClassFactoryTestHandler)));
            Assert.IsTrue(handlers.Exists(handler => handler.GetType() == typeof(SecondClassFactoryTestHandler)));
            Assert.IsTrue(handlers.TrueForAll(handler => handler.Dependency.ContextName == "Template"));
        }

        [Test]
        public void BindingFactoryUsesSameContextScopedFactoryInstance()
        {
            var container = CreateContainer("Template");

            // ObjectFactoryFeature binds ClassFactory through all implemented interfaces.
            // Creation and runtime binding are separate contracts, but they must still
            // point to the same context-owned factory instance and therefore the same container.
            var classFactory = container.Resolve<IClassFactory>();
            var bindingFactory = container.Resolve<IContainerBindingFactory>();
            var instance = new ClassFactoryBoundTestService();

            bindingFactory.BindFromInstanceAsSingle(instance);

            Assert.AreSame(classFactory, bindingFactory);
            Assert.AreSame(instance, container.Resolve<ClassFactoryBoundTestService>());
        }

        private static DiContainer CreateContainer(string contextName)
        {
            var container = new DiContainer();

            container.Bind<ClassFactoryTestDependency>()
                .FromInstance(new ClassFactoryTestDependency(contextName))
                .AsSingle();

            container.BindInterfacesAndSelfTo<ClassFactory>()
                .AsSingle();

            return container;
        }

        private sealed class ClassFactoryTestDependency
        {
            public ClassFactoryTestDependency(string contextName)
            {
                ContextName = contextName;
            }

            public string ContextName { get; }
        }

        private sealed class ClassFactoryTestConsumer
        {
            public ClassFactoryTestConsumer(ClassFactoryTestDependency dependency)
            {
                Dependency = dependency;
            }

            public ClassFactoryTestDependency Dependency { get; }
        }

        private sealed class ClassFactoryBoundTestService
        {
        }

        private abstract class BaseClassFactoryTestHandler
        {
            protected BaseClassFactoryTestHandler(ClassFactoryTestDependency dependency)
            {
                Dependency = dependency;
            }

            public ClassFactoryTestDependency Dependency { get; }
        }

        private sealed class FirstClassFactoryTestHandler : BaseClassFactoryTestHandler
        {
            public FirstClassFactoryTestHandler(ClassFactoryTestDependency dependency)
                : base(dependency)
            {
            }
        }

        private sealed class SecondClassFactoryTestHandler : BaseClassFactoryTestHandler
        {
            public SecondClassFactoryTestHandler(ClassFactoryTestDependency dependency)
                : base(dependency)
            {
            }
        }
    }
}
