using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace CountyIdle.Systems;

public sealed class MapLayerAtlasManifest
{
    public Vector2 TilePixelSize { get; private init; }
    public Vector2 RenderAnchor { get; private init; }

    public Dictionary<string, MapLayerAtlasDefinition> Atlases { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, MapLayerAtlasTileDefinition> Tiles { get; } = new(StringComparer.Ordinal);

    public static MapLayerAtlasManifest? TryLoad(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            return null;
        }

        try
        {
            var content = FileAccess.GetFileAsString(path);
            var dto = JsonSerializer.Deserialize<MapLayerAtlasManifestDto>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            if (dto == null)
            {
                return null;
            }

            var tilePixelSize = ParseVector2(dto.TilePixelSize);
            var renderAnchor = ParseVector2(dto.RenderAnchor);
            if (tilePixelSize == Vector2.Zero || renderAnchor == Vector2.Zero)
            {
                return null;
            }

            var manifest = new MapLayerAtlasManifest
            {
                TilePixelSize = tilePixelSize,
                RenderAnchor = renderAnchor
            };

            if (dto.Atlases != null)
            {
                foreach (var atlasDto in dto.Atlases)
                {
                    if (string.IsNullOrWhiteSpace(atlasDto.AtlasName) ||
                        string.IsNullOrWhiteSpace(atlasDto.SourceImage))
                    {
                        continue;
                    }

                    manifest.Atlases[atlasDto.AtlasName] = new MapLayerAtlasDefinition(
                        atlasDto.AtlasName,
                        atlasDto.SourceImage,
                        ParseVector2OrFallback(atlasDto.TilePixelSize, tilePixelSize),
                        ParseVector2OrFallback(atlasDto.RenderAnchor, renderAnchor),
                        ParseVector2OrFallback(atlasDto.DrawSize, tilePixelSize),
                        ParseVector2OrFallback(atlasDto.DrawAnchor, renderAnchor),
                        ParseVector2OrFallback(atlasDto.Margins, Vector2.Zero),
                        ParseVector2OrFallback(atlasDto.Separation, Vector2.Zero));
                }
            }

            if (dto.Assets != null)
            {
                foreach (var assetDto in dto.Assets)
                {
                    if (string.IsNullOrWhiteSpace(assetDto.AssetId) ||
                        string.IsNullOrWhiteSpace(assetDto.AtlasName) ||
                        !manifest.Atlases.ContainsKey(assetDto.AtlasName) ||
                        assetDto.PixelRegion == null ||
                        assetDto.PixelRegion.Length < 4)
                    {
                        continue;
                    }

                    manifest.Tiles[assetDto.AssetId] = new MapLayerAtlasTileDefinition(
                        assetDto.AssetId,
                        assetDto.AtlasName,
                        assetDto.Family ?? string.Empty,
                        assetDto.Variant ?? string.Empty,
                        new Rect2(
                            assetDto.PixelRegion[0],
                            assetDto.PixelRegion[1],
                            assetDto.PixelRegion[2],
                            assetDto.PixelRegion[3]));
                }
            }

            return manifest;
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Failed to parse map layer atlas manifest: {exception.Message}");
            return null;
        }
    }

    private static Vector2 ParseVector2(float[]? values)
    {
        if (values == null || values.Length < 2)
        {
            return Vector2.Zero;
        }

        return new Vector2(values[0], values[1]);
    }

    private static Vector2 ParseVector2OrFallback(float[]? values, Vector2 fallback)
    {
        var parsed = ParseVector2(values);
        return parsed == Vector2.Zero ? fallback : parsed;
    }

    private sealed class MapLayerAtlasManifestDto
    {
        [JsonPropertyName("tile_pixel_size")]
        public float[]? TilePixelSize { get; set; }

        [JsonPropertyName("render_anchor")]
        public float[]? RenderAnchor { get; set; }

        [JsonPropertyName("atlases")]
        public MapLayerAtlasAtlasDto[]? Atlases { get; set; }

        [JsonPropertyName("assets")]
        public MapLayerAtlasAssetDto[]? Assets { get; set; }
    }

    private sealed class MapLayerAtlasAtlasDto
    {
        [JsonPropertyName("atlas_name")]
        public string? AtlasName { get; set; }

        [JsonPropertyName("source_image")]
        public string? SourceImage { get; set; }

        [JsonPropertyName("tile_pixel_size")]
        public float[]? TilePixelSize { get; set; }

        [JsonPropertyName("render_anchor")]
        public float[]? RenderAnchor { get; set; }

        [JsonPropertyName("draw_size")]
        public float[]? DrawSize { get; set; }

        [JsonPropertyName("draw_anchor")]
        public float[]? DrawAnchor { get; set; }

        [JsonPropertyName("margins")]
        public float[]? Margins { get; set; }

        [JsonPropertyName("separation")]
        public float[]? Separation { get; set; }
    }

    private sealed class MapLayerAtlasAssetDto
    {
        [JsonPropertyName("asset_id")]
        public string? AssetId { get; set; }

        [JsonPropertyName("atlas_name")]
        public string? AtlasName { get; set; }

        [JsonPropertyName("family")]
        public string? Family { get; set; }

        [JsonPropertyName("variant")]
        public string? Variant { get; set; }

        [JsonPropertyName("pixel_region")]
        public float[]? PixelRegion { get; set; }
    }
}

public sealed record MapLayerAtlasDefinition(
    string AtlasName,
    string SourceImage,
    Vector2 TilePixelSize,
    Vector2 RenderAnchor,
    Vector2 DrawSize,
    Vector2 DrawAnchor,
    Vector2 Margins,
    Vector2 Separation);

public sealed record MapLayerAtlasTileDefinition(
    string AssetId,
    string AtlasName,
    string Family,
    string Variant,
    Rect2 PixelRegion);
