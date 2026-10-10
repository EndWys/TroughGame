using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private Transform _entitiesContainer;
        [SerializeField] private PlayerSpawnComponent _spawnComponent;

        protected override void InstallBindings()
        {
            PlayerSpawnValidation.ValidateEntitiesContainer(_entitiesContainer);

            BindFromInstance<Transform, Transform>(
                _entitiesContainer, PlayerNetworkEntityConstants.EntitiesContainer);
            BindAsSingle<INetworkEntityFactory, PlayerNetworkEntityFactory>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            PlayerSpawnValidation.ValidateConfiguration(_spawnComponent);
            return UniTask.CompletedTask;
        }
    }
}
