using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyNetworkEntityComponent : BaseNetworkEntityRoot
    {
        private Transform _entitiesContainer;

        [Inject]
        private void Construct(
            [Inject(Id = EnemyNetworkEntityConstants.EntitiesContainer)] Transform entitiesContainer)
        {
            _entitiesContainer = entitiesContainer != null
                ? entitiesContainer
                : throw new ArgumentNullException(nameof(entitiesContainer));
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return Array.Empty<INetworkEntityComponent>();
        }

        protected override void AfterComponentsInitialized()
        {
            transform.SetParent(_entitiesContainer, true);
        }
    }
}
