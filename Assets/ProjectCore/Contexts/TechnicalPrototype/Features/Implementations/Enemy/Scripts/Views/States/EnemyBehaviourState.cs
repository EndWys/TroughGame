using Domain;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public abstract class EnemyBehaviourState : MonoBehaviour,
        IState<EnemyBehaviourStateType, EnemyBehaviourPayload>
    {
        public virtual void Init() { }

        public virtual void Enter() { }

        public abstract EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload);

        public virtual void Exit() { }
    }
}
