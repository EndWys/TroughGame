namespace ProjectCore.Template
{
    internal interface IDebugConsoleController
    {
        public void Initialize(DebugConsoleSettings settings);
        public void Attach(IDebugConsoleView view);
        public void Detach(IDebugConsoleView view);
        public void RefreshDebugVisualization();
    }
}
