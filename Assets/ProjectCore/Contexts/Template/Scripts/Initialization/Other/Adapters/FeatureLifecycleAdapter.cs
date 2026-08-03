using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    internal sealed class FeatureLifecycleAdapter
    {
        private DiContainer _container;
        private bool _isInstalled;

        public void InstallBindings(
            Type featureType,
            DiContainer container,
            Action installBindings)
        {
            if (_isInstalled)
            {
                throw new InvalidOperationException(
                    $"{featureType.Name} bindings have already been installed.");
            }

            _container = container ?? throw new ArgumentNullException(nameof(container));
            installBindings();
            _isInstalled = true;
        }

        public UniTask InitializeAsync(
            Type featureType,
            CancellationToken cancellationToken,
            Func<CancellationToken, UniTask> initializeAsync)
        {
            if (!_isInstalled)
            {
                throw new InvalidOperationException(
                    $"{featureType.Name} must be installed before initialization.");
            }

            return initializeAsync(cancellationToken);
        }

        public ConcreteIdArgConditionCopyNonLazyBinder BindAsSingle<T>() where T : class
        {
            return _container.Bind<T>().AsSingle();
        }

        public ConcreteIdArgConditionCopyNonLazyBinder BindAsSingle<TContract, TImplementation>()
            where TImplementation : class, TContract
        {
            return _container.Bind<TContract>().To<TImplementation>().AsSingle();
        }

        public ConcreteIdArgConditionCopyNonLazyBinder BindInterfacesAndSelfAsSingle<T>() where T : class
        {
            return _container.BindInterfacesAndSelfTo<T>().AsSingle();
        }

        public ConcreteIdArgConditionCopyNonLazyBinder BindInterfacesAndSelfFromComponentInHierarchyAsSingle<T>()
            where T : Component
        {
            return _container.BindInterfacesAndSelfTo<T>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public TImplementation ResolveAs<TContract, TImplementation>()
            where TContract : class
            where TImplementation : class, TContract
        {
            return Resolve<TContract>() as TImplementation
                ?? throw new InvalidOperationException(
                    $"The {typeof(TContract).Name} binding is not {typeof(TImplementation).Name}.");
        }
    }
}
