using System;
using System.Collections.Generic;

namespace ProjectCore.Domain.Scripts.Paterns.Mediator
{
    public interface ICompoundMediator<TColleague, TEventKey>  : IMediator<TColleague, TEventKey> 
        where TColleague : IColleague<TColleague, TEventKey> 
        where TEventKey : Enum
    {
        protected Dictionary<TEventKey, IMediatorEventStrategy<TColleague,TEventKey>> Strategies { get; }
    }
}