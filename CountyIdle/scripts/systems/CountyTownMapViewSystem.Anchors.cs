using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    // 建筑阴影色
    private static readonly Color AnchorShadowColor = new(0.04f, 0.05f, 0.05f, 0.18f);

    // 绘制活动锚点建筑（含屋顶、墙体、选中效果）
    private void DrawActivityAnchorBuilding(TownActivityAnchorData anchor, Vector2 origin)
    {
        var baseColor = GetAnchorColor(anchor.AnchorType);
        var footprintScale = GetAnchorFootprintScale(anchor.AnchorType);
        var center = GetAnchorVisualCenter(anchor, origin);
        var foundationRadius = GetScaledHexRadius() * Mathf.Clamp(footprintScale * 0.98f, 0.54f, 0.82f);
        var foundation = CreateHex(center + new Vector2(0f, ScaleValue(1.4f)), foundationRadius);
        DrawColoredPolygon(foundation, baseColor.Darkened(0.42f) * 0.92f);
        DrawPolyline(foundation, baseColor.Lightened(0.12f), Math.Max(0.8f, ScaleValue(1.0f)), true);
        var footprint = CreateDiamond(center, ScaleValue(TileHalfWidth * footprintScale), ScaleValue(TileHalfHeight * footprintScale));
        var baseTop = footprint[0];
        var baseRight = footprint[1];
        var baseBottom = footprint[2];
        var baseLeft = footprint[3];
        var isSelected = IsSelectedActivityAnchor(anchor);

        var wallHeight = ScaleValue(anchor.Floors == 1 ? 13f : 20f);
        var roofLift = ScaleValue(anchor.Floors == 1 ? 4.5f : 6.5f);
        var wallOffset = new Vector2(0f, -wallHeight);
        var roofTop = baseTop + wallOffset;
        var roofRight = baseRight + wallOffset;
        var roofBottom = baseBottom + wallOffset;
        var roofLeft = baseLeft + wallOffset;

        // 选中状态绘制光环与描边
        if (isSelected)
        {
            var pulse = 1.0f + (Mathf.Sin(Time.GetTicksMsec() / 180.0f) * 0.045f);
            var selectionHalo = CreateHex(
                center + new Vector2(0f, -ScaleValue(1.6f)),
                foundationRadius * 1.34f * pulse);
            DrawColoredPolygon(selectionHalo, TownActivityAnchorVisualRules.GetSelectionHaloColor(anchor.AnchorType));

            var selectionFootprint = CreateHex(
                center + new Vector2(0f, -ScaleValue(1.2f)),
                foundationRadius * 1.18f * pulse);
            DrawColoredPolygon(selectionFootprint, TownActivityAnchorVisualRules.GetSelectionGlowColor(anchor.AnchorType));
            DrawPolyline(selectionFootprint, TownActivityAnchorVisualRules.GetSelectionOutlineColor(anchor.AnchorType), Math.Max(1.3f, ScaleValue(1.8f)), true);

            var innerSelectionRing = CreateHex(
                center + new Vector2(0f, -ScaleValue(2.4f)),
                foundationRadius * 0.94f);
            DrawPolyline(innerSelectionRing, TownActivityAnchorVisualRules.GetSelectionPathColor(anchor.AnchorType), Math.Max(0.8f, ScaleValue(1.0f)), true);
        }

        var shadow = CreateHex(center + new Vector2(ScaleValue(2.4f), ScaleValue(3.6f)), foundationRadius * 0.76f);
        DrawColoredPolygon(shadow, AnchorShadowColor);

        // 基础墙体/屋顶配色
        var wallBright = WallBrightColor.Lerp(baseColor, 0.18f);
        var wallDark = WallDarkColor.Lerp(baseColor.Darkened(0.28f), 0.22f);
        var roofMain = RoofMainColor.Lerp(baseColor, 0.58f);
        var roofShade = RoofShadeColor.Lerp(baseColor.Darkened(0.18f), 0.44f);

        // 绘制道路连接路径
        DrawAnchorPath(anchor, origin, baseBottom, baseColor);

        var leftWall = new[] { baseLeft, baseBottom, roofBottom, roofLeft };
        var rightWall = new[] { baseBottom, baseRight, roofRight, roofBottom };
        DrawTexturedPolygon(leftWall, _wallDarkTexture, wallDark);
        DrawTexturedPolygon(rightWall, _wallBrightTexture, wallBright);

        var eaveTop = roofTop + new Vector2(0f, -roofLift);
        var eaveRight = roofRight + new Vector2(ScaleValue(3.6f), ScaleValue(1.9f));
        var eaveBottom = roofBottom + new Vector2(0f, ScaleValue(2.6f));
        var eaveLeft = roofLeft + new Vector2(-ScaleValue(3.6f), ScaleValue(1.9f));

        var roofFace = new[] { eaveTop, eaveRight, eaveBottom, eaveLeft };
        DrawTexturedPolygon(roofFace, _roofTexture, roofMain);

        var roofShadeFace = new[] { roofTop, roofRight, eaveRight, eaveTop };
        DrawTexturedPolygon(roofShadeFace, _roofTexture, roofShade);

        var ridgeStart = (eaveTop + eaveLeft) * 0.5f;
        var ridgeEnd = (eaveTop + eaveRight) * 0.5f;
        DrawLine(ridgeStart, ridgeEnd, RoofRidgeColor.Lerp(baseColor, 0.22f), Math.Max(0.9f, ScaleValue(1.4f)));

        var edgeWidth = Math.Max(0.7f, ScaleValue(0.9f));
        DrawLine(eaveTop, eaveRight, GridLineColor, edgeWidth);
        DrawLine(eaveRight, eaveBottom, GridLineColor, edgeWidth);
        DrawLine(eaveBottom, eaveLeft, GridLineColor, edgeWidth);
        DrawLine(eaveLeft, eaveTop, GridLineColor, edgeWidth);

        // 选中状态描边屋檐
        if (isSelected)
        {
            DrawPolyline(roofFace, baseColor.Lightened(0.35f), Math.Max(1.0f, ScaleValue(1.3f)), true);
        }

        // 绘制不同类型的装饰细节
        DrawAnchorAccent(anchor, baseColor, wallBright, roofMain, ridgeStart, ridgeEnd, eaveTop, eaveRight, eaveBottom, eaveLeft);
    }

    // 绘制建筑与道路的连接小径
    private void DrawAnchorPath(TownActivityAnchorData anchor, Vector2 origin, Vector2 baseBottom, Color baseColor)
    {
        var roadCenter = GetTownCellCenter(anchor.RoadCell, origin) + new Vector2(0f, ScaleValue(0.8f));
        var entrancePoint = GetAnchorEntrancePoint(anchor, baseBottom);
        var isSelected = IsSelectedActivityAnchor(anchor);
        var pathColor = isSelected
            ? TownActivityAnchorVisualRules.GetSelectionPathColor(anchor.AnchorType)
            : baseColor * 0.50f;
        var pathWidth = isSelected
            ? Math.Max(1.2f, ScaleValue(1.8f))
            : Math.Max(0.9f, ScaleValue(1.3f));

        DrawLine(roadCenter, entrancePoint, pathColor, pathWidth);
        DrawCircle(entrancePoint, Math.Max(0.9f, ScaleValue(isSelected ? 2.1f : 1.6f)), isSelected ? pathColor.Lightened(0.10f) : baseColor * 0.78f);

        // 选中时强调道路端点
        if (isSelected)
        {
            DrawCircle(roadCenter, Math.Max(0.9f, ScaleValue(1.6f)), TownActivityAnchorVisualRules.GetSelectionGlowColor(anchor.AnchorType));
        }
    }

    // 计算建筑入口在屋檐底边附近的位置
    private Vector2 GetAnchorEntrancePoint(TownActivityAnchorData anchor, Vector2 baseBottom)
    {
        var roadOffset = GetRoadOffset(anchor.Facing);
        return baseBottom + new Vector2(ScaleValue(roadOffset.X * 2.2f), ScaleValue(roadOffset.Y * 1.1f) - ScaleValue(2.4f));
    }

    // 生成选中锚点的提示文本
    private string BuildSelectedAnchorHint(TownActivityAnchorData anchor)
    {
        var anchorTypeText = SectMapSemanticRules.GetAnchorTypeText(anchor.AnchorType);
        var statusText = GetSelectedAnchorStatusText(anchor);
        var assignedResidents = GetAssignedResidentCount(anchor);
        var presentResidents = GetPresentResidentCount(anchor);
        var inboundResidents = GetInboundResidentCount(anchor);

        return $"{anchor.Label}（{anchorTypeText}）· {statusText} · 可视 {presentResidents}/{assignedResidents} · 前往中 {inboundResidents}";
    }

    // 生成选中锚点的状态文本
    private string GetSelectedAnchorStatusText(TownActivityAnchorData anchor)
    {
        if (anchor.AnchorType == TownActivityAnchorType.Administration)
        {
            return SectMapSemanticRules.GetAdministrationStatusText();
        }

        var assignedResidents = GetAssignedResidentCount(anchor);
        if (assignedResidents <= 0)
        {
            return SectMapSemanticRules.GetEmptyResidentStatusText(anchor.AnchorType);
        }

        var presentResidents = GetPresentResidentCount(anchor);
        var inboundResidents = GetInboundResidentCount(anchor);

        if (anchor.AnchorType == TownActivityAnchorType.Leisure)
        {
            if (presentResidents > 0)
            {
                return SectMapSemanticRules.GetLeisureBusyStatusText();
            }

            if (inboundResidents > 0)
            {
                return SectMapSemanticRules.GetLeisureInboundStatusText();
            }

            return SectMapSemanticRules.GetLeisureIdleStatusText();
        }

        if (presentResidents > 0)
        {
            return SectMapSemanticRules.GetWorkBusyStatusText();
        }

        if (inboundResidents > 0)
        {
            return SectMapSemanticRules.GetWorkInboundStatusText();
        }

        return SectMapSemanticRules.GetWorkIdleStatusText();
    }

    // 根据点击位置选择锚点（取遮挡更靠前者）
    private TownActivityAnchorData? PickActivityAnchorAt(Vector2 localPosition, Vector2 origin)
    {
        if (_mapData == null || _mapData.ActivityAnchors.Count == 0)
        {
            return null;
        }

        TownActivityAnchorData? selectedAnchor = null;
        var selectedDepth = float.MinValue;
        var selectedDepthX = float.MinValue;

        foreach (var anchor in _mapData.ActivityAnchors)
        {
            if (!IsPointInsideActivityAnchor(anchor, origin, localPosition))
            {
                continue;
            }

            var center = GetAnchorVisualCenter(anchor, origin);
            if (center.Y > selectedDepth ||
                (Mathf.IsEqualApprox(center.Y, selectedDepth) && center.X >= selectedDepthX))
            {
                selectedDepth = center.Y;
                selectedDepthX = center.X;
                selectedAnchor = anchor;
            }
        }

        return selectedAnchor;
    }

    // 判断点是否落入锚点 hitbox
    private bool IsPointInsideActivityAnchor(TownActivityAnchorData anchor, Vector2 origin, Vector2 point)
    {
        var center = GetAnchorVisualCenter(anchor, origin) + new Vector2(0f, -ScaleValue(anchor.Floors == 1 ? 7f : 10f));
        var hitbox = CreateHex(center, GetScaledHexRadius() * Mathf.Clamp(GetAnchorFootprintScale(anchor.AnchorType) * 1.16f, 0.62f, 0.92f));
        return Geometry2D.IsPointInPolygon(point, hitbox);
    }

    // 判断锚点是否为当前选中
    private bool IsSelectedActivityAnchor(TownActivityAnchorData anchor)
    {
        return _selectedActivityAnchor != null &&
               _selectedActivityAnchor.AnchorType == anchor.AnchorType &&
               _selectedActivityAnchor.LotCell == anchor.LotCell &&
               string.Equals(_selectedActivityAnchor.Label, anchor.Label, StringComparison.Ordinal);
    }

    // 绘制不同锚点的装饰细节
    private void DrawAnchorAccent(
        TownActivityAnchorData anchor,
        Color baseColor,
        Color wallColor,
        Color roofColor,
        Vector2 ridgeStart,
        Vector2 ridgeEnd,
        Vector2 eaveTop,
        Vector2 eaveRight,
        Vector2 eaveBottom,
        Vector2 eaveLeft)
    {
        switch (anchor.AnchorType)
        {
            case TownActivityAnchorType.Farmstead:
            {
                var hayCenter = eaveBottom + new Vector2(-ScaleValue(4f), ScaleValue(3f));
                DrawCircle(hayCenter, Math.Max(1.2f, ScaleValue(1.9f)), wallColor);
                DrawCircle(hayCenter + new Vector2(ScaleValue(3.3f), ScaleValue(1.1f)), Math.Max(1.0f, ScaleValue(1.5f)), baseColor.Lightened(0.18f));
                DrawLine(ridgeStart, ridgeStart + new Vector2(-ScaleValue(2.6f), -ScaleValue(5f)), baseColor.Darkened(0.18f), Math.Max(0.8f, ScaleValue(1.0f)));
                break;
            }
            case TownActivityAnchorType.Workshop:
            {
                var chimneyBase = eaveRight + new Vector2(-ScaleValue(1.8f), -ScaleValue(1.4f));
                var chimneyTop = chimneyBase + new Vector2(0f, -ScaleValue(9f));
                DrawLine(chimneyBase, chimneyTop, wallColor.Darkened(0.15f), Math.Max(1.0f, ScaleValue(1.5f)));
                DrawLine(chimneyTop + new Vector2(-ScaleValue(1.3f), 0f), chimneyTop + new Vector2(ScaleValue(1.3f), 0f), wallColor, Math.Max(0.8f, ScaleValue(1.0f)));
                if (anchor.VisualVariant % 2 == 0)
                {
                    DrawCircle(chimneyTop + new Vector2(ScaleValue(0.8f), -ScaleValue(2.4f)), Math.Max(0.8f, ScaleValue(1.3f)), baseColor * 0.55f);
                }
                break;
            }
            case TownActivityAnchorType.Market:
            {
                var canopyDrop = ScaleValue(anchor.VisualVariant == 0 ? 4.4f : 3.6f);
                var awning = new[]
                {
                    eaveLeft + new Vector2(ScaleValue(1.8f), ScaleValue(1.0f)),
                    eaveRight + new Vector2(-ScaleValue(1.4f), ScaleValue(0.8f)),
                    eaveRight + new Vector2(-ScaleValue(3.2f), canopyDrop),
                    eaveLeft + new Vector2(ScaleValue(3.0f), canopyDrop)
                };
                DrawColoredPolygon(awning, baseColor.Lightened(0.12f));
                DrawLine(awning[0], awning[1], roofColor.Darkened(0.10f), Math.Max(0.7f, ScaleValue(0.9f)));
                break;
            }
            case TownActivityAnchorType.Academy:
            {
                var plaqueCenter = ((eaveTop + eaveLeft) * 0.5f) + new Vector2(ScaleValue(0.8f), -ScaleValue(4.2f));
                var plaqueRect = new Rect2(plaqueCenter - new Vector2(ScaleValue(1.6f), ScaleValue(4.2f)), new Vector2(ScaleValue(3.2f), ScaleValue(8.4f)));
                DrawRect(plaqueRect, wallColor.Lightened(0.08f));
                DrawLine(plaqueCenter + new Vector2(0f, ScaleValue(4.2f)), plaqueCenter + new Vector2(0f, ScaleValue(8f)), roofColor.Darkened(0.12f), Math.Max(0.8f, ScaleValue(1.0f)));
                break;
            }
            case TownActivityAnchorType.Administration:
            {
                var poleBase = (ridgeStart + ridgeEnd) * 0.5f;
                var poleTop = poleBase + new Vector2(0f, -ScaleValue(10f));
                DrawLine(poleBase, poleTop, wallColor.Darkened(0.18f), Math.Max(1.0f, ScaleValue(1.4f)));
                var pennant = new[]
                {
                    poleTop,
                    poleTop + new Vector2(ScaleValue(5.2f), ScaleValue(1.8f)),
                    poleTop + new Vector2(ScaleValue(1.2f), ScaleValue(5.2f))
                };
                DrawColoredPolygon(pennant, baseColor.Lightened(0.06f));
                break;
            }
            case TownActivityAnchorType.Leisure:
            {
                var leftLantern = eaveLeft + new Vector2(ScaleValue(2.0f), ScaleValue(3.6f));
                var rightLantern = eaveRight + new Vector2(-ScaleValue(2.0f), ScaleValue(3.6f));
                DrawLine(eaveLeft + new Vector2(ScaleValue(2.0f), ScaleValue(1.4f)), leftLantern, wallColor.Darkened(0.08f), Math.Max(0.7f, ScaleValue(0.9f)));
                DrawLine(eaveRight + new Vector2(-ScaleValue(2.0f), ScaleValue(1.4f)), rightLantern, wallColor.Darkened(0.08f), Math.Max(0.7f, ScaleValue(0.9f)));
                DrawCircle(leftLantern, Math.Max(0.9f, ScaleValue(1.4f)), baseColor.Lightened(0.20f));
                DrawCircle(rightLantern, Math.Max(0.9f, ScaleValue(1.4f)), baseColor.Lightened(0.20f));
                break;
            }
        }
    }

    // 根据锚点类型获取建筑占地比例
    private static float GetAnchorFootprintScale(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Market => 0.66f,
            TownActivityAnchorType.Administration => 0.64f,
            TownActivityAnchorType.Academy => 0.62f,
            TownActivityAnchorType.Workshop => 0.60f,
            _ => 0.58f
        };
    }

    // 获取锚点基础颜色
    private static Color GetAnchorColor(TownActivityAnchorType anchorType)
    {
        return TownActivityAnchorVisualRules.GetMapBaseColor(anchorType);
    }

    // 为同格多建筑分配稳定的显示偏移，避免多个建筑完全叠在同一点。
    private Vector2 GetAnchorVisualCenter(TownActivityAnchorData anchor, Vector2 origin)
    {
        var center = GetTownCellCenter(anchor.LotCell, origin);
        var slotIndex = GetAnchorSlotIndex(anchor);
        var slotCount = GetAnchorSlotCount(anchor.LotCell);
        return center + GetStructureVisualOffset(slotIndex, slotCount);
    }

    private int GetAnchorSlotIndex(TownActivityAnchorData anchor)
    {
        if (_mapData == null)
        {
            return 0;
        }

        var index = 0;
        foreach (var candidate in _mapData.ActivityAnchors)
        {
            if (candidate.LotCell != anchor.LotCell)
            {
                continue;
            }

            if (ReferenceEquals(candidate, anchor))
            {
                return index;
            }

            index++;
        }

        return 0;
    }

    private int GetAnchorSlotCount(Vector2I cell)
    {
        if (_mapData == null)
        {
            return 1;
        }

        var count = 0;
        foreach (var anchor in _mapData.ActivityAnchors)
        {
            if (anchor.LotCell == cell)
            {
                count++;
            }
        }

        return Math.Max(count, 1);
    }

    private Vector2 GetStructureVisualOffset(int slotIndex, int slotCount)
    {
        return slotCount switch
        {
            2 => slotIndex switch
            {
                0 => new Vector2(-ScaleValue(8f), ScaleValue(3f)),
                _ => new Vector2(ScaleValue(8f), -ScaleValue(2f))
            },
            3 => slotIndex switch
            {
                0 => new Vector2(-ScaleValue(9f), ScaleValue(3f)),
                1 => new Vector2(ScaleValue(9f), ScaleValue(3f)),
                _ => new Vector2(0f, -ScaleValue(7f))
            },
            _ when slotCount >= 4 => slotIndex switch
            {
                0 => new Vector2(-ScaleValue(9f), ScaleValue(4f)),
                1 => new Vector2(ScaleValue(9f), ScaleValue(4f)),
                2 => new Vector2(-ScaleValue(5f), -ScaleValue(6f)),
                _ => new Vector2(ScaleValue(5f), -ScaleValue(6f))
            },
            _ => Vector2.Zero
        };
    }

    // 尝试在地图上放置指定建筑锚点
    public bool TryPlaceBuildingAnchor(
        IndustryBuildingType buildingType,
        out Vector2I? placedCell,
        out string log)
    {
        placedCell = null;
        log = string.Empty;
        if (_usesExternalMap || _mapData == null)
        {
            log = "当前不在山门沙盘，无法落地建筑。";
            return false;
        }

        // 决定落点：优先选中地块，其次自动推荐
        var targetCell = ResolvePlacementCell(buildingType, out var usedSelected, out var fallbackNote);
        if (targetCell == null)
        {
            log = "未找到可用地块，暂无法落地建筑。";
            return false;
        }

        var anchorType = ResolveAnchorType(buildingType);
        var roadCell = FindNearestRoadCell(_mapData, targetCell.Value) ?? targetCell.Value;
        var facing = ResolveFacingFromRoad(targetCell.Value, roadCell);
        var floors = ResolveAnchorFloors(buildingType);
        var visualVariant = GetCellHash(targetCell.Value, ((int)buildingType * 37) + 17) % 3;
        var label = BuildAnchorLabel(anchorType);

        // 创建锚点并写入地图
        var anchor = new TownActivityAnchorData(
            anchorType,
            roadCell,
            targetCell.Value,
            facing,
            floors,
            visualVariant,
            label);

        RegisterPlacement(buildingType, targetCell.Value);
        _mapData.AddActivityAnchor(anchor);
        _mapData.AddBuilding(new TownBuildingData(targetCell.Value, facing, floors, anchorType is TownActivityAnchorType.Academy or TownActivityAnchorType.Administration));
        _selectedActivityAnchor = anchor;
        _selectedCell = targetCell.Value;
        placedCell = targetCell.Value;
        UpdateMapHint();
        NotifySelectionSummaryChanged();
        QueueRedraw();

        var locationText = $"[{targetCell.Value.X},{targetCell.Value.Y}]";
        var displayName = SectMapSemanticRules.GetBuildingDisplayName(buildingType);
        log = usedSelected
            ? $"已在地块 {locationText} 落成 {displayName}。"
            : $"未选中可用地块{fallbackNote}，已在 {locationText} 落成 {displayName}。";
        return true;
    }

    // 记录已放置建筑信息
    private void RegisterPlacement(IndustryBuildingType buildingType, Vector2I cell)
    {
        // 同地块允许累积多种建筑，但同类型建筑不重复登记到同一格。
        for (var index = _placedBuildings.Count - 1; index >= 0; index--)
        {
            var existing = _placedBuildings[index];
            if (existing.X == cell.X &&
                existing.Y == cell.Y &&
                existing.BuildingType == buildingType)
            {
                return;
            }
        }

        _placedBuildings.Add(new TownBuildingPlacement(buildingType, cell.X, cell.Y));
    }

    // 将已放置建筑映射到地图（用于重建场景）
    private void ApplyPlacedBuildings(TownMapData mapData)
    {
        if (_placedBuildings.Count == 0)
        {
            return;
        }

        var groupedPlacements = _placedBuildings
            .Where(placed => placed != null)
            .GroupBy(placed => new Vector2I(placed.X, placed.Y))
            .ToArray();

        if (groupedPlacements.Length == 0)
        {
            return;
        }

        // 重建时先整格清空，再按格回放所有建筑，避免同格第二座建筑把第一座擦掉。
        foreach (var group in groupedPlacements)
        {
            var cell = group.Key;
            if (!mapData.IsInside(cell) || mapData.GetTerrain(cell.X, cell.Y) == TownTerrainType.Water)
            {
                continue;
            }

            ClearStructuresAtCell(mapData, cell);
        }

        var anchorCounts = new Dictionary<TownActivityAnchorType, int>();
        foreach (var anchor in mapData.ActivityAnchors)
        {
            anchorCounts[anchor.AnchorType] = anchorCounts.GetValueOrDefault(anchor.AnchorType, 0) + 1;
        }

        foreach (var group in groupedPlacements)
        {
            var cell = group.Key;
            if (!mapData.IsInside(cell) || mapData.GetTerrain(cell.X, cell.Y) == TownTerrainType.Water)
            {
                continue;
            }

            foreach (var placed in group)
            {
                var anchorType = ResolveAnchorType(placed.BuildingType);
                var roadCell = FindNearestRoadCell(mapData, cell) ?? cell;
                var facing = ResolveFacingFromRoad(cell, roadCell);
                var floors = ResolveAnchorFloors(placed.BuildingType);
                var visualVariant = GetCellHash(cell, ((int)placed.BuildingType * 37) + 17 + anchorCounts.GetValueOrDefault(anchorType, 0)) % 3;
                var label = BuildAnchorLabel(anchorType, anchorCounts);
                var anchor = new TownActivityAnchorData(
                    anchorType,
                    roadCell,
                    cell,
                    facing,
                    floors,
                    visualVariant,
                    label);
                mapData.AddActivityAnchor(anchor);
                mapData.AddBuilding(new TownBuildingData(cell, facing, floors, anchorType is TownActivityAnchorType.Academy or TownActivityAnchorType.Administration));
            }
        }
    }

    // 汇总已放置建筑数量，用于提示
    private TownMapBuildingHints GetPlacedBuildingCounts()
    {
        var agriculture = 0;
        var workshop = 0;
        var research = 0;
        var trade = 0;
        var administration = 0;

        foreach (var placed in _placedBuildings)
        {
            switch (placed.BuildingType)
            {
                case IndustryBuildingType.Agriculture:
                    agriculture++;
                    break;
                case IndustryBuildingType.Workshop:
                    workshop++;
                    break;
                case IndustryBuildingType.Research:
                    research++;
                    break;
                case IndustryBuildingType.Trade:
                    trade++;
                    break;
                case IndustryBuildingType.Administration:
                    administration++;
                    break;
            }
        }

        return new TownMapBuildingHints(agriculture, workshop, research, trade, administration);
    }

    // 决定建筑落点（返回推荐地块与提示）
    private Vector2I? ResolvePlacementCell(IndustryBuildingType buildingType, out bool usedSelected, out string fallbackNote)
    {
        usedSelected = false;
        fallbackNote = string.Empty;
        if (_mapData == null)
        {
            return null;
        }

        if (_selectedCell.HasValue && CanCellAcceptBuilding(_mapData, _selectedCell.Value, buildingType))
        {
            usedSelected = true;
            return _selectedCell.Value;
        }

        if (_selectedActivityAnchor != null && CanCellAcceptBuilding(_mapData, _selectedActivityAnchor.LotCell, buildingType))
        {
            usedSelected = true;
            return _selectedActivityAnchor.LotCell;
        }

        var autoCell = FindAutoPlacementCell(_mapData, buildingType);
        if (autoCell != null)
        {
            fallbackNote = "（已按推荐落点自动落建）";
        }
        return autoCell;
    }

    // 判断地块是否可用
    private static bool CanCellAcceptBuilding(TownMapData mapData, Vector2I cell, IndustryBuildingType buildingType)
    {
        if (!mapData.IsInside(cell) || mapData.GetTerrain(cell.X, cell.Y) == TownTerrainType.Water)
        {
            return false;
        }

        var compound = mapData.GetCellCompound(cell);
        if (compound == null)
        {
            return false;
        }

        if (HasBuildingTypeOnCell(mapData, cell, buildingType))
        {
            return false;
        }

        return GetCellOccupiedSlotCount(mapData, cell) < compound.BuildSlotCount;
    }

    // 判断地块是否已经有同类型建筑，防止同格连续建同一种建筑。
    private static bool HasBuildingTypeOnCell(TownMapData mapData, Vector2I cell, IndustryBuildingType buildingType)
    {
        foreach (var anchor in mapData.ActivityAnchors)
        {
            if (anchor.LotCell == cell && ResolveBuildingType(anchor.AnchorType) == buildingType)
            {
                return true;
            }
        }

        return false;
    }

    // 计算当前地块已占用坊位数；锚点与建筑壳体共存时只按 1 个槽位计算。
    private static int GetCellOccupiedSlotCount(TownMapData mapData, Vector2I cell)
    {
        var anchorCount = 0;
        foreach (var anchor in mapData.ActivityAnchors)
        {
            if (anchor.LotCell == cell)
            {
                anchorCount++;
            }
        }

        var buildingCount = 0;
        foreach (var building in mapData.Buildings)
        {
            if (building.Cell == cell)
            {
                buildingCount++;
            }
        }

        return Math.Max(anchorCount, buildingCount);
    }

    // 清理指定地块上的结构
    private static void ClearStructuresAtCell(TownMapData mapData, Vector2I cell)
    {
        for (var index = mapData.ActivityAnchors.Count - 1; index >= 0; index--)
        {
            if (mapData.ActivityAnchors[index].LotCell == cell)
            {
                mapData.ActivityAnchors.RemoveAt(index);
            }
        }

        for (var index = mapData.Buildings.Count - 1; index >= 0; index--)
        {
            if (mapData.Buildings[index].Cell == cell)
            {
                mapData.Buildings.RemoveAt(index);
            }
        }
    }

    // 自动推荐落点：对推荐地块打分
    private Vector2I? FindAutoPlacementCell(TownMapData mapData, IndustryBuildingType buildingType)
    {
        var bestCell = (Vector2I?)null;
        var bestScore = int.MinValue;
        foreach (var cell in mapData.EnumerateAllCells())
        {
            if (!CanCellAcceptBuilding(mapData, cell, buildingType))
            {
                continue;
            }

            var compound = mapData.GetCellCompound(cell);
            if (compound == null)
            {
                continue;
            }

            var score = 0;
            if (compound.SuggestedBuildType == buildingType)
            {
                score += 100;
            }

            if (compound.ContentKind == TownCellContentKind.Production &&
                buildingType is IndustryBuildingType.Agriculture or IndustryBuildingType.Workshop or IndustryBuildingType.Trade)
            {
                score += 40;
            }

            if (compound.ContentKind == TownCellContentKind.Service &&
                buildingType is IndustryBuildingType.Research or IndustryBuildingType.Administration)
            {
                score += 40;
            }

            if (mapData.GetTerrain(cell.X, cell.Y) == TownTerrainType.Courtyard)
            {
                score += 20;
            }

            if (FindNearestRoadCell(mapData, cell) != null)
            {
                score += 10;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestCell = cell;
            }
        }

        return bestCell;
    }

    // 建筑类型映射为锚点类型
    private static TownActivityAnchorType ResolveAnchorType(IndustryBuildingType buildingType)
    {
        return buildingType switch
        {
            IndustryBuildingType.Agriculture => TownActivityAnchorType.Farmstead,
            IndustryBuildingType.Workshop => TownActivityAnchorType.Workshop,
            IndustryBuildingType.Research => TownActivityAnchorType.Academy,
            IndustryBuildingType.Trade => TownActivityAnchorType.Market,
            IndustryBuildingType.Administration => TownActivityAnchorType.Administration,
            _ => TownActivityAnchorType.Farmstead
        };
    }

    // 反向映射锚点类型，供同格重复建造校验与摘要统计使用。
    private static IndustryBuildingType ResolveBuildingType(TownActivityAnchorType anchorType)
    {
        return anchorType switch
        {
            TownActivityAnchorType.Farmstead => IndustryBuildingType.Agriculture,
            TownActivityAnchorType.Workshop => IndustryBuildingType.Workshop,
            TownActivityAnchorType.Academy => IndustryBuildingType.Research,
            TownActivityAnchorType.Market => IndustryBuildingType.Trade,
            TownActivityAnchorType.Administration => IndustryBuildingType.Administration,
            _ => IndustryBuildingType.Agriculture
        };
    }

    // 根据建筑类型与精英提示决定楼层数
    private int ResolveAnchorFloors(IndustryBuildingType buildingType)
    {
        return buildingType switch
        {
            IndustryBuildingType.Administration => 2,
            IndustryBuildingType.Research => _eliteHint > 4 ? 2 : 1,
            _ => 1
        };
    }

    // 生成锚点显示名称
    private string BuildAnchorLabel(TownActivityAnchorType anchorType)
    {
        var anchorCounts = new Dictionary<TownActivityAnchorType, int>();
        if (_mapData != null)
        {
            foreach (var anchor in _mapData.ActivityAnchors)
            {
                anchorCounts[anchor.AnchorType] = anchorCounts.GetValueOrDefault(anchor.AnchorType, 0) + 1;
            }
        }

        return BuildAnchorLabel(anchorType, anchorCounts);
    }

    // 按计数生成锚点名称
    private static string BuildAnchorLabel(TownActivityAnchorType anchorType, Dictionary<TownActivityAnchorType, int> anchorCounts)
    {
        var count = anchorCounts.GetValueOrDefault(anchorType, 0) + 1;
        anchorCounts[anchorType] = count;
        return $"{SectMapSemanticRules.GetAnchorLabelPrefix(anchorType)}·{count}号";
    }

    // 寻找最近的道路格
    private static Vector2I? FindNearestRoadCell(TownMapData mapData, Vector2I lotCell)
    {
        Vector2I? bestRoadCell = null;
        var bestDistance = int.MaxValue;
        foreach (var offset in GetHexNeighborOffsets(lotCell.Y))
        {
            var neighbor = lotCell + offset;
            if (!mapData.IsInside(neighbor) || mapData.GetTerrain(neighbor.X, neighbor.Y) != TownTerrainType.Road)
            {
                continue;
            }

            var distance = Math.Abs(neighbor.X - lotCell.X) + Math.Abs(neighbor.Y - lotCell.Y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoadCell = neighbor;
            }
        }

        return bestRoadCell;
    }

    // 根据道路位置推导建筑朝向
    private static TownFacing ResolveFacingFromRoad(Vector2I lotCell, Vector2I roadCell)
    {
        var delta = roadCell - lotCell;
        if (delta.Y < 0)
        {
            return TownFacing.North;
        }

        if (delta.Y > 0)
        {
            return TownFacing.South;
        }

        return delta.X >= 0 ? TownFacing.East : TownFacing.West;
    }
}
