using System;

namespace Domain
{
    public interface IColleague<out TColleague, out TEventKey>  
        where TColleague : IColleague<TColleague, TEventKey>
        where TEventKey : Enum
    {
        public void Init(IMediator<TColleague, TEventKey> mediator);
    }
}