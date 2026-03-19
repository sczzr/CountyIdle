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

        // 营建进入队列，落点在完工时由主界面统一处理。
        _gameLoop.BuildIndustryBuilding(buildingType);
    }

    private void ApplyPendingConstructionPlacements()
    {
        if (_gameLoop == null || _sectMapRenderer == null)
        {
            return;
        }

        var pending = _gameLoop.State.PendingConstructionCompletions;
        if (pending == null || pending.Count == 0)
        {
            return;
        }

        // 处理完工待落点，失败则保留待下一时辰重试。
        var completed = pending.ToArray();
        pending.Clear();
        var failed = new System.Collections.Generic.List<IndustryBuildingType>();

        foreach (var buildingType in completed)
        {
            if (_sectMapRenderer.TryPlaceBuildingAnchor(buildingType, out var placedCell, out var placementLog))
            {
                if (placedCell.HasValue)
                {
                    RegisterBuildingPlacement(_gameLoop.State, buildingType, placedCell.Value);
                }
            }
            else
            {
                failed.Add(buildingType);
            }

            if (!string.IsNullOrWhiteSpace(placementLog))
            {
                AppendLog(placementLog);
            }
        }

        if (failed.Count > 0)
        {
            pending.AddRange(failed);
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
