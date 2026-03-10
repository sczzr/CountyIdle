using System;
using System.Collections.Generic;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Systems;

public partial class CountyTownMapViewSystem
{
    private const string FarmerResidentSpritePath = "res://assets/characters/residents/resident_farmer.png";
    private const string WorkerResidentSpritePath = "res://assets/characters/residents/resident_worker.png";
    private const string MerchantResidentSpritePath = "res://assets/characters/residents/resident_merchant.png";
    private const string ScholarResidentSpritePath = "res://assets/characters/residents/resident_scholar.png";
    private const int MinutesPerDay = 24 * 60;

    private enum ResidentActivityPhase
    {
        AtHome,
        CommuteToWork,
        AtWork,
        CommuteToLeisure,
        AtLeisure,
        CommuteHome
    }

    private readonly record struct ResidentSchedule(
        int LeaveHomeMinute,
        int ArriveWorkMinute,
        int LeaveWorkMinute,
        int ArriveLeisureMinute,
        int LeaveLeisureMinute,
        int ArriveHomeMinute);

    private readonly record struct ResidentPose(
        Vector2 Position,
        Color Modulate,
        float ShadowScale);

    private sealed class ResidentWalker
    {
        public JobType JobType { get; init; }
        public DiscipleProfile Profile { get; init; } = null!;
        public Vector2I HomeCell { get; init; }
        public Vector2I HomeRoadCell { get; init; }
        public TownActivityAnchorType WorkAnchorType { get; init; }
        public Vector2I WorkRoadCell { get; init; }
        public Vector2I LeisureRoadCell { get; init; }
        public List<Vector2I> HomeToWorkRoute { get; init; } = new();
        public List<Vector2I> WorkToLeisureRoute { get; init; } = new();
        public List<Vector2I> LeisureToHomeRoute { get; init; } = new();
        public int ScheduleOffsetMinutes { get; init; }
        public float BobPhase { get; init; }
        public Texture2D Texture { get; init; } = null!;
        public Color BadgeColor { get; init; }
    }

    private static readonly Color ResidentShadowColor = new(0.08f, 0.07f, 0.06f, 0.34f);
    private readonly List<ResidentWalker> _residentWalkers = new();
    private readonly Dictionary<JobType, Texture2D> _residentTextures = new();

    private int _residentFarmerHint;
    private int _residentWorkerHint;
    private int _residentMerchantHint;
    private int _residentScholarHint;
    private int _residentPopulationBucket;
    private int _currentResidentGameMinutes;
    private int _lastResidentClockMinute = -1;
    private float _currentMinuteInterpolation;
    private float _residentTimeScale = 1.0f;
    private GameState _residentSourceState = new();

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        var needsUpdate = false;

        if (_isInitialized)
        {
            if (Mathf.Abs(_zoomVelocity) > 0.001f)
            {
                _zoomTarget = Mathf.Clamp(_zoomTarget + (_zoomVelocity * dt), MinZoom, MaxZoom);
                _zoomVelocity = Mathf.Lerp(_zoomVelocity, 0f, dt * ZoomVelocityDamping);
                needsUpdate = true;
            }

            var nextZoom = Mathf.Lerp(_zoom, _zoomTarget, dt * ZoomLerpSpeed);
            if (!Mathf.IsEqualApprox(nextZoom, _zoom))
            {
                _zoom = nextZoom;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                UpdateMapHint();
            }
        }

        if (_residentWalkers.Count > 0)
        {
            _currentMinuteInterpolation = Math.Clamp(_currentMinuteInterpolation + (dt * _residentTimeScale), 0f, 0.999f);
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            QueueRedraw();
        }
    }

    public void SetResidentClock(int gameMinutes, float timeScale)
    {
        if (_residentWalkers.Count == 0)
        {
            return;
        }

        var safeGameMinutes = Math.Max(gameMinutes, 0);
        if (safeGameMinutes != _lastResidentClockMinute)
        {
            _lastResidentClockMinute = safeGameMinutes;
            _currentResidentGameMinutes = safeGameMinutes;
            _currentMinuteInterpolation = 0f;
            QueueRedraw();
        }

        _residentTimeScale = Math.Clamp(timeScale, 1.0f, 4.0f);
    }

    public void RefreshResidents(GameState state)
    {
        _residentSourceState = state.Clone();
        _residentWalkers.Clear();
        _selectedResidentDiscipleId = null;
        _currentResidentGameMinutes = Math.Max(state.GameMinutes, 0);
        _lastResidentClockMinute = _currentResidentGameMinutes;
        _currentMinuteInterpolation = 0f;
        UpdateMapHint();
        QueueRedraw();
    }

    private void RebuildResidents()
    {
        _residentWalkers.Clear();

        if (_mapData == null || _mapData.Buildings.Count == 0)
        {
            return;
        }

        var roadCells = new List<Vector2I>(_mapData.EnumerateCellsByTerrain(TownTerrainType.Road));
        if (roadCells.Count == 0)
        {
            return;
        }

        var workAnchorsByType = BuildActivityAnchorLookup();
        var leisureAnchors = new List<TownActivityAnchorData>(_mapData.EnumerateActivityAnchors(TownActivityAnchorType.Leisure));
        var profileQueues = BuildResidentProfileQueues();

        EnsureResidentTextures();

        var visibleResidents = Math.Clamp(_populationHint / 12, 8, 24);
        var allocations = BuildResidentAllocations(visibleResidents);
        var residenceCursor = 0;
        var walkerIndex = 0;

        foreach (var allocation in allocations)
        {
            for (var count = 0; count < allocation.Value; count++)
            {
                var building = _mapData.Buildings[residenceCursor % _mapData.Buildings.Count];
                var homeRoadCell = GetEntranceRoadCell(building, roadCells);
                var workAnchorType = GetWorkAnchorType(allocation.Key);
                var workRoadCell = PickWorkRoadCell(allocation.Key, roadCells, workAnchorsByType, walkerIndex);
                var leisureRoadCell = PickLeisureRoadCell(roadCells, leisureAnchors, walkerIndex);
                var profile = DequeueResidentProfile(profileQueues, allocation.Key, walkerIndex);

                _residentWalkers.Add(new ResidentWalker
                {
                    JobType = allocation.Key,
                    Profile = profile,
                    HomeCell = building.Cell,
                    HomeRoadCell = homeRoadCell,
                    WorkAnchorType = workAnchorType,
                    WorkRoadCell = workRoadCell,
                    LeisureRoadCell = leisureRoadCell,
                    HomeToWorkRoute = BuildRoute(homeRoadCell, workRoadCell),
                    WorkToLeisureRoute = BuildRoute(workRoadCell, leisureRoadCell),
                    LeisureToHomeRoute = BuildRoute(leisureRoadCell, homeRoadCell),
                    ScheduleOffsetMinutes = ((walkerIndex % 7) * 4) - 12,
                    BobPhase = 0.45f * walkerIndex,
                    Texture = _residentTextures[allocation.Key],
                    BadgeColor = GetResidentBadgeColor(allocation.Key)
                });

                residenceCursor++;
                walkerIndex++;
            }
        }

        if (_selectedResidentDiscipleId.HasValue &&
            _residentWalkers.TrueForAll(walker => walker.Profile.Id != _selectedResidentDiscipleId.Value))
        {
            _selectedResidentDiscipleId = null;
        }
    }

    private Dictionary<JobType, int> BuildResidentAllocations(int visibleResidents)
    {
        var source = new Dictionary<JobType, int>
        {
            [JobType.Farmer] = _residentFarmerHint,
            [JobType.Worker] = _residentWorkerHint,
            [JobType.Merchant] = _residentMerchantHint,
            [JobType.Scholar] = _residentScholarHint
        };

        var totalAssigned = 0;
        foreach (var item in source)
        {
            totalAssigned += item.Value;
        }

        if (totalAssigned <= 0)
        {
            return new Dictionary<JobType, int>
            {
                [JobType.Farmer] = 3,
                [JobType.Worker] = 2,
                [JobType.Merchant] = 2,
                [JobType.Scholar] = 1
            };
        }

        var result = new Dictionary<JobType, int>();
        var remainders = new List<(JobType JobType, double Remainder)>();
        var allocated = 0;

        foreach (var item in source)
        {
            var exact = visibleResidents * (item.Value / (double)totalAssigned);
            var floorValue = (int)Math.Floor(exact);
            result[item.Key] = floorValue;
            remainders.Add((item.Key, exact - floorValue));
            allocated += floorValue;
        }

        remainders.Sort((left, right) => right.Remainder.CompareTo(left.Remainder));
        var remainderIndex = 0;
        while (allocated < visibleResidents)
        {
            var jobType = remainders[remainderIndex % remainders.Count].JobType;
            result[jobType] += 1;
            allocated += 1;
            remainderIndex++;
        }

        foreach (var item in source)
        {
            if (item.Value > 0 && result[item.Key] <= 0)
            {
                result[item.Key] = 1;
            }
        }

        return result;
    }

    private Dictionary<TownActivityAnchorType, List<TownActivityAnchorData>> BuildActivityAnchorLookup()
    {
        var lookup = new Dictionary<TownActivityAnchorType, List<TownActivityAnchorData>>();
        if (_mapData == null)
        {
            return lookup;
        }

        foreach (var anchor in _mapData.ActivityAnchors)
        {
            if (!lookup.TryGetValue(anchor.AnchorType, out var anchors))
            {
                anchors = new List<TownActivityAnchorData>();
                lookup[anchor.AnchorType] = anchors;
            }

            anchors.Add(anchor);
        }

        return lookup;
    }

    private void DrawResidents(TownMapData mapData, Vector2 origin)
    {
        if (_residentWalkers.Count == 0)
        {
            return;
        }

        var sortedWalkers = new List<(ResidentWalker Walker, ResidentPose Pose)>();
        foreach (var walker in _residentWalkers)
        {
            sortedWalkers.Add((walker, GetResidentPose(walker, origin)));
        }

        sortedWalkers.Sort((left, right) => left.Pose.Position.Y.CompareTo(right.Pose.Position.Y));

        var baseShadowRadius = new Vector2(ScaleValue(5.4f), ScaleValue(2.6f));
        foreach (var entry in sortedWalkers)
        {
            var shadowCenter = entry.Pose.Position + new Vector2(0f, ScaleValue(3.5f));
            DrawEllipse(shadowCenter, baseShadowRadius * entry.Pose.ShadowScale, ResidentShadowColor * entry.Pose.Modulate.A);

            var residentSize = new Vector2(
                ScaleValue(entry.Walker.Texture.GetWidth() * 0.72f),
                ScaleValue(entry.Walker.Texture.GetHeight() * 0.72f));
            var textureRect = new Rect2(
                entry.Pose.Position - new Vector2(residentSize.X * 0.5f, residentSize.Y),
                residentSize);
            DrawTextureRect(entry.Walker.Texture, textureRect, false, entry.Pose.Modulate);
            DrawCircle(
                entry.Pose.Position + new Vector2(ScaleValue(4f), -ScaleValue(10f)),
                Math.Max(1.0f, ScaleValue(1.3f)),
                entry.Walker.BadgeColor * entry.Pose.Modulate.A);

            if (_selectedResidentDiscipleId.HasValue && entry.Walker.Profile.Id == _selectedResidentDiscipleId.Value)
            {
                DrawArc(
                    entry.Pose.Position + new Vector2(0f, -ScaleValue(9f)),
                    Math.Max(ScaleValue(9.5f), 7.0f),
                    0f,
                    Mathf.Tau,
                    24,
                    new Color(0.95f, 0.84f, 0.46f, 0.92f),
                    Math.Max(1.3f, ScaleValue(1.6f)),
                    true);
            }
        }
    }

    private bool HandleResidentSelection(Vector2 localPosition)
    {
        if (_mapData == null || _residentWalkers.Count == 0)
        {
            return false;
        }

        var origin = CalculateMapOrigin(_mapData);
        var selectedWalker = PickResidentAt(localPosition, origin);
        if (selectedWalker == null)
        {
            return false;
        }

        _selectedResidentDiscipleId = selectedWalker.Profile.Id;
        _selectedActivityAnchor = FindAnchorForResident(selectedWalker);
        UpdateMapHint();
        QueueRedraw();
        RequestDiscipleInspection(selectedWalker.Profile.Id, selectedWalker.JobType);
        return true;
    }

    private string? TryBuildSelectedResidentHint()
    {
        var walker = GetSelectedResidentWalker();
        if (walker == null)
        {
            return null;
        }

        var phaseText = GetResidentActivityPhaseText(walker);
        return $"{walker.Profile.Name} · {walker.Profile.RankName} · {walker.Profile.DutyDisplayName} · {walker.Profile.RealmName}\n当前{phaseText}，已联动弟子谱定位该弟子。";
    }

    private void TryInspectAnchorResidents(TownActivityAnchorData anchor)
    {
        var representativeWalker = ResolveRepresentativeWalkerForAnchor(anchor);
        if (representativeWalker == null)
        {
            _selectedResidentDiscipleId = null;
            return;
        }

        _selectedResidentDiscipleId = representativeWalker.Profile.Id;
        RequestDiscipleInspection(representativeWalker.Profile.Id, representativeWalker.JobType);
    }

    private ResidentPose GetResidentPose(ResidentWalker walker, Vector2 origin)
    {
        var minuteOfDay = GetResidentMinuteOfDay(walker);
        var schedule = GetScheduleForJob(walker.JobType);
        var bobOffset = Mathf.Sin(((float)Time.GetTicksMsec() / 180f) + walker.BobPhase) * ScaleValue(0.9f);
        var verticalOffset = new Vector2(0f, -ScaleValue(4.2f) + bobOffset);

        if (minuteOfDay < schedule.LeaveHomeMinute || minuteOfDay >= schedule.ArriveHomeMinute)
        {
            var homePosition = GetTownCellCenter(walker.HomeRoadCell, origin) + verticalOffset + new Vector2(0f, ScaleValue(1.2f));
            return new ResidentPose(homePosition, new Color(1f, 1f, 1f, 0.78f), 0.72f);
        }

        if (minuteOfDay < schedule.ArriveWorkMinute)
        {
            var progress = GetPhaseProgress(minuteOfDay, schedule.LeaveHomeMinute, schedule.ArriveWorkMinute);
            var position = GetRoutePosition(walker.HomeToWorkRoute, progress, origin) + verticalOffset;
            return new ResidentPose(position, Colors.White, 1.0f);
        }

        if (minuteOfDay < schedule.LeaveWorkMinute)
        {
            var workPosition = GetTownCellCenter(walker.WorkRoadCell, origin) + verticalOffset;
            return new ResidentPose(workPosition, new Color(1f, 1f, 1f, 0.96f), 0.92f);
        }

        if (minuteOfDay < schedule.ArriveLeisureMinute)
        {
            var progress = GetPhaseProgress(minuteOfDay, schedule.LeaveWorkMinute, schedule.ArriveLeisureMinute);
            var position = GetRoutePosition(walker.WorkToLeisureRoute, progress, origin) + verticalOffset;
            return new ResidentPose(position, Colors.White, 1.0f);
        }

        if (minuteOfDay < schedule.LeaveLeisureMinute)
        {
            var leisurePosition = GetTownCellCenter(walker.LeisureRoadCell, origin) + verticalOffset + new Vector2(0f, -ScaleValue(0.6f));
            return new ResidentPose(leisurePosition, new Color(1f, 1f, 1f, 1.0f), 1.08f);
        }

        var homewardProgress = GetPhaseProgress(minuteOfDay, schedule.LeaveLeisureMinute, schedule.ArriveHomeMinute);
        var homewardPosition = GetRoutePosition(walker.LeisureToHomeRoute, homewardProgress, origin) + verticalOffset;
        return new ResidentPose(homewardPosition, Colors.White, 1.0f);
    }

    private float GetResidentMinuteOfDay(ResidentWalker walker)
    {
        var totalMinutes = _currentResidentGameMinutes + _currentMinuteInterpolation;
        return Modulo(totalMinutes + walker.ScheduleOffsetMinutes, MinutesPerDay);
    }

    private ResidentActivityPhase GetResidentActivityPhase(ResidentWalker walker)
    {
        var minuteOfDay = GetResidentMinuteOfDay(walker);
        var schedule = GetScheduleForJob(walker.JobType);

        if (minuteOfDay < schedule.LeaveHomeMinute || minuteOfDay >= schedule.ArriveHomeMinute)
        {
            return ResidentActivityPhase.AtHome;
        }

        if (minuteOfDay < schedule.ArriveWorkMinute)
        {
            return ResidentActivityPhase.CommuteToWork;
        }

        if (minuteOfDay < schedule.LeaveWorkMinute)
        {
            return ResidentActivityPhase.AtWork;
        }

        if (minuteOfDay < schedule.ArriveLeisureMinute)
        {
            return ResidentActivityPhase.CommuteToLeisure;
        }

        if (minuteOfDay < schedule.LeaveLeisureMinute)
        {
            return ResidentActivityPhase.AtLeisure;
        }

        return ResidentActivityPhase.CommuteHome;
    }

    private int GetAssignedResidentCount(TownActivityAnchorData anchor)
    {
        var count = 0;
        foreach (var walker in _residentWalkers)
        {
            if (IsResidentAssignedToAnchor(walker, anchor))
            {
                count++;
            }
        }

        return count;
    }

    private int GetPresentResidentCount(TownActivityAnchorData anchor)
    {
        var count = 0;
        foreach (var walker in _residentWalkers)
        {
            if (!IsResidentAssignedToAnchor(walker, anchor))
            {
                continue;
            }

            var phase = GetResidentActivityPhase(walker);
            if ((anchor.AnchorType == TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.AtLeisure) ||
                (anchor.AnchorType != TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.AtWork))
            {
                count++;
            }
        }

        return count;
    }

    private int GetInboundResidentCount(TownActivityAnchorData anchor)
    {
        var count = 0;
        foreach (var walker in _residentWalkers)
        {
            if (!IsResidentAssignedToAnchor(walker, anchor))
            {
                continue;
            }

            var phase = GetResidentActivityPhase(walker);
            if ((anchor.AnchorType == TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.CommuteToLeisure) ||
                (anchor.AnchorType != TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.CommuteToWork))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsResidentAssignedToAnchor(ResidentWalker walker, TownActivityAnchorData anchor)
    {
        return anchor.AnchorType == TownActivityAnchorType.Leisure
            ? walker.LeisureRoadCell == anchor.RoadCell
            : walker.WorkRoadCell == anchor.RoadCell;
    }

    private static float Modulo(float value, int mod)
    {
        var result = value % mod;
        return result < 0 ? result + mod : result;
    }

    private static ResidentSchedule GetScheduleForJob(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => new ResidentSchedule(330, 390, 990, 1050, 1140, 1200),
            JobType.Worker => new ResidentSchedule(420, 480, 1020, 1080, 1170, 1230),
            JobType.Merchant => new ResidentSchedule(480, 540, 1110, 1140, 1260, 1320),
            JobType.Scholar => new ResidentSchedule(450, 510, 990, 1050, 1170, 1230),
            _ => new ResidentSchedule(420, 480, 1020, 1080, 1170, 1230)
        };
    }

    private string GetResidentActivityPhaseText(ResidentWalker walker)
    {
        return GetResidentActivityPhase(walker) switch
        {
            ResidentActivityPhase.AtHome => "在舍中静修",
            ResidentActivityPhase.CommuteToWork => "正前往当值场所",
            ResidentActivityPhase.AtWork => "正值守本职差事",
            ResidentActivityPhase.CommuteToLeisure => "正在转往晚间论道点",
            ResidentActivityPhase.AtLeisure => "正在歇息论道",
            ResidentActivityPhase.CommuteHome => "正在归舍收束",
            _ => "状态稳定"
        };
    }

    private static float GetPhaseProgress(float minuteOfDay, int phaseStartMinute, int phaseEndMinute)
    {
        var duration = Math.Max(phaseEndMinute - phaseStartMinute, 1);
        return Math.Clamp((minuteOfDay - phaseStartMinute) / duration, 0f, 1f);
    }

    private Vector2 GetRoutePosition(List<Vector2I> routeCells, float progress, Vector2 origin)
    {
        if (routeCells.Count == 0)
        {
            return origin;
        }

        if (routeCells.Count == 1)
        {
            return GetTownCellCenter(routeCells[0], origin);
        }

        var segmentCount = routeCells.Count - 1;
        var routeProgress = Math.Clamp(progress, 0f, 1f) * segmentCount;
        var segmentIndex = Math.Min((int)Math.Floor(routeProgress), segmentCount - 1);
        var localProgress = routeProgress - segmentIndex;
        var from = GetTownCellCenter(routeCells[segmentIndex], origin);
        var to = GetTownCellCenter(routeCells[segmentIndex + 1], origin);
        return from.Lerp(to, localProgress);
    }

    private ResidentWalker? PickResidentAt(Vector2 localPosition, Vector2 origin)
    {
        ResidentWalker? selectedWalker = null;
        var selectedDepth = float.MinValue;
        var selectedDepthX = float.MinValue;

        foreach (var walker in _residentWalkers)
        {
            var pose = GetResidentPose(walker, origin);
            var residentRect = BuildResidentHitRect(walker, pose);
            if (!residentRect.HasPoint(localPosition))
            {
                continue;
            }

            if (pose.Position.Y > selectedDepth ||
                (Mathf.IsEqualApprox(pose.Position.Y, selectedDepth) && pose.Position.X >= selectedDepthX))
            {
                selectedDepth = pose.Position.Y;
                selectedDepthX = pose.Position.X;
                selectedWalker = walker;
            }
        }

        return selectedWalker;
    }

    private Rect2 BuildResidentHitRect(ResidentWalker walker, ResidentPose pose)
    {
        var residentSize = new Vector2(
            ScaleValue(walker.Texture.GetWidth() * 0.72f),
            ScaleValue(walker.Texture.GetHeight() * 0.72f));
        return new Rect2(
            pose.Position - new Vector2(residentSize.X * 0.5f, residentSize.Y),
            residentSize).Grow(Math.Max(2.0f, ScaleValue(2.4f)));
    }

    private Vector2I GetEntranceRoadCell(TownBuildingData building, List<Vector2I> roadCells)
    {
        var expectedRoadCell = building.Cell + GetRoadOffset(building.Facing);
        if (_mapData != null &&
            _mapData.IsInside(expectedRoadCell) &&
            _mapData.GetTerrain(expectedRoadCell.X, expectedRoadCell.Y) == TownTerrainType.Road)
        {
            return expectedRoadCell;
        }

        return FindNearestRoadCell(building.Cell, roadCells);
    }

    private Vector2I PickWorkRoadCell(
        JobType jobType,
        List<Vector2I> roadCells,
        IReadOnlyDictionary<TownActivityAnchorType, List<TownActivityAnchorData>> workAnchorsByType,
        int walkerIndex)
    {
        var workAnchorType = GetWorkAnchorType(jobType);
        if (workAnchorsByType.TryGetValue(workAnchorType, out var typedAnchors) && typedAnchors.Count > 0)
        {
            return typedAnchors[walkerIndex % typedAnchors.Count].RoadCell;
        }

        if (_mapData == null || roadCells.Count == 0)
        {
            return Vector2I.Zero;
        }

        var center = new Vector2((_mapData.Width - 1) * 0.5f, (_mapData.Height - 1) * 0.5f);
        var bestCell = roadCells[0];
        var bestScore = float.MaxValue;

        foreach (var cell in roadCells)
        {
            var deltaToCenter = new Vector2(cell.X, cell.Y) - center;
            var distanceToCenter = deltaToCenter.Length();
            var spread = GetCellSpread(cell, walkerIndex);
            float score;

            switch (jobType)
            {
                case JobType.Farmer:
                    score = (-distanceToCenter * 0.82f) + (Mathf.Abs(cell.Y - (_mapData.Height * 0.72f)) * 0.09f) + spread;
                    break;
                case JobType.Worker:
                    score = (distanceToCenter * 0.66f) + (Mathf.Abs(cell.Y - center.Y) * 0.14f) + spread;
                    break;
                case JobType.Merchant:
                    score = (distanceToCenter * 0.50f) + (Mathf.Abs(cell.Y - center.Y) * 0.04f) + spread;
                    break;
                case JobType.Scholar:
                    score = (Mathf.Abs(cell.Y - (_mapData.Height * 0.25f)) * 0.40f) + (distanceToCenter * 0.31f) + spread;
                    break;
                default:
                    score = distanceToCenter + spread;
                    break;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestCell = cell;
            }
        }

        return bestCell;
    }

    private Vector2I PickLeisureRoadCell(List<Vector2I> roadCells, List<TownActivityAnchorData> leisureAnchors, int walkerIndex)
    {
        if (leisureAnchors.Count > 0)
        {
            return leisureAnchors[walkerIndex % leisureAnchors.Count].RoadCell;
        }

        if (_mapData == null || roadCells.Count == 0)
        {
            return Vector2I.Zero;
        }

        var center = new Vector2((_mapData.Width - 1) * 0.5f, (_mapData.Height - 1) * 0.5f);
        var bestCell = roadCells[0];
        var bestScore = float.MaxValue;

        foreach (var cell in roadCells)
        {
            var deltaToCenter = new Vector2(cell.X, cell.Y) - center;
            var distanceToCenter = deltaToCenter.Length();
            var score = (distanceToCenter * 0.58f) + Mathf.Abs(cell.Y - center.Y) * 0.12f + GetCellSpread(cell, walkerIndex + 13);
            if (score < bestScore)
            {
                bestScore = score;
                bestCell = cell;
            }
        }

        return bestCell;
    }

    private static TownActivityAnchorType GetWorkAnchorType(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => TownActivityAnchorType.Farmstead,
            JobType.Worker => TownActivityAnchorType.Workshop,
            JobType.Merchant => TownActivityAnchorType.Market,
            JobType.Scholar => TownActivityAnchorType.Academy,
            _ => TownActivityAnchorType.Administration
        };
    }

    private static Color GetResidentBadgeColor(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => new Color(0.48f, 0.76f, 0.34f, 1.0f),
            JobType.Worker => new Color(0.72f, 0.62f, 0.40f, 1.0f),
            JobType.Merchant => new Color(0.84f, 0.52f, 0.30f, 1.0f),
            JobType.Scholar => new Color(0.48f, 0.62f, 0.90f, 1.0f),
            _ => Colors.White
        };
    }

    private Dictionary<JobType, Queue<DiscipleProfile>> BuildResidentProfileQueues()
    {
        var lookup = new Dictionary<JobType, Queue<DiscipleProfile>>
        {
            [JobType.Farmer] = new Queue<DiscipleProfile>(),
            [JobType.Worker] = new Queue<DiscipleProfile>(),
            [JobType.Merchant] = new Queue<DiscipleProfile>(),
            [JobType.Scholar] = new Queue<DiscipleProfile>()
        };

        foreach (var profile in DiscipleRosterSystem.BuildRoster(_residentSourceState))
        {
            if (!profile.JobType.HasValue || !lookup.TryGetValue(profile.JobType.Value, out var queue))
            {
                continue;
            }

            queue.Enqueue(profile);
        }

        return lookup;
    }

    private DiscipleProfile DequeueResidentProfile(Dictionary<JobType, Queue<DiscipleProfile>> profileQueues, JobType jobType, int walkerIndex)
    {
        if (profileQueues.TryGetValue(jobType, out var queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }

        return new DiscipleProfile(
            100000 + walkerIndex,
            $"{GetFallbackSurname(walkerIndex)}巡值弟子",
            "外门",
            jobType,
            GetFallbackDutyDisplayName(jobType),
            "炼气二层",
            1,
            DiscipleAgeBand.Young,
            18 + (walkerIndex % 12),
            false,
            60,
            60,
            50,
            50,
            50,
            50,
            50,
            40,
            "待命轮值",
            "外门居舍",
            SectOrganizationRules.GetLinkedPeakSummary(jobType),
            "状态平稳 / 服从调度",
            "当前为地图占位弟子。");
    }

    private ResidentWalker? GetSelectedResidentWalker()
    {
        if (!_selectedResidentDiscipleId.HasValue)
        {
            return null;
        }

        foreach (var walker in _residentWalkers)
        {
            if (walker.Profile.Id == _selectedResidentDiscipleId.Value)
            {
                return walker;
            }
        }

        return null;
    }

    private ResidentWalker? ResolveRepresentativeWalkerForAnchor(TownActivityAnchorData anchor)
    {
        ResidentWalker? inboundWalker = null;
        ResidentWalker? assignedWalker = null;

        foreach (var walker in _residentWalkers)
        {
            if (!IsResidentAssignedToAnchor(walker, anchor))
            {
                continue;
            }

            assignedWalker ??= walker;
            var phase = GetResidentActivityPhase(walker);
            if ((anchor.AnchorType == TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.AtLeisure) ||
                (anchor.AnchorType != TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.AtWork))
            {
                return walker;
            }

            if (inboundWalker == null &&
                ((anchor.AnchorType == TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.CommuteToLeisure) ||
                 (anchor.AnchorType != TownActivityAnchorType.Leisure && phase == ResidentActivityPhase.CommuteToWork)))
            {
                inboundWalker = walker;
            }
        }

        return inboundWalker ?? assignedWalker;
    }

    private TownActivityAnchorData? FindAnchorForResident(ResidentWalker walker)
    {
        if (_mapData == null)
        {
            return null;
        }

        var phase = GetResidentActivityPhase(walker);
        var wantsLeisureAnchor = phase is ResidentActivityPhase.AtLeisure or ResidentActivityPhase.CommuteToLeisure;
        var roadCell = wantsLeisureAnchor ? walker.LeisureRoadCell : walker.WorkRoadCell;

        foreach (var anchor in _mapData.ActivityAnchors)
        {
            if (anchor.RoadCell != roadCell)
            {
                continue;
            }

            if (wantsLeisureAnchor)
            {
                if (anchor.AnchorType == TownActivityAnchorType.Leisure)
                {
                    return anchor;
                }

                continue;
            }

            if (anchor.AnchorType == walker.WorkAnchorType)
            {
                return anchor;
            }
        }

        return null;
    }

    private static string GetFallbackSurname(int walkerIndex)
    {
        return (walkerIndex % 4) switch
        {
            0 => "云",
            1 => "陆",
            2 => "沈",
            _ => "苏"
        };
    }

    private static string GetFallbackDutyDisplayName(JobType jobType)
    {
        return jobType switch
        {
            JobType.Farmer => "阵材职司",
            JobType.Worker => "阵务职司",
            JobType.Merchant => "外事职司",
            JobType.Scholar => "推演职司",
            _ => "待命轮值"
        };
    }

    private void EnsureResidentTextures()
    {
        if (_residentTextures.Count > 0)
        {
            return;
        }

        _residentTextures[JobType.Farmer] = LoadResidentTextureOrFallback(
            FarmerResidentSpritePath,
            new Color(0.33f, 0.56f, 0.24f, 1.0f),
            new Color(0.72f, 0.86f, 0.52f, 1.0f));
        _residentTextures[JobType.Worker] = LoadResidentTextureOrFallback(
            WorkerResidentSpritePath,
            new Color(0.56f, 0.42f, 0.24f, 1.0f),
            new Color(0.82f, 0.68f, 0.44f, 1.0f));
        _residentTextures[JobType.Merchant] = LoadResidentTextureOrFallback(
            MerchantResidentSpritePath,
            new Color(0.58f, 0.22f, 0.20f, 1.0f),
            new Color(0.88f, 0.68f, 0.50f, 1.0f));
        _residentTextures[JobType.Scholar] = LoadResidentTextureOrFallback(
            ScholarResidentSpritePath,
            new Color(0.23f, 0.34f, 0.58f, 1.0f),
            new Color(0.70f, 0.78f, 0.92f, 1.0f));
    }

    private static Texture2D LoadResidentTextureOrFallback(string path, Color robeColor, Color trimColor)
    {
        if (ResourceLoader.Exists(path))
        {
            var texture = ResourceLoader.Load<Texture2D>(path);
            if (texture != null)
            {
                return texture;
            }
        }

        return CreateFallbackResidentTexture(robeColor, trimColor);
    }

    private static Texture2D CreateFallbackResidentTexture(Color robeColor, Color trimColor)
    {
        var image = Image.CreateEmpty(16, 20, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);

        var skinColor = new Color(0.91f, 0.80f, 0.68f, 1.0f);
        var hairColor = new Color(0.12f, 0.10f, 0.08f, 1.0f);
        var legColor = new Color(0.20f, 0.18f, 0.17f, 1.0f);

        FillRect(image, 6, 2, 4, 1, hairColor);
        FillRect(image, 5, 3, 6, 5, skinColor);
        FillRect(image, 4, 8, 8, 7, robeColor);
        FillRect(image, 5, 10, 6, 1, trimColor);
        FillRect(image, 4, 15, 3, 4, legColor);
        FillRect(image, 9, 15, 3, 4, legColor);
        FillRect(image, 2, 9, 2, 5, robeColor);
        FillRect(image, 12, 9, 2, 5, robeColor);

        return ImageTexture.CreateFromImage(image);
    }

    private static void FillRect(Image image, int x, int y, int width, int height, Color color)
    {
        for (var drawX = x; drawX < x + width; drawX++)
        {
            for (var drawY = y; drawY < y + height; drawY++)
            {
                image.SetPixel(drawX, drawY, color);
            }
        }
    }

    private static List<Vector2I> BuildRoute(Vector2I start, Vector2I end)
    {
        var route = new List<Vector2I> { start };
        var cursor = start;

        while (cursor.X != end.X)
        {
            cursor = new Vector2I(cursor.X + Math.Sign(end.X - cursor.X), cursor.Y);
            route.Add(cursor);
        }

        while (cursor.Y != end.Y)
        {
            cursor = new Vector2I(cursor.X, cursor.Y + Math.Sign(end.Y - cursor.Y));
            route.Add(cursor);
        }

        return route;
    }

    private Vector2I FindNearestRoadCell(Vector2I source, List<Vector2I> roadCells)
    {
        var bestCell = roadCells[0];
        var bestDistance = int.MaxValue;

        foreach (var roadCell in roadCells)
        {
            var distance = Math.Abs(roadCell.X - source.X) + Math.Abs(roadCell.Y - source.Y);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = roadCell;
            }
        }

        return bestCell;
    }

    private static Vector2I GetRoadOffset(TownFacing facing)
    {
        return facing switch
        {
            TownFacing.North => Vector2I.Up,
            TownFacing.South => Vector2I.Down,
            TownFacing.East => Vector2I.Right,
            _ => Vector2I.Left
        };
    }

    private float GetCellSpread(Vector2I cell, int walkerIndex)
    {
        var hash = (cell.X * 73856093) ^ (cell.Y * 19349663) ^ ((_layoutSeed + walkerIndex) * 83492791);
        var normalized = ((hash & 1023) / 1023.0f) - 0.5f;
        return normalized * 0.16f;
    }

    private void DrawEllipse(Vector2 center, Vector2 radius, Color color)
    {
        const int steps = 14;
        var points = new Vector2[steps];
        for (var index = 0; index < steps; index++)
        {
            var angle = Mathf.Tau * index / steps;
            points[index] = center + new Vector2(Mathf.Cos(angle) * radius.X, Mathf.Sin(angle) * radius.Y);
        }

        DrawColoredPolygon(points, color);
    }
}
