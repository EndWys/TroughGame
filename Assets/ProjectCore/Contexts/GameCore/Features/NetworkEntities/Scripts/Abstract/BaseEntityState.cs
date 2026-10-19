using System;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.GameCore
{
    public abstract class BaseEntityState<TStateType, TPayload> :
        BaseProcessorsBasedMonoBehaviourState<TStateType, TPayload>
        where TStateType : struct, Enum
        where TPayload : struct
    {
        private IClassFactory _classFactory;

        [Inject]
        private void ConstructBaseEntityState(IClassFactory classFactory)
        {
            _classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        }

        public virtual void Restore() { }

        protected TProcessor CreateProcessor<TProcessor>(params object[] parameters)
        {
            return _classFactory.Create<TProcessor>(parameters);
        }
    }
}
