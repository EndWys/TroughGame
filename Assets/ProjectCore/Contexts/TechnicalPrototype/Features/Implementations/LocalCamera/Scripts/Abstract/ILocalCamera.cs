namespace ProjectCore.TechnicalPrototype
{
    public interface ILocalCamera
    {
        float ZoomFactor { get; }

        void SetZoomFactor(float zoomFactor);
    }
}
