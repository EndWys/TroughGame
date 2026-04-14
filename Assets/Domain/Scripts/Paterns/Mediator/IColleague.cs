using System;

namespace ProjectCore.Domain.Scripts.Paterns.Mediator
{
    public interface IColleague<out TColleague, out TEventKey>  
        where TColleague : IColleague<TColleague, TEventKey>
        where TEventKey : Enum
    {
        public void Initialize(IMediator<TColleague, TEventKey> mediator);
    }
}