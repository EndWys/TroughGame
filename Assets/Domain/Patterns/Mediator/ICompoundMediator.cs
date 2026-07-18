using System.Collections.Generic;

namespace Domain
{
    public interface ICompoundMediator : IMediator
    {
        protected List<MemberHandler> Handlers { get; }
    }
}
