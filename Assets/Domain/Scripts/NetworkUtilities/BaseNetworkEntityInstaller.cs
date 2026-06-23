using Fusion;
using UnityEngine;
using Zenject;

namespace Domain
{
    public abstract class BaseNetworkEntityInstaller<TNetworkEntity> : MonoInstaller where TNetworkEntity : NetworkBehaviour
    {
        [field: SerializeReference] private TNetworkEntity _networkEntity;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<TNetworkEntity>()
                .FromInstance(_networkEntity)
                .AsCached();

            BindAdditionalComponents();
        }
        
        protected abstract void BindAdditionalComponents();

        protected void BindComponentInChildren<T>()
        {
            Container.Bind<T>().FromComponentInChildren().AsCached();
        }

        protected void BindComponentFromInstance<T>(T instance)
        {
            Container.Bind<T>().FromInstance(instance).AsCached();
        }
    }
}
