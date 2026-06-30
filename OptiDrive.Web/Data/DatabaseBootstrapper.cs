using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace OptiDrive.Web.Data;

public static class DatabaseBootstrapper
{
    private static readonly IReadOnlyDictionary<string, string> UserColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["PasswordHash"] = "TEXT NOT NULL DEFAULT ''",
        ["LinkedProviders"] = "TEXT NOT NULL DEFAULT 'Local'",
        ["EmailConfirmed"] = "INTEGER NOT NULL DEFAULT 0",
        ["TwoFactorEnabled"] = "INTEGER NOT NULL DEFAULT 0",
        ["AuthenticatorKey"] = "TEXT NOT NULL DEFAULT ''",
        ["RecoveryCodesJson"] = "TEXT NOT NULL DEFAULT '[]'",
        ["FailedLoginAttempts"] = "INTEGER NOT NULL DEFAULT 0",
        ["LockoutEndUtc"] = "TEXT NULL"
    };

    private static readonly IReadOnlyDictionary<string, string> RouteColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["AverageSpeedKmh"] = "INTEGER NOT NULL DEFAULT 0",
        ["AdjustedConsumptionPer100"] = "REAL NOT NULL DEFAULT 0",
        ["ConsumptionSpeedFactor"] = "REAL NOT NULL DEFAULT 1",
        ["MinimumArrivalLevelPercent"] = "INTEGER NOT NULL DEFAULT 0"
    };

    public static async Task EnsureRuntimeSchemaAsync(OptiDriveDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (!db.Database.IsSqlite())
        {
            return;
        }

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await EnsureColumnsAsync(connection, "Users", UserColumns, cancellationToken);
        await EnsureColumnsAsync(connection, "Routes", RouteColumns, cancellationToken);
        await EnsureTableAsync(
            connection,
            """
            CREATE TABLE IF NOT EXISTS SocialConnectionRequests (
                Id TEXT NOT NULL CONSTRAINT PK_SocialConnectionRequests PRIMARY KEY,
                RequesterId TEXT NOT NULL,
                TargetUserId TEXT NOT NULL,
                Status INTEGER NOT NULL,
                CreatedAtUtc TEXT NOT NULL,
                RespondedAtUtc TEXT NULL
            )
            """,
            cancellationToken);
    }

    private static async Task EnsureTableAsync(DbConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureColumnsAsync(
        DbConnection connection,
        string tableName,
        IReadOnlyDictionary<string, string> requiredColumns,
        CancellationToken cancellationToken)
    {
        var existingColumns = await GetTableColumnsAsync(connection, tableName, cancellationToken);
        foreach (var column in requiredColumns)
        {
            if (existingColumns.Contains(column.Key))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {column.Key} {column.Value}";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static async Task<HashSet<string>> GetTableColumnsAsync(DbConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}')";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }
}
