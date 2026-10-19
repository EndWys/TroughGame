using Shared;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(
        true,
        sourceNamespace: "ProjectCore.TechnicalPrototype",
        sourceAssembly: "ProjectCore.Runtime",
        sourceClassName: "EnemyBehaviourState")]
    public abstract class BaseEnemyBehaviourState : MonoBehaviour,
        IState<EnemyBehaviourStateType, EnemyBehaviourPayload>
    {
        public virtual void Init()
        {
        }

        public virtual void Enter()
        {
        }

        public abstract EnemyBehaviourStateType Tick(EnemyBehaviourPayload payload);

        public virtual void Exit()
        {
        }
    }
}
