using System;

namespace Domain
{
    public interface IState<out TStatesType, in TStatePayload>
        where TStatesType : Enum
        where TStatePayload : struct
    {
        public void Enter();

        public TStatesType Tick(TStatePayload payload);

        public void Exit();
    }
}
