using System;
using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerFeature : BaseMonoBehaviourFeature
    {
        [SerializeField] private Transform _entitiesContainer;

        protected override void InstallBindings()
        {
            if (_entitiesContainer == null)
            {
                throw new InvalidOperationException("Player entities container must be configured.");
            }

            BindFromInstance<Transform, Transform>(
                _entitiesContainer, PlayerNetworkEntityConstants.EntitiesContainer);
            BindAsSingle<INetworkEntityFactory, PlayerNetworkEntityFactory>();
        }
    }
}
