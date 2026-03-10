using Godot;

namespace CountyIdle.Systems;

public sealed class HexAtlas5x4
{
    public const int Columns = 5;
    public const int Rows = 4;

    private static readonly Vector2[] HexUvTemplate =
    [
        new Vector2(0.5f, 0.0f),
        new Vector2(1.0f, 0.25f),
        new Vector2(1.0f, 0.75f),
        new Vector2(0.5f, 1.0f),
        new Vector2(0.0f, 0.75f),
        new Vector2(0.0f, 0.25f)
    ];

    private static readonly int[] DefaultColumnStarts = [32, 578, 1123, 1669, 2215];
    private static readonly int[] DefaultColumnWidths = [507, 506, 507, 507, 506];
    private static readonly int[] DefaultRowStarts = [20, 403, 786, 1168];
    private static readonly int[] DefaultRowHeights = [344, 343, 342, 341];
    private static readonly Vector2 DefaultAtlasSize = new(2752f, 1536f);

    private readonly Rect2[,] _regions = new Rect2[Rows, Columns];
    private readonly Vector2[][] _uvs = new Vector2[Rows * Columns][];

    private HexAtlas5x4(Texture2D texture)
    {
        Texture = texture;
        BuildRegions(texture);
        BuildUvs();
    }

    public Texture2D Texture { get; }

    public static HexAtlas5x4? TryLoad(string path)
    {
        var resourceTexture = TryLoadImportedTexture(path);
        if (resourceTexture != null)
        {
            return new HexAtlas5x4(resourceTexture);
        }

        if (!FileAccess.FileExists(path))
        {
            GD.PushWarning($"HexAtlas5x4 missing texture: {path}");
            return null;
        }

        var bytes = FileAccess.GetFileAsBytes(path);
        var image = new Image();
        var error = LoadImageFromBuffer(image, bytes);
        if (error != Error.Ok)
        {
            GD.PushWarning($"HexAtlas5x4 failed to load texture: {path} ({error})");
            return null;
        }

        var imageTexture = ImageTexture.CreateFromImage(image);
        return new HexAtlas5x4(imageTexture);
    }

    private static Texture2D? TryLoadImportedTexture(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        return ResourceLoader.Load<Texture2D>(path);
    }

    public Rect2 GetRegion(int column, int row)
    {
        return _regions[row, column];
    }

    public Vector2[] GetUv(int column, int row)
    {
        return _uvs[(row * Columns) + column];
    }

    private void BuildRegions(Texture2D texture)
    {
        var width = texture.GetWidth();
        var height = texture.GetHeight();
        if (Mathf.IsEqualApprox(width, DefaultAtlasSize.X) && Mathf.IsEqualApprox(height, DefaultAtlasSize.Y))
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    _regions[row, column] = new Rect2(
                        DefaultColumnStarts[column],
                        DefaultRowStarts[row],
                        DefaultColumnWidths[column],
                        DefaultRowHeights[row]);
                }
            }

            return;
        }

        var cellWidth = width / (float)Columns;
        var cellHeight = height / (float)Rows;
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                _regions[row, column] = new Rect2(
                    column * cellWidth,
                    row * cellHeight,
                    cellWidth,
                    cellHeight);
            }
        }
    }

    private void BuildUvs()
    {
        var atlasWidth = Texture.GetWidth();
        var atlasHeight = Texture.GetHeight();
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                var region = _regions[row, column];
                var uv = new Vector2[HexUvTemplate.Length];
                for (var index = 0; index < HexUvTemplate.Length; index++)
                {
                    var normalized = HexUvTemplate[index];
                    var pixelX = region.Position.X + (region.Size.X * normalized.X);
                    var pixelY = region.Position.Y + (region.Size.Y * normalized.Y);
                    uv[index] = new Vector2(pixelX / atlasWidth, pixelY / atlasHeight);
                }

                _uvs[(row * Columns) + column] = uv;
            }
        }
    }

    private static Error LoadImageFromBuffer(Image image, byte[] bytes)
    {
        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
            bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
        {
            return image.LoadPngFromBuffer(bytes);
        }

        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return image.LoadJpgFromBuffer(bytes);
        }

        if (bytes.Length >= 12 &&
            bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
        {
            return image.LoadWebpFromBuffer(bytes);
        }

        return Error.FileCorrupt;
    }
}
