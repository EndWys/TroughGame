using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    public abstract class BaseFeature : IBaseFeature
    {
        private bool _isInstalled;

        private DiContainer Container { get; set; }

        public void InstallBindings(DiContainer container)
        {
            if (_isInstalled)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} bindings have already been installed.");
            }

            Container = container ?? throw new ArgumentNullException(nameof(container));
            InstallBindings();
            _isInstalled = true;
        }

        UniTask IBaseFeature.InitializeAsync(CancellationToken cancellationToken)
        {
            if (!_isInstalled)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} must be installed before initialization.");
            }

            return InitializeAsync(cancellationToken);
        }

        protected abstract void InstallBindings();

        protected virtual UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindAsSingle<T>() where T : class
        {
            return Container.Bind<T>().AsSingle();
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindAsSingle<TContract, TImplementation>()
            where TImplementation : class, TContract
        {
            return Container.Bind<TContract>().To<TImplementation>().AsSingle();
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindInterfacesAndSelfAsSingle<T>() where T : class
        {
            return Container.BindInterfacesAndSelfTo<T>().AsSingle();
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindInterfacesAndSelfFromComponentInHierarchyAsSingle<T>()
            where T : Component
        {
            return Container.BindInterfacesAndSelfTo<T>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        protected T Resolve<T>() where T : class
        {
            return Container.Resolve<T>();
        }

        protected TImplementation ResolveAs<TContract, TImplementation>()
            where TContract : class
            where TImplementation : class, TContract
        {
            return Resolve<TContract>() as TImplementation
                ?? throw new InvalidOperationException(
                    $"The {typeof(TContract).Name} binding is not {typeof(TImplementation).Name}.");
        }
    }
}
