using System;
using Cysharp.Threading.Tasks;

namespace ProjectCore.Domain.Scripts.Paterns.Mediator
{
    public interface IMediator<in TColleague, in TEventKey> 
        where TColleague : IColleague<TColleague, TEventKey> 
        where TEventKey : Enum
    {
        public void Notify(TColleague sender, TEventKey eventKey, object args = null);
    }
}