using System;
using Microsoft.Data.Sqlite;

namespace CountyIdle.Core;

public sealed class SqliteMigrationRunner
{
    private const int CurrentSchemaVersion = 2;
    private readonly string _connectionString;

    public SqliteMigrationRunner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void EnsureMigrated()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        EnableForeignKeys(connection);

        using var transaction = connection.BeginTransaction();
        CreateSchemaMigrationTable(connection, transaction);
        ApplySchemaVersion1(connection, transaction);
        ApplySchemaVersion2(connection, transaction);
        transaction.Commit();
    }

    private static void CreateSchemaMigrationTable(SqliteConnection connection, SqliteTransaction transaction)
    {
        ExecuteNonQuery(
            connection,
            transaction,
            """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version INTEGER PRIMARY KEY,
                applied_at_utc TEXT NOT NULL
            );
            """);
    }

    private static void ApplySchemaVersion1(SqliteConnection connection, SqliteTransaction transaction)
    {
        if (HasMigrationVersion(connection, transaction, 1))
        {
            return;
        }

        ExecuteNonQuery(
            connection,
            transaction,
            """
            CREATE TABLE IF NOT EXISTS save_slots (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                slot_key TEXT NOT NULL UNIQUE,
                slot_name TEXT NOT NULL,
                is_autosave INTEGER NOT NULL DEFAULT 0,
                created_at_utc TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL,
                game_minutes INTEGER NOT NULL DEFAULT 0,
                population INTEGER NOT NULL DEFAULT 0,
                gold REAL NOT NULL DEFAULT 0,
                tech_level INTEGER NOT NULL DEFAULT 0
            );
            """);

        ExecuteNonQuery(
            connection,
            transaction,
            """
            CREATE TABLE IF NOT EXISTS save_snapshots (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                slot_id INTEGER NOT NULL,
                schema_version INTEGER NOT NULL,
                game_state_json TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                FOREIGN KEY(slot_id) REFERENCES save_slots(id) ON DELETE CASCADE
            );
            """);

        ExecuteNonQuery(
            connection,
            transaction,
            """
            CREATE INDEX IF NOT EXISTS ix_save_snapshots_slot_id_created_at
            ON save_snapshots(slot_id, created_at_utc DESC, id DESC);
            """);

        ExecuteNonQuery(
            connection,
            transaction,
            """
            INSERT INTO schema_migrations(version, applied_at_utc)
            VALUES ($version, $appliedAtUtc);
            """,
            command =>
            {
                command.Parameters.AddWithValue("$version", 1);
                command.Parameters.AddWithValue("$appliedAtUtc", DateTime.UtcNow.ToString("O"));
            });
    }

    private static void ApplySchemaVersion2(SqliteConnection connection, SqliteTransaction transaction)
    {
        if (HasMigrationVersion(connection, transaction, 2))
        {
            return;
        }

        ExecuteNonQuery(
            connection,
            transaction,
            "ALTER TABLE save_slots ADD COLUMN happiness REAL NOT NULL DEFAULT 0;");
        ExecuteNonQuery(
            connection,
            transaction,
            "ALTER TABLE save_slots ADD COLUMN threat REAL NOT NULL DEFAULT 0;");
        ExecuteNonQuery(
            connection,
            transaction,
            "ALTER TABLE save_slots ADD COLUMN exploration_depth INTEGER NOT NULL DEFAULT 1;");
        ExecuteNonQuery(
            connection,
            transaction,
            "ALTER TABLE save_slots ADD COLUMN warehouse_used REAL NOT NULL DEFAULT 0;");
        ExecuteNonQuery(
            connection,
            transaction,
            "ALTER TABLE save_slots ADD COLUMN warehouse_capacity REAL NOT NULL DEFAULT 1;");

        ExecuteNonQuery(
            connection,
            transaction,
            """
            INSERT INTO schema_migrations(version, applied_at_utc)
            VALUES ($version, $appliedAtUtc);
            """,
            command =>
            {
                command.Parameters.AddWithValue("$version", 2);
                command.Parameters.AddWithValue("$appliedAtUtc", DateTime.UtcNow.ToString("O"));
            });
    }

    private static bool HasMigrationVersion(SqliteConnection connection, SqliteTransaction transaction, int version)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(1) FROM schema_migrations WHERE version = $version;";
        command.Parameters.AddWithValue("$version", version);
        var result = Convert.ToInt64(command.ExecuteScalar() ?? 0);
        return result > 0;
    }

    private static void EnableForeignKeys(SqliteConnection connection)
    {
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
    }

    private static void ExecuteNonQuery(
        SqliteConnection connection,
        SqliteTransaction transaction,
        string commandText,
        Action<SqliteCommand>? configure = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        configure?.Invoke(command);
        command.ExecuteNonQuery();
    }
}
