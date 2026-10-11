namespace ProjectCore.GameCore
{
    public abstract class BaseEntityInputSourceComponent<TInputFrame> :
        BaseNetworkEntityComponent
        where TInputFrame : struct
    {
        public abstract bool TryGetInput(out TInputFrame input);
    }
}
