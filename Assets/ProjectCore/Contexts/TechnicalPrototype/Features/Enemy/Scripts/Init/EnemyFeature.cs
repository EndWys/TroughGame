using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private Transform _entitiesContainer;
        [SerializeField] private EnemySpawnComponent _spawnComponent;

        protected override void InstallBindings()
        {
            if (_entitiesContainer == null)
            {
                throw new InvalidOperationException("Enemy entities container must be configured.");
            }

            BindFromInstance<Transform, Transform>(
                _entitiesContainer, EnemyNetworkEntityConstants.EntitiesContainer);
            BindFromInstance<EnemySpawnComponent, EnemySpawnComponent>(_spawnComponent);
            BindAsSingle<INetworkEntityFactory, EnemyNetworkEntityFactory>();
            BindInterfacesAndSelfAsSingle<EnemyCheatHandler>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_spawnComponent == null)
            {
                throw new InvalidOperationException("Enemy spawn component must be configured.");
            }

            _spawnComponent.ValidateConfiguration();
            Resolve<EnemyCheatHandler>().Initialize();
            return UniTask.CompletedTask;
        }
    }
}
