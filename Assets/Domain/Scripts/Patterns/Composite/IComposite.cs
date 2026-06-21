using System.Collections.Generic;

namespace Domain
{
    public interface IComposite<out TComponent>
    {
        IReadOnlyList<TComponent> Components { get; }
    }
}
