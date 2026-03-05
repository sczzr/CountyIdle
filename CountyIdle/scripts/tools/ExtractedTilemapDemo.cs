using Godot;

namespace CountyIdle.Tools;

public partial class ExtractedTilemapDemo : Node2D
{
    private const string TileMapLayerPath = "TileMapLayer";
    private const string CameraPath = "Camera2D";
    private const int TileCount = 53;
    private const int AtlasColumns = 8;
    private const int RenderColumns = 10;
    private const int TileSize = 256;

    public override void _Ready()
    {
        var tileMapLayer = GetNode<TileMapLayer>(TileMapLayerPath);
        tileMapLayer.Clear();

        for (var i = 0; i < TileCount; i++)
        {
            var atlasX = i % AtlasColumns;
            var atlasY = i / AtlasColumns;
            var cellX = i % RenderColumns;
            var cellY = i / RenderColumns;

            tileMapLayer.SetCell(
                coords: new Vector2I(cellX, cellY),
                sourceId: 0,
                atlasCoords: new Vector2I(atlasX, atlasY),
                alternativeTile: 0);
        }

        var rows = Mathf.CeilToInt((float)TileCount / RenderColumns);
        var centerX = RenderColumns * TileSize * 0.5f;
        var centerY = rows * TileSize * 0.5f;
        var camera = GetNodeOrNull<Camera2D>(CameraPath);
        if (camera != null)
        {
            camera.Position = new Vector2(centerX, centerY);
            camera.Zoom = new Vector2(0.35f, 0.35f);
        }
    }
}
