using System;
using System.Collections.Generic;
using Fusion;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntityComponentLifecycleProcessor
    {
        public bool IsInitialized { get; private set; }

        public bool HasValidNetworkContext(NetworkBehaviour parent)
        {
            return parent != null && parent.Object != null && parent.Object.IsValid && parent.Runner != null;
        }

        public void Initialize(Action initialize, Action reset)
        {
            if (initialize == null) throw new ArgumentNullException(nameof(initialize));
            if (reset == null) throw new ArgumentNullException(nameof(reset));

            Reset(reset);
            try
            {
                initialize();
                IsInitialized = true;
            }
            catch (Exception initializationError)
            {
                try
                {
                    Reset(reset);
                }
                catch (Exception cleanupError)
                {
                    throw new AggregateException(initializationError, cleanupError);
                }

                throw;
            }
        }

        public void Reset(Action reset)
        {
            IsInitialized = false;
            if (reset == null) throw new ArgumentNullException(nameof(reset));
            reset();
        }

        internal static void ResetComponents(IReadOnlyList<INetworkEntityComponent> components)
        {
            List<Exception> errors = null;
            for (int index = components.Count - 1; index >= 0; index--)
            {
                try
                {
                    components[index].ResetLocalRuntime();
                }
                catch (Exception error)
                {
                    errors ??= new List<Exception>();
                    errors.Add(error);
                }
            }

            if (errors != null) throw new AggregateException("Entity component cleanup failed.", errors);
        }
    }
}
