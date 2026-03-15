using System.Collections.Generic;
using CountyIdle.Models;
using CountyIdle.Systems;

namespace CountyIdle;

public partial class Main
{
    private const int MaxSectChronicleSnapshots = 12;

    private readonly List<SectChronicleSettlementSnapshot> _sectChronicleSnapshots = new();

    private void CaptureSectChronicleSnapshot(GameState state)
    {
        if (_lastObservedHourSettlements < 0 || state.HourSettlements < _lastObservedHourSettlements)
        {
            _sectChronicleSnapshots.Clear();
        }

        if (_lastObservedHourSettlements == state.HourSettlements && _sectChronicleSnapshots.Count > 0)
        {
            return;
        }

        _sectChronicleSnapshots.Add(SectChronicleRules.CaptureSnapshot(state));
        if (_sectChronicleSnapshots.Count > MaxSectChronicleSnapshots)
        {
            _sectChronicleSnapshots.RemoveAt(0);
        }

        _lastObservedHourSettlements = state.HourSettlements;
    }

    private List<SectChronicleSettlementSnapshot> GetSectChronicleSnapshots()
    {
        return new List<SectChronicleSettlementSnapshot>(_sectChronicleSnapshots);
    }
}
