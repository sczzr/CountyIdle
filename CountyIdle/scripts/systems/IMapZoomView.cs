namespace CountyIdle.Systems;

public interface IMapZoomView
{
    float Zoom { get; }
    float MinZoom { get; }
    float MaxZoom { get; }
    float DefaultZoom { get; }

    void SetZoom(float zoom);
    void AdjustZoom(float delta);
}
