using System;
using CountyIdle.Models;

namespace CountyIdle.Core;

/// <summary>
/// 轻量事件总线：用于状态刷新与日志广播。
/// </summary>
public class EventBus
{
    // 游戏状态刷新事件（UI 订阅）
    public event Action<GameState>? StateChanged;
    // 日志新增事件（右栏/日志面板订阅）
    public event Action<string>? LogAdded;

    /// <summary>
    /// 广播最新状态快照。
    /// </summary>
    public void PublishState(GameState state)
    {
        StateChanged?.Invoke(state);
    }

    /// <summary>
    /// 广播新增日志。
    /// </summary>
    public void PublishLog(string message)
    {
        LogAdded?.Invoke(message);
    }
}
