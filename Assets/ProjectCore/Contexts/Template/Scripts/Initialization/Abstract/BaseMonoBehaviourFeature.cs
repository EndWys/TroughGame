using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    public abstract class BaseMonoBehaviourFeature : MonoBehaviour, IBaseFeature
    {
        private readonly FeatureLifecycleAdapter _lifecycleAdapter =
            new FeatureLifecycleAdapter();

        public void InstallBindings(DiContainer container)
        {
            _lifecycleAdapter.InstallBindings(GetType(), container, InstallBindings);
        }

        UniTask IBaseFeature.InitializeAsync(CancellationToken cancellationToken)
        {
            return _lifecycleAdapter.InitializeAsync(
                GetType(),
                cancellationToken,
                InitializeAsync);
        }

        protected abstract void InstallBindings();

        protected virtual UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindAsSingle<T>() where T : class
        {
            return _lifecycleAdapter.BindAsSingle<T>();
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindAsSingle<TContract, TImplementation>()
            where TImplementation : class, TContract
        {
            return _lifecycleAdapter.BindAsSingle<TContract, TImplementation>();
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindInterfacesAndSelfAsSingle<T>() where T : class
        {
            return _lifecycleAdapter.BindInterfacesAndSelfAsSingle<T>();
        }

        protected ConcreteIdArgConditionCopyNonLazyBinder BindInterfacesAndSelfFromComponentInHierarchyAsSingle<T>()
            where T : Component
        {
            return _lifecycleAdapter.BindInterfacesAndSelfFromComponentInHierarchyAsSingle<T>();
        }

        protected T Resolve<T>() where T : class
        {
            return _lifecycleAdapter.Resolve<T>();
        }

        protected TImplementation ResolveAs<TContract, TImplementation>()
            where TContract : class
            where TImplementation : class, TContract
        {
            return _lifecycleAdapter.ResolveAs<TContract, TImplementation>();
        }
    }
}
