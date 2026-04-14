using System;

namespace Domain
{
    public interface IMediatorEventStrategy<in TColleague,out TEventKey> 
        where TColleague : IColleague<TColleague, TEventKey>
        where TEventKey : Enum
    {
        public TEventKey EventKey { get; }
        public void Handle(TColleague sender, object args);
    }
}