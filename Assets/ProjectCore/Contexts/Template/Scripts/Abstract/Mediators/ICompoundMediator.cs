using Shared;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public interface ICompoundMediator : IMediator
    {
        protected List<BaseMemberHandler> Handlers { get; }
    }
}
