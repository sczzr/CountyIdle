using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CountyIdle.Models;
using Microsoft.Data.Sqlite;

namespace CountyIdle.Core;

/// <summary>
/// SQLite 存档仓库：封装存档槽与快照的读写。
/// </summary>
public sealed class SqliteSaveRepository
{
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly SqliteMigrationRunner _migrationRunner;

    private static readonly object NativeInitLock = new();
    private static bool _nativeSqliteInitialized;

    /// <summary>
    /// 初始化仓库与连接字符串。
    /// </summary>
    public SqliteSaveRepository(string databasePath)
    {
        _databasePath = databasePath;
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();
        _migrationRunner = new SqliteMigrationRunner(_connectionString);
    }

    /// <summary>
    /// 确保原生 SQLite 已初始化并完成结构迁移。
    /// </summary>
    public void EnsureInitialized()
    {
        EnsureNativeSqliteInitialized();

        var directoryPath = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        _migrationRunner.EnsureMigrated();
    }

    /// <summary>
    /// 判断是否存在任何存档快照。
    /// </summary>
    public bool HasAnySnapshots()
    {
        EnsureInitialized();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM save_snapshots;";
        var count = Convert.ToInt64(command.ExecuteScalar() ?? 0);
        return count > 0;
    }

    /// <summary>
    /// 保存存档快照并返回槽摘要。
    /// </summary>
    public SaveSlotSummary SaveSnapshot(
        string slotKey,
        string slotName,
        bool isAutosave,
        string gameStateJson,
        int gameMinutes,
        int population,
        double gold,
        int techLevel,
        double happiness,
        double threat,
        int explorationDepth,
        double warehouseUsed,
        double warehouseCapacity)
    {
        EnsureInitialized();

        var timestamp = DateTime.UtcNow;

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        var slotId = UpsertSlot(
            connection,
            transaction,
            slotKey,
            slotName,
            isAutosave,
            timestamp,
            gameMinutes,
            population,
            gold,
            techLevel,
            happiness,
            threat,
            explorationDepth,
            warehouseUsed,
            warehouseCapacity);

        InsertSnapshot(connection, transaction, slotId, gameStateJson, timestamp);
        var summary = GetSlotSummary(connection, transaction, slotId);

        transaction.Commit();
        return summary;
    }

    /// <summary>
    /// 读取指定槽位的最新快照。
    /// </summary>
    public bool TryLoadLatestSnapshot(string slotKey, out SaveSnapshotRecord? snapshot, out SaveSlotSummary? slotSummary)
    {
        EnsureInitialized();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                slot.id AS slot_id,
                slot.slot_key,
                slot.slot_name,
                slot.is_autosave,
                slot.created_at_utc,
                slot.updated_at_utc,
                slot.game_minutes,
                slot.population,
                slot.gold,
                slot.tech_level,
                slot.happiness,
                slot.threat,
                slot.exploration_depth,
                slot.warehouse_used,
                slot.warehouse_capacity,
                snap.id AS snapshot_id,
                snap.schema_version,
                snap.game_state_json,
                snap.created_at_utc AS snapshot_created_at_utc
            FROM save_slots AS slot
            INNER JOIN save_snapshots AS snap
                ON snap.slot_id = slot.id
            WHERE slot.slot_key = $slotKey
            ORDER BY snap.id DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$slotKey", slotKey);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            snapshot = null;
            slotSummary = null;
            return false;
        }

        slotSummary = ReadSlotSummary(reader);
        snapshot = ReadSnapshotRecord(reader);
        return true;
    }

    /// <summary>
    /// 读取最新的一条快照（任意槽位）。
    /// </summary>
    public bool TryLoadLatestSnapshot(out SaveSnapshotRecord? snapshot, out SaveSlotSummary? slotSummary)
    {
        EnsureInitialized();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                slot.id AS slot_id,
                slot.slot_key,
                slot.slot_name,
                slot.is_autosave,
                slot.created_at_utc,
                slot.updated_at_utc,
                slot.game_minutes,
                slot.population,
                slot.gold,
                slot.tech_level,
                slot.happiness,
                slot.threat,
                slot.exploration_depth,
                slot.warehouse_used,
                slot.warehouse_capacity,
                snap.id AS snapshot_id,
                snap.schema_version,
                snap.game_state_json,
                snap.created_at_utc AS snapshot_created_at_utc
            FROM save_slots AS slot
            INNER JOIN save_snapshots AS snap
                ON snap.slot_id = slot.id
            ORDER BY snap.id DESC
            LIMIT 1;
            """;

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            snapshot = null;
            slotSummary = null;
            return false;
        }

        slotSummary = ReadSlotSummary(reader);
        snapshot = ReadSnapshotRecord(reader);
        return true;
    }

    /// <summary>
    /// 列出全部存档槽摘要。
    /// </summary>
    public IReadOnlyList<SaveSlotSummary> ListSlots()
    {
        EnsureInitialized();

        var slots = new List<SaveSlotSummary>();
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id AS slot_id,
                slot_key,
                slot_name,
                is_autosave,
                created_at_utc,
                updated_at_utc,
                game_minutes,
                population,
                gold,
                tech_level,
                happiness,
                threat,
                exploration_depth,
                warehouse_used,
                warehouse_capacity
            FROM save_slots
            ORDER BY updated_at_utc DESC, id DESC;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            slots.Add(ReadSlotSummary(reader));
        }

        return slots;
    }

    /// <summary>
    /// 重命名存档槽。
    /// </summary>
    public bool RenameSlot(string slotKey, string slotName)
    {
        EnsureInitialized();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE save_slots
            SET slot_name = $slotName,
                updated_at_utc = $updatedAtUtc
            WHERE slot_key = $slotKey;
            """;
        command.Parameters.AddWithValue("$slotKey", slotKey);
        command.Parameters.AddWithValue("$slotName", slotName);
        command.Parameters.AddWithValue("$updatedAtUtc", DateTime.UtcNow.ToString("O"));
        return command.ExecuteNonQuery() > 0;
    }

    /// <summary>
    /// 删除指定存档槽（联动删除快照）。
    /// </summary>
    public bool DeleteSlot(string slotKey)
    {
        EnsureInitialized();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM save_slots WHERE slot_key = $slotKey;";
        command.Parameters.AddWithValue("$slotKey", slotKey);
        return command.ExecuteNonQuery() > 0;
    }

    /// <summary>
    /// 初始化原生 SQLite（避免多线程重复初始化）。
    /// </summary>
    private static void EnsureNativeSqliteInitialized()
    {
        if (_nativeSqliteInitialized)
        {
            return;
        }

        lock (NativeInitLock)
        {
            if (_nativeSqliteInitialized)
            {
                return;
            }

            SQLitePCL.Batteries_V2.Init();
            _nativeSqliteInitialized = true;
        }
    }

    /// <summary>
    /// 打开连接并启用外键约束。
    /// </summary>
    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    /// <summary>
    /// 插入或更新存档槽，返回槽位 ID。
    /// </summary>
    private static long UpsertSlot(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string slotKey,
        string slotName,
        bool isAutosave,
        DateTime timestamp,
        int gameMinutes,
        int population,
        double gold,
        int techLevel,
        double happiness,
        double threat,
        int explorationDepth,
        double warehouseUsed,
        double warehouseCapacity)
    {
        using (var upsertCommand = connection.CreateCommand())
        {
            upsertCommand.Transaction = transaction;
            upsertCommand.CommandText =
                """
                INSERT INTO save_slots (
                    slot_key,
                    slot_name,
                    is_autosave,
                    created_at_utc,
                    updated_at_utc,
                    game_minutes,
                    population,
                    gold,
                    tech_level,
                    happiness,
                    threat,
                    exploration_depth,
                    warehouse_used,
                    warehouse_capacity
                )
                VALUES (
                    $slotKey,
                    $slotName,
                    $isAutosave,
                    $createdAtUtc,
                    $updatedAtUtc,
                    $gameMinutes,
                    $population,
                    $gold,
                    $techLevel,
                    $happiness,
                    $threat,
                    $explorationDepth,
                    $warehouseUsed,
                    $warehouseCapacity
                )
                ON CONFLICT(slot_key) DO UPDATE SET
                    slot_name = excluded.slot_name,
                    is_autosave = excluded.is_autosave,
                    updated_at_utc = excluded.updated_at_utc,
                    game_minutes = excluded.game_minutes,
                    population = excluded.population,
                    gold = excluded.gold,
                    tech_level = excluded.tech_level,
                    happiness = excluded.happiness,
                    threat = excluded.threat,
                    exploration_depth = excluded.exploration_depth,
                    warehouse_used = excluded.warehouse_used,
                    warehouse_capacity = excluded.warehouse_capacity;
                """;
            upsertCommand.Parameters.AddWithValue("$slotKey", slotKey);
            upsertCommand.Parameters.AddWithValue("$slotName", slotName);
            upsertCommand.Parameters.AddWithValue("$isAutosave", isAutosave ? 1 : 0);
            upsertCommand.Parameters.AddWithValue("$createdAtUtc", timestamp.ToString("O"));
            upsertCommand.Parameters.AddWithValue("$updatedAtUtc", timestamp.ToString("O"));
            upsertCommand.Parameters.AddWithValue("$gameMinutes", gameMinutes);
            upsertCommand.Parameters.AddWithValue("$population", population);
            upsertCommand.Parameters.AddWithValue("$gold", gold);
            upsertCommand.Parameters.AddWithValue("$techLevel", techLevel);
            upsertCommand.Parameters.AddWithValue("$happiness", happiness);
            upsertCommand.Parameters.AddWithValue("$threat", threat);
            upsertCommand.Parameters.AddWithValue("$explorationDepth", explorationDepth);
            upsertCommand.Parameters.AddWithValue("$warehouseUsed", warehouseUsed);
            upsertCommand.Parameters.AddWithValue("$warehouseCapacity", warehouseCapacity);
            upsertCommand.ExecuteNonQuery();
        }

        using var selectCommand = connection.CreateCommand();
        selectCommand.Transaction = transaction;
        selectCommand.CommandText = "SELECT id FROM save_slots WHERE slot_key = $slotKey LIMIT 1;";
        selectCommand.Parameters.AddWithValue("$slotKey", slotKey);
        return Convert.ToInt64(selectCommand.ExecuteScalar() ?? 0);
    }

    /// <summary>
    /// 新增一条存档快照。
    /// </summary>
    private static void InsertSnapshot(
        SqliteConnection connection,
        SqliteTransaction transaction,
        long slotId,
        string gameStateJson,
        DateTime timestamp)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO save_snapshots (
                slot_id,
                schema_version,
                game_state_json,
                created_at_utc
            )
            VALUES (
                $slotId,
                $schemaVersion,
                $gameStateJson,
                $createdAtUtc
            );
            """;
        command.Parameters.AddWithValue("$slotId", slotId);
        command.Parameters.AddWithValue("$schemaVersion", 1);
        command.Parameters.AddWithValue("$gameStateJson", gameStateJson);
        command.Parameters.AddWithValue("$createdAtUtc", timestamp.ToString("O"));
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// 读取槽位摘要（用于 UI 展示）。
    /// </summary>
    private static SaveSlotSummary GetSlotSummary(SqliteConnection connection, SqliteTransaction transaction, long slotId)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT
                id AS slot_id,
                slot_key,
                slot_name,
                is_autosave,
                created_at_utc,
                updated_at_utc,
                game_minutes,
                population,
                gold,
                tech_level,
                happiness,
                threat,
                exploration_depth,
                warehouse_used,
                warehouse_capacity
            FROM save_slots
            WHERE id = $slotId
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$slotId", slotId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            throw new InvalidOperationException("保存完成后未能读取存档槽摘要。");
        }

        return ReadSlotSummary(reader);
    }

    /// <summary>
    /// 从 Reader 中解析槽位摘要。
    /// </summary>
    private static SaveSlotSummary ReadSlotSummary(SqliteDataReader reader)
    {
        return new SaveSlotSummary
        {
            Id = reader.GetInt64(reader.GetOrdinal("slot_id")),
            SlotKey = reader.GetString(reader.GetOrdinal("slot_key")),
            SlotName = reader.GetString(reader.GetOrdinal("slot_name")),
            IsAutosave = reader.GetInt64(reader.GetOrdinal("is_autosave")) != 0,
            CreatedAtUtc = ReadUtcDateTime(reader, "created_at_utc"),
            UpdatedAtUtc = ReadUtcDateTime(reader, "updated_at_utc"),
            GameMinutes = reader.GetInt32(reader.GetOrdinal("game_minutes")),
            Population = reader.GetInt32(reader.GetOrdinal("population")),
            Gold = reader.GetDouble(reader.GetOrdinal("gold")),
            TechLevel = reader.GetInt32(reader.GetOrdinal("tech_level")),
            Happiness = reader.GetDouble(reader.GetOrdinal("happiness")),
            Threat = reader.GetDouble(reader.GetOrdinal("threat")),
            ExplorationDepth = reader.GetInt32(reader.GetOrdinal("exploration_depth")),
            WarehouseUsed = reader.GetDouble(reader.GetOrdinal("warehouse_used")),
            WarehouseCapacity = reader.GetDouble(reader.GetOrdinal("warehouse_capacity"))
        };
    }

    /// <summary>
    /// 从 Reader 中解析快照记录。
    /// </summary>
    private static SaveSnapshotRecord ReadSnapshotRecord(SqliteDataReader reader)
    {
        return new SaveSnapshotRecord
        {
            Id = reader.GetInt64(reader.GetOrdinal("snapshot_id")),
            SlotId = reader.GetInt64(reader.GetOrdinal("slot_id")),
            SchemaVersion = reader.GetInt32(reader.GetOrdinal("schema_version")),
            GameStateJson = reader.GetString(reader.GetOrdinal("game_state_json")),
            CreatedAtUtc = ReadUtcDateTime(reader, "snapshot_created_at_utc")
        };
    }

    /// <summary>
    /// 解析 UTC 时间戳（失败时回退当前时间）。
    /// </summary>
    private static DateTime ReadUtcDateTime(SqliteDataReader reader, string columnName)
    {
        var raw = reader.GetString(reader.GetOrdinal(columnName));
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)
            ? result
            : DateTime.UtcNow;
    }
}
