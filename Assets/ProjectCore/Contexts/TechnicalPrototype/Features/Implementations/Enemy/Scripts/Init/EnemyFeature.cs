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
            BindAsSingle<INetworkEntityFactory, EnemyNetworkEntityFactory>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_spawnComponent == null)
            {
                throw new InvalidOperationException("Enemy spawn component must be configured.");
            }

            _spawnComponent.ValidateConfiguration();
            return UniTask.CompletedTask;
        }
    }
}
