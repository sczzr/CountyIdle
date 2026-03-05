using System;
using System.Text.Json;
using Godot;
using CountyIdle.Models;

namespace CountyIdle.Core;

public class SaveSystem
{
    private const string SavePath = "user://savegame.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public bool Save(GameState state, out string message)
    {
        try
        {
            var json = JsonSerializer.Serialize(state, JsonOptions);
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(json);
            message = "存档成功。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"存档失败：{ex.Message}";
            return false;
        }
    }

    public bool TryLoad(out GameState state, out string message)
    {
        state = new GameState();

        if (!FileAccess.FileExists(SavePath))
        {
            message = "未找到存档，已使用初始状态。";
            return false;
        }

        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            var content = file.GetAsText();
            state = JsonSerializer.Deserialize<GameState>(content) ?? new GameState();
            message = "读档成功。";
            return true;
        }
        catch (Exception ex)
        {
            message = $"读档失败：{ex.Message}";
            state = new GameState();
            return false;
        }
    }
}
