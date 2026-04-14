using System;

namespace Domain
{
    public interface IMediator<in TColleague, in TEventKey> 
        where TColleague : IColleague<TColleague, TEventKey> 
        where TEventKey : Enum
    {
        public void Notify(TColleague sender, TEventKey eventKey, object args = null);
    }
}