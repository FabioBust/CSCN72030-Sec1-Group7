using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AquaGuard.Data;

internal sealed class DataLogger(string databasePath)
{
    static DataLogger()
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_winsqlite3());
    }

    internal void LogSensorReadings(IReadOnlyList<LoggedSensorReading> readings)
    {
        if (readings.Count == 0)
            return;

        using var connection = OpenDatabase(timeoutSeconds: 1);
        using var transaction = connection.BeginTransaction();
        using var schema = connection.CreateCommand();
        schema.Transaction = transaction;
        schema.CommandText = """
            CREATE TABLE IF NOT EXISTS sensor_readings (
                id INTEGER PRIMARY KEY,
                habitat_id TEXT NOT NULL,
                device_id TEXT NOT NULL,
                value REAL NOT NULL,
                unit TEXT,
                timestamp_utc TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS sensor_readings_habitat_time
                ON sensor_readings (habitat_id, timestamp_utc);
            """;
        schema.ExecuteNonQuery();

        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO sensor_readings (habitat_id, device_id, value, unit, timestamp_utc)
            VALUES ($habitat, $device, $value, $unit, $timestamp)
            """;
        var habitat = insert.Parameters.Add("$habitat", SqliteType.Text);
        var device = insert.Parameters.Add("$device", SqliteType.Text);
        var value = insert.Parameters.Add("$value", SqliteType.Real);
        var unit = insert.Parameters.Add("$unit", SqliteType.Text);
        var timestamp = insert.Parameters.Add("$timestamp", SqliteType.Text);
        foreach (var reading in readings)
        {
            habitat.Value = reading.HabitatId;
            device.Value = reading.DeviceId;
            value.Value = reading.Value;
            unit.Value = (object?)reading.Unit ?? DBNull.Value;
            timestamp.Value = FormatTimestamp(reading.Timestamp);
            insert.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    internal IReadOnlyList<DateTimeOffset> ReadFeedings(string habitatId, string deviceId, DateTimeOffset since)
    {
        using var connection = OpenFeedingDatabase();
        using var select = connection.CreateCommand();
        select.CommandText = """
            SELECT timestamp_utc FROM feeding_events
            WHERE habitat_id = $habitat AND device_id = $device AND timestamp_utc > $since
            ORDER BY timestamp_utc, id
            """;
        select.Parameters.AddWithValue("$habitat", habitatId);
        select.Parameters.AddWithValue("$device", deviceId);
        select.Parameters.AddWithValue("$since", FormatTimestamp(since));
        using var rows = select.ExecuteReader();
        var feedings = new List<DateTimeOffset>();
        while (rows.Read())
            feedings.Add(DateTimeOffset.ParseExact(rows.GetString(0), "O", CultureInfo.InvariantCulture));

        return feedings;
    }

    internal void LogFeeding(string habitatId, string deviceId, DateTimeOffset timestamp)
    {
        using var connection = OpenFeedingDatabase();
        using var insert = connection.CreateCommand();
        insert.CommandText = """
            INSERT INTO feeding_events (habitat_id, device_id, timestamp_utc) VALUES ($habitat, $device, $timestamp)
            """;
        insert.Parameters.AddWithValue("$habitat", habitatId);
        insert.Parameters.AddWithValue("$device", deviceId);
        insert.Parameters.AddWithValue("$timestamp", FormatTimestamp(timestamp));
        insert.ExecuteNonQuery();
    }

    private SqliteConnection OpenFeedingDatabase()
    {
        var connection = OpenDatabase();
        try
        {
            using var schema = connection.CreateCommand();
            schema.CommandText = """
                CREATE TABLE IF NOT EXISTS feeding_events (
                    id INTEGER PRIMARY KEY,
                    habitat_id TEXT NOT NULL,
                    device_id TEXT NOT NULL,
                    timestamp_utc TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS feeding_events_device_time
                    ON feeding_events (habitat_id, device_id, timestamp_utc);
                """;
            schema.ExecuteNonQuery();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static string FormatTimestamp(DateTimeOffset timestamp) =>
        timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    internal void LogEquipmentSetting(string habitatId, string deviceId, string setting, string value)
    {
        using var connection = OpenDatabase();

        using var schema = connection.CreateCommand();
        schema.CommandText = """
            CREATE TABLE IF NOT EXISTS equipment_setting_changes (
                id INTEGER PRIMARY KEY,
                habitat_id TEXT NOT NULL,
                device_id TEXT NOT NULL,
                setting TEXT NOT NULL,
                value TEXT NOT NULL,
                timestamp_utc TEXT NOT NULL
            )
            """;
        schema.ExecuteNonQuery();

        using var insert = connection.CreateCommand();
        insert.CommandText = """
            INSERT INTO equipment_setting_changes (habitat_id, device_id, setting, value, timestamp_utc)
            VALUES ($habitat, $device, $setting, $value, $timestamp)
            """;
        insert.Parameters.AddWithValue("$habitat", habitatId);
        insert.Parameters.AddWithValue("$device", deviceId);
        insert.Parameters.AddWithValue("$setting", setting);
        insert.Parameters.AddWithValue("$value", value);
        insert.Parameters.AddWithValue("$timestamp", FormatTimestamp(DateTimeOffset.UtcNow));
        insert.ExecuteNonQuery();
    }

    private SqliteConnection OpenDatabase(int timeoutSeconds = 30)
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false,
            DefaultTimeout = timeoutSeconds
        }.ToString());
        try
        {
            connection.Open();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}

internal sealed record LoggedSensorReading(
    string HabitatId, string DeviceId, double Value, string? Unit, DateTimeOffset Timestamp);
