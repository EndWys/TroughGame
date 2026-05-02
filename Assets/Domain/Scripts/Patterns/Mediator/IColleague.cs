using System;

namespace Domain
{
    public interface IColleague<out TColleague, out TEventKey>  
        where TColleague : IColleague<TColleague, TEventKey>
        where TEventKey : Enum
    {
        public void SetMediator(IMediator<TColleague, TEventKey> mediator);
    }
}