using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain
{
    public abstract class BaseProcessorsBasedMonoBehaviourState<TStatesType, TStatePayload> :
        MonoBehaviour,
        IState<TStatesType, TStatePayload>
        where TStatesType : Enum
        where TStatePayload : struct
    {
        private IReadOnlyList<IStateProcessor<TStatesType, TStatePayload>> _processors;

        public virtual void Init()
        {
            _processors = CreateProcessors();
        }

        public abstract void Enter();

        public virtual TStatesType Tick(TStatePayload payload)
        {
            foreach (IStateProcessor<TStatesType, TStatePayload> processor in _processors)
            {
                if (processor.Execute(payload, out TStatesType resultState))
                {
                    return resultState;
                }
            }

            return FallbackState;
        }

        public abstract void Exit();

        protected abstract IReadOnlyList<IStateProcessor<TStatesType, TStatePayload>> CreateProcessors();

        protected abstract TStatesType FallbackState { get; }
    }
}
