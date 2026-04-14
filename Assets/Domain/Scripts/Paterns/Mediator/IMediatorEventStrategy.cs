using System;

namespace ProjectCore.Domain.Scripts.Paterns.Mediator
{
    public interface IMediatorEventStrategy<in TColleague,out TEventKey> 
        where TColleague : IColleague<TColleague, TEventKey>
        where TEventKey : Enum
    {
        public TEventKey EventKey { get; }
        public void Handle(TColleague sender, object args);
    }
}