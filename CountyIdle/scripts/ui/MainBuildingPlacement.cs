using CountyIdle.Models;
using Godot;

namespace CountyIdle;

public partial class Main
{
    private void BuildIndustryBuildingWithPlacement(IndustryBuildingType buildingType)
    {
        if (_gameLoop == null)
        {
            return;
        }

        var before = GetBuildingCount(_gameLoop.State, buildingType);
        _gameLoop.BuildIndustryBuilding(buildingType);
        var after = GetBuildingCount(_gameLoop.State, buildingType);

        if (after <= before)
        {
            return;
        }

        if (_sectMapRenderer == null)
        {
            return;
        }

        if (_sectMapRenderer.TryPlaceBuildingAnchor(buildingType, out var placedCell, out var placementLog))
        {
            if (placedCell.HasValue)
            {
                RegisterBuildingPlacement(_gameLoop.State, buildingType, placedCell.Value);
            }

            if (!string.IsNullOrWhiteSpace(placementLog))
            {
                AppendLog(placementLog);
            }
            return;
        }

        if (!string.IsNullOrWhiteSpace(placementLog))
        {
            AppendLog(placementLog);
        }
    }

    private static void RegisterBuildingPlacement(GameState state, IndustryBuildingType buildingType, Vector2I cell)
    {
        if (state == null)
        {
            return;
        }

        state.TownBuildingPlacements ??= new System.Collections.Generic.List<TownBuildingPlacement>();
        for (var index = state.TownBuildingPlacements.Count - 1; index >= 0; index--)
        {
            var existing = state.TownBuildingPlacements[index];
            if (existing == null)
            {
                state.TownBuildingPlacements.RemoveAt(index);
                continue;
            }

            if (existing.X == cell.X && existing.Y == cell.Y)
            {
                if (existing.BuildingType == buildingType)
                {
                    return;
                }

                state.TownBuildingPlacements.RemoveAt(index);
            }
        }

        state.TownBuildingPlacements.Add(new TownBuildingPlacement(buildingType, cell.X, cell.Y));
    }

    private static int GetBuildingCount(GameState state, IndustryBuildingType buildingType)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture => state.AgricultureBuildings,
            IndustryBuildingType.Workshop => state.WorkshopBuildings,
            IndustryBuildingType.Research => state.ResearchBuildings,
            IndustryBuildingType.Trade => state.TradeBuildings,
            IndustryBuildingType.Administration => state.AdministrationBuildings,
            _ => 0
        };
    }
}
