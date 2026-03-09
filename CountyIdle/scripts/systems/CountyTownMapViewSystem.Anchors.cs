using System;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    private static readonly Color AnchorShadowColor = new(0.04f, 0.05f, 0.05f, 0.18f);

    private void DrawActivityAnchorBuilding(TownActivityAnchorData anchor, Vector2 origin)
    {
        var baseColor = GetAnchorColor(anchor.AnchorType);
        var footprintScale = GetAnchorFootprintScale(anchor.AnchorType);
        var center = GetTownCellCenter(anchor.LotCell, origin);
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

        var wallBright = WallBrightColor.Lerp(baseColor, 0.18f);
        var wallDark = WallDarkColor.Lerp(baseColor.Darkened(0.28f), 0.22f);
        var roofMain = RoofMainColor.Lerp(baseColor, 0.58f);
        var roofShade = RoofShadeColor.Lerp(baseColor.Darkened(0.18f), 0.44f);

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

        if (isSelected)
        {
            DrawPolyline(roofFace, baseColor.Lightened(0.35f), Math.Max(1.0f, ScaleValue(1.3f)), true);
        }

        DrawAnchorAccent(anchor, baseColor, wallBright, roofMain, ridgeStart, ridgeEnd, eaveTop, eaveRight, eaveBottom, eaveLeft);
    }

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

        if (isSelected)
        {
            DrawCircle(roadCenter, Math.Max(0.9f, ScaleValue(1.6f)), TownActivityAnchorVisualRules.GetSelectionGlowColor(anchor.AnchorType));
        }
    }

    private Vector2 GetAnchorEntrancePoint(TownActivityAnchorData anchor, Vector2 baseBottom)
    {
        var roadOffset = GetRoadOffset(anchor.Facing);
        return baseBottom + new Vector2(ScaleValue(roadOffset.X * 2.2f), ScaleValue(roadOffset.Y * 1.1f) - ScaleValue(2.4f));
    }

    private string BuildSelectedAnchorHint(TownActivityAnchorData anchor)
    {
        var anchorTypeText = SectMapSemanticRules.GetAnchorTypeText(anchor.AnchorType);
        var statusText = GetSelectedAnchorStatusText(anchor);
        var assignedResidents = GetAssignedResidentCount(anchor);
        var presentResidents = GetPresentResidentCount(anchor);
        var inboundResidents = GetInboundResidentCount(anchor);

        return $"{anchor.Label}（{anchorTypeText}）· {statusText} · 可视 {presentResidents}/{assignedResidents} · 前往中 {inboundResidents}";
    }

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

            var center = GetTownCellCenter(anchor.LotCell, origin);
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

    private bool IsPointInsideActivityAnchor(TownActivityAnchorData anchor, Vector2 origin, Vector2 point)
    {
        var center = GetTownCellCenter(anchor.LotCell, origin) + new Vector2(0f, -ScaleValue(anchor.Floors == 1 ? 7f : 10f));
        var hitbox = CreateHex(center, GetScaledHexRadius() * Mathf.Clamp(GetAnchorFootprintScale(anchor.AnchorType) * 1.16f, 0.62f, 0.92f));
        return Geometry2D.IsPointInPolygon(point, hitbox);
    }

    private bool IsSelectedActivityAnchor(TownActivityAnchorData anchor)
    {
        return _selectedActivityAnchor != null &&
               _selectedActivityAnchor.AnchorType == anchor.AnchorType &&
               _selectedActivityAnchor.LotCell == anchor.LotCell &&
               string.Equals(_selectedActivityAnchor.Label, anchor.Label, StringComparison.Ordinal);
    }

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

    private static Color GetAnchorColor(TownActivityAnchorType anchorType)
    {
        return TownActivityAnchorVisualRules.GetMapBaseColor(anchorType);
    }
}
