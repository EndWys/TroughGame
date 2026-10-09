using System.Collections.Generic;

namespace Shared
{
    public interface IComposite<out TComponent>
    {
        IReadOnlyList<TComponent> Components { get; }
    }
}
