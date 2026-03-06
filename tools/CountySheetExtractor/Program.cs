using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  extract <sourcePath> <outputDir> [threshold] [minArea] [contactCellSize] [contactColumns]");
    return;
}

var command = args[0].Trim().ToLowerInvariant();
switch (command)
{
    case "extract":
        RunExtract(args);
        break;
    case "build-runtime-atlas":
        RunBuildRuntimeAtlas(args);
        break;
    default:
        throw new InvalidOperationException($"Unknown command: {command}");
}

static void RunExtract(string[] args)
{
    var sourcePath = args.Length > 1 ? args[1] : throw new ArgumentException("Missing sourcePath");
    var outputDir = args.Length > 2 ? args[2] : throw new ArgumentException("Missing outputDir");
    var threshold = args.Length > 3 && int.TryParse(args[3], out var parsedThreshold) ? parsedThreshold : 18;
    var minArea = args.Length > 4 && int.TryParse(args[4], out var parsedArea) ? parsedArea : 400;
    var contactCellSize = args.Length > 5 && int.TryParse(args[5], out var parsedCellSize) ? parsedCellSize : 256;
    var contactColumns = args.Length > 6 && int.TryParse(args[6], out var parsedColumns) ? parsedColumns : 8;

    Directory.CreateDirectory(outputDir);
    var tilesDir = Path.Combine(outputDir, "tiles");
    Directory.CreateDirectory(tilesDir);

    using var bitmap = new Bitmap(sourcePath);
    var width = bitmap.Width;
    var height = bitmap.Height;
    var background = EstimateBackground(bitmap);

    var foreground = new bool[width * height];
    var visited = new bool[foreground.Length];

    for (var y = 0; y < height; y++)
    {
        for (var x = 0; x < width; x++)
        {
            var color = bitmap.GetPixel(x, y);
            foreground[Flatten(x, y, width)] = color.A > 7 && ColorDistance(color, background) > threshold;
        }
    }

    var components = ExtractComponents(foreground, visited, width, height, minArea)
        .OrderBy(component => component.MinY)
        .ThenBy(component => component.MinX)
        .ToList();

    var tileMetadata = new List<TileMetadata>();
    for (var index = 0; index < components.Count; index++)
    {
        var component = components[index];
        var tileId = index + 1;
        var tileFileName = $"tile_{tileId:D3}.png";
        var tilePath = Path.Combine(tilesDir, tileFileName);
        using var crop = bitmap.Clone(component.ToRectangle(), PixelFormat.Format32bppArgb);
        TransparentizeBackground(crop, background, threshold);
        crop.Save(tilePath, ImageFormat.Png);

        tileMetadata.Add(new TileMetadata(
            tileId,
            $"tiles/{tileFileName}",
            [component.MinX, component.MinY, component.Width, component.Height],
            component.PixelArea));
    }

    var metadata = new ExtractionMetadata(
        Path.GetFullPath(sourcePath),
        [width, height],
        [background.R, background.G, background.B],
        threshold,
        minArea,
        tileMetadata.Count,
        tileMetadata);

    var metadataPath = Path.Combine(outputDir, "tilemap_metadata.json");
    File.WriteAllText(
        metadataPath,
        JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

    var contactRows = (int)Math.Ceiling(tileMetadata.Count / (double)contactColumns);
    using var contactSheet = new Bitmap(contactColumns * contactCellSize, Math.Max(contactRows, 1) * contactCellSize, PixelFormat.Format32bppArgb);
    var labelFontFamily = SystemFonts.CaptionFont?.FontFamily ?? FontFamily.GenericSansSerif;
    using (var graphics = Graphics.FromImage(contactSheet))
    using (var labelBrush = new SolidBrush(Color.White))
    using (var labelFont = new Font(labelFontFamily, 12f, FontStyle.Bold))
    {
        graphics.Clear(Color.FromArgb(255, 32, 32, 32));
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        foreach (var tile in tileMetadata)
        {
            var tilePath = Path.Combine(outputDir, tile.File.Replace('/', Path.DirectorySeparatorChar));
            using var tileBitmap = new Bitmap(tilePath);

            var slotIndex = tile.Id - 1;
            var column = slotIndex % contactColumns;
            var row = slotIndex / contactColumns;
            var originX = column * contactCellSize;
            var originY = row * contactCellSize;

            var maxDrawWidth = contactCellSize - 18;
            var maxDrawHeight = contactCellSize - 34;
            var scale = Math.Min(maxDrawWidth / (float)tileBitmap.Width, maxDrawHeight / (float)tileBitmap.Height);
            var drawWidth = (int)Math.Round(tileBitmap.Width * scale);
            var drawHeight = (int)Math.Round(tileBitmap.Height * scale);
            var drawX = originX + (contactCellSize - drawWidth) / 2;
            var drawY = originY + 8 + (maxDrawHeight - drawHeight) / 2;

            graphics.DrawImage(tileBitmap, new Rectangle(drawX, drawY, drawWidth, drawHeight));
            graphics.DrawString(tile.Id.ToString("D3"), labelFont, labelBrush, originX + 8, originY + contactCellSize - 24);
        }
    }

    var contactSheetPath = Path.Combine(outputDir, "tilemap_contact_sheet.png");
    contactSheet.Save(contactSheetPath, ImageFormat.Png);

    Console.WriteLine($"Extracted {tileMetadata.Count} components");
    Console.WriteLine($"Metadata: {metadataPath}");
    Console.WriteLine($"Contact sheet: {contactSheetPath}");
}

static void RunBuildRuntimeAtlas(string[] args)
{
    var extractDir = args.Length > 1 ? args[1] : throw new ArgumentException("Missing extractDir");
    var outputDir = args.Length > 2 ? args[2] : throw new ArgumentException("Missing outputDir");
    var atlasCellSize = args.Length > 3 && int.TryParse(args[3], out var parsedCellSize) ? parsedCellSize : 128;
    var atlasColumns = args.Length > 4 && int.TryParse(args[4], out var parsedColumns) ? parsedColumns : 6;

    Directory.CreateDirectory(outputDir);

    var mapping = new Dictionary<string, int>
    {
        ["grass_0"] = 21,
        ["grass_1"] = 22,
        ["grass_2"] = 25,
        ["grass_3"] = 35,
        ["courtyard_0"] = 17,
        ["courtyard_1"] = 23,
        ["courtyard_2"] = 44,
        ["water_0"] = 42,
        ["road_0"] = 11,
        ["road_1"] = 15,
        ["road_2"] = 16,
        ["road_3"] = 13,
        ["road_4"] = 15,
        ["road_5"] = 8,
        ["road_6"] = 6,
        ["road_7"] = 5,
        ["road_8"] = 16,
        ["road_9"] = 14,
        ["road_10"] = 9,
        ["road_11"] = 7,
        ["road_12"] = 8,
        ["road_13"] = 5,
        ["road_14"] = 7,
        ["road_15"] = 6,
        ["prop_bush"] = 54,
        ["prop_flowers"] = 55,
        ["prop_rock_small"] = 39,
        ["prop_rock_large"] = 36,
        ["prop_stump"] = 60,
        ["prop_lilies"] = 56,
        ["prop_reed"] = 57,
        ["prop_stepping"] = 48,
        ["accent_plaza"] = 46,
        ["accent_border"] = 43
    };

    var keys = mapping.Keys.ToList();
    var atlasRows = (int)Math.Ceiling(keys.Count / (double)atlasColumns);
    using var atlas = new Bitmap(atlasColumns * atlasCellSize, atlasRows * atlasCellSize, PixelFormat.Format32bppArgb);
    using (var graphics = Graphics.FromImage(atlas))
    {
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        for (var index = 0; index < keys.Count; index++)
        {
            var key = keys[index];
            var tileId = mapping[key];
            var tilePath = Path.Combine(extractDir, "tiles", $"tile_{tileId:D3}.png");
            if (!File.Exists(tilePath))
            {
                throw new FileNotFoundException($"Missing extracted tile for {key}", tilePath);
            }

            using var tile = new Bitmap(tilePath);
            var column = index % atlasColumns;
            var row = index / atlasColumns;
            var slotX = column * atlasCellSize;
            var slotY = row * atlasCellSize;
            var margin = 6;
            var maxDrawWidth = atlasCellSize - (margin * 2);
            var maxDrawHeight = atlasCellSize - (margin * 2);
            var scale = Math.Min(maxDrawWidth / (float)tile.Width, maxDrawHeight / (float)tile.Height);
            var drawWidth = (int)Math.Round(tile.Width * scale);
            var drawHeight = (int)Math.Round(tile.Height * scale);
            var drawX = slotX + (atlasCellSize - drawWidth) / 2;
            var drawY = slotY + (atlasCellSize - drawHeight) / 2;

            graphics.DrawImage(tile, new Rectangle(drawX, drawY, drawWidth, drawHeight));
        }
    }

    var atlasPath = Path.Combine(outputDir, "county_reference_isometric_atlas.png");
    atlas.Save(atlasPath, ImageFormat.Png);

    var manifestTiles = new Dictionary<string, object>();
    for (var index = 0; index < keys.Count; index++)
    {
        var key = keys[index];
        var column = index % atlasColumns;
        var row = index / atlasColumns;
        manifestTiles[key] = new
        {
            source_tile_id = mapping[key],
            atlas_coord = new[] { column, row },
            pixel_region = new[] { column * atlasCellSize, row * atlasCellSize, atlasCellSize, atlasCellSize }
        };
    }

    var manifest = new
    {
        source_image = "res://assets/picture/Isometric_game_asset_sheet_topdown_view_zen_chines_delpmaspu.png",
        extracted_from = Path.GetFullPath(extractDir),
        tile_pixel_size = new[] { atlasCellSize, atlasCellSize },
        render_anchor = new[] { atlasCellSize / 2, (int)Math.Round(atlasCellSize * 0.648f) },
        logical_tile_size = new[] { 32, 16 },
        tile_shape = "isometric",
        tile_layout = "diamond_down",
        tiles = manifestTiles
    };

    var manifestPath = Path.Combine(outputDir, "county_reference_isometric_manifest.json");
    File.WriteAllText(
        manifestPath,
        JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

    var tileSetLines = new List<string>
    {
        "[gd_resource type=\"TileSet\" format=3]",
        "",
        "[ext_resource type=\"Texture2D\" path=\"res://assets/tiles/county_reference_isometric/county_reference_isometric_atlas.png\" id=\"1_atlas\"]",
        "",
        "[sub_resource type=\"TileSetAtlasSource\" id=\"TileSetAtlasSource_source\"]",
        "texture = ExtResource(\"1_atlas\")",
        $"texture_region_size = Vector2i({atlasCellSize}, {atlasCellSize})"
    };

    for (var index = 0; index < keys.Count; index++)
    {
        var column = index % atlasColumns;
        var row = index / atlasColumns;
        tileSetLines.Add($"{column}:{row}/0 = 0");
    }

    tileSetLines.Add("");
    tileSetLines.Add("[resource]");
    tileSetLines.Add("tile_size = Vector2i(32, 16)");
    tileSetLines.Add("tile_shape = 1");
    tileSetLines.Add("tile_layout = 5");
    tileSetLines.Add("tile_offset_axis = 0");
    tileSetLines.Add("sources/0 = SubResource(\"TileSetAtlasSource_source\")");

    var tileSetPath = Path.Combine(outputDir, "CountyReferenceIsometricTileSet.tres");
    File.WriteAllLines(tileSetPath, tileSetLines);

    Console.WriteLine($"Runtime atlas: {atlasPath}");
    Console.WriteLine($"Runtime manifest: {manifestPath}");
    Console.WriteLine($"Runtime tileset: {tileSetPath}");
}

static Color EstimateBackground(Bitmap bitmap)
{
    var samples = new[]
    {
        bitmap.GetPixel(0, 0),
        bitmap.GetPixel(bitmap.Width - 1, 0),
        bitmap.GetPixel(0, bitmap.Height - 1),
        bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1),
        bitmap.GetPixel(bitmap.Width / 2, 0)
    };

    var avgR = (int)Math.Round(samples.Average(sample => sample.R));
    var avgG = (int)Math.Round(samples.Average(sample => sample.G));
    var avgB = (int)Math.Round(samples.Average(sample => sample.B));
    return Color.FromArgb(255, avgR, avgG, avgB);
}

static int Flatten(int x, int y, int width) => y * width + x;

static int ColorDistance(Color left, Color right) =>
    Math.Abs(left.R - right.R) + Math.Abs(left.G - right.G) + Math.Abs(left.B - right.B);

static IReadOnlyList<ComponentBox> ExtractComponents(bool[] foreground, bool[] visited, int width, int height, int minArea)
{
    var components = new List<ComponentBox>();
    var neighbors = new (int Dx, int Dy)[]
    {
        (-1, -1), (0, -1), (1, -1),
        (-1, 0),           (1, 0),
        (-1, 1),  (0, 1),  (1, 1)
    };

    for (var y = 0; y < height; y++)
    {
        for (var x = 0; x < width; x++)
        {
            var startIndex = Flatten(x, y, width);
            if (!foreground[startIndex] || visited[startIndex])
            {
                continue;
            }

            var queue = new Queue<int>();
            queue.Enqueue(startIndex);
            visited[startIndex] = true;

            var minX = x;
            var maxX = x;
            var minY = y;
            var maxY = y;
            var pixelArea = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentX = current % width;
                var currentY = current / width;

                if (!foreground[current])
                {
                    continue;
                }

                pixelArea++;
                minX = Math.Min(minX, currentX);
                maxX = Math.Max(maxX, currentX);
                minY = Math.Min(minY, currentY);
                maxY = Math.Max(maxY, currentY);

                foreach (var (dx, dy) in neighbors)
                {
                    var nextX = currentX + dx;
                    var nextY = currentY + dy;
                    if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                    {
                        continue;
                    }

                    var nextIndex = Flatten(nextX, nextY, width);
                    if (visited[nextIndex] || !foreground[nextIndex])
                    {
                        continue;
                    }

                    visited[nextIndex] = true;
                    queue.Enqueue(nextIndex);
                }
            }

            if (pixelArea >= minArea)
            {
                components.Add(new ComponentBox(minX, minY, maxX, maxY, pixelArea));
            }
        }
    }

    return components;
}

static void TransparentizeBackground(Bitmap bitmap, Color background, int threshold)
{
    var transparencyThreshold = Math.Max(threshold + 18, 46);
    for (var y = 0; y < bitmap.Height; y++)
    {
        for (var x = 0; x < bitmap.Width; x++)
        {
            var color = bitmap.GetPixel(x, y);
            if (color.A <= 7)
            {
                bitmap.SetPixel(x, y, Color.Transparent);
                continue;
            }

            if (ColorDistance(color, background) <= transparencyThreshold)
            {
                bitmap.SetPixel(x, y, Color.Transparent);
            }
        }
    }
}

internal sealed record ComponentBox(int MinX, int MinY, int MaxX, int MaxY, int PixelArea)
{
    public int Width => MaxX - MinX + 1;
    public int Height => MaxY - MinY + 1;

    public Rectangle ToRectangle() => new(MinX, MinY, Width, Height);
}

internal sealed record TileMetadata(int Id, string File, int[] SourceBboxXywh, int PixelArea);

internal sealed record ExtractionMetadata(
    string SourceImage,
    int[] SourceSize,
    int[] EstimatedBackgroundRgb,
    int ForegroundThreshold,
    int MinComponentArea,
    int TileCount,
    IReadOnlyList<TileMetadata> Tiles);
