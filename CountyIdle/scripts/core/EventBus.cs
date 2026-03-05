using System;
using CountyIdle.Models;

namespace CountyIdle.Core;

public class EventBus
{
    public event Action<GameState>? StateChanged;
    public event Action<string>? LogAdded;

    public void PublishState(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    public void PublishLog(string message)
    {
        LogAdded?.Invoke(message);
    }
}
