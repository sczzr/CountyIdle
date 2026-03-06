param(
    [string]$SourceImage = "CountyIdle/assets/picture/Isometric_game_asset_sheet_topdown_view_zen_chines_delpmaspu.png",
    [string]$OutputDir = "CountyIdle/assets/tiles/county_reference_isometric",
    [int]$ExtractThreshold = 30,
    [int]$ExtractMinArea = 300,
    [int]$AtlasCellSize = 128,
    [int]$AtlasColumns = 6
)

$ErrorActionPreference = "Stop"

$extractDir = Join-Path $OutputDir "source_sheet_extract_t30"

dotnet run --project ".\tools\CountySheetExtractor\CountySheetExtractor.csproj" -- `
    extract $SourceImage $extractDir $ExtractThreshold $ExtractMinArea 256 8

if ($LASTEXITCODE -ne 0) {
    throw "Failed to extract source sheet components."
}

dotnet run --project ".\tools\CountySheetExtractor\CountySheetExtractor.csproj" -- `
    build-runtime-atlas $extractDir $OutputDir $AtlasCellSize $AtlasColumns

if ($LASTEXITCODE -ne 0) {
    throw "Failed to build runtime atlas from source sheet."
}

Write-Output "Generated county runtime atlas from source image: $SourceImage"
