using System.Globalization;
using System.IO;
using AquaGuard.Core;
using AquaGuard.Data;
using Microsoft.Data.Sqlite;

namespace AquaGuard.Tests;

[TestClass]
public sealed class DataLoggerTests
{
    private string folder = null!;
    private string database = null!;

    [TestInitialize]
    public void CreateStorage()
    {
        folder = Directory.CreateTempSubdirectory("aquaguard-logger-").FullName;
        database = Path.Combine(folder, "history.db");
    }

    [TestCleanup]
    public void RemoveStorage() => Directory.Delete(folder, recursive: true);

    [TestMethod]
    public void AllEventTypesSurviveReopeningWithIdentifiersValuesAndUtcTimestamps()
    {
        var logger = new DataLogger(database);
        var timestamp = new DateTimeOffset(2026, 11, 25, 15, 0, 0, TimeSpan.FromHours(-5));
        const string habitat = "reef'quoted";
        logger.LogSensorReadings([new(habitat, "salinity", 35.5, "ppt", timestamp)]);
        logger.LogEquipmentSetting(habitat, "pump", "power", "On");
        logger.LogAlarmTransitions([new(habitat, "salinity", "Raised", EnvironmentalSeverity.Critical, timestamp)]);
        logger.LogFeeding(habitat, "feeder", timestamp);

        var reopened = new DataLogger(database);
        var history = reopened.ReadHabitatHistory(habitat, DateTimeOffset.UtcNow.AddDays(1), 0, 20);

        Assert.HasCount(4, history);
        Assert.IsTrue(history.Any(row => row.Kind == "Sensor reading" && row.DeviceId == "salinity"
            && row.Value == "35.5" && row.Unit == "ppt"));
        Assert.IsTrue(history.Any(row => row.Kind == "Equipment setting" && row.DeviceId == "pump"
            && row.Setting == "power" && row.Value == "On"));
        Assert.IsTrue(history.Any(row => row.Kind == "Alarm" && row.Setting == "Raised" && row.Value == "Critical"));
        Assert.IsTrue(history.Any(row => row.Kind == "Feeding" && row.DeviceId == "feeder" && row.Value == "1"));
        Assert.IsTrue(history.All(row => DateTimeOffset.ParseExact(row.TimestampUtc, "O",
            CultureInfo.InvariantCulture).Offset == TimeSpan.Zero));
        Assert.AreEqual(timestamp.ToUniversalTime(),
            reopened.ReadFeedings(habitat, "feeder", timestamp.AddSeconds(-1)).Single());
        Assert.HasCount(0, reopened.ReadHabitatHistory("other", DateTimeOffset.UtcNow.AddDays(1), 0, 20));
    }

    [TestMethod]
    public void FailedBatchRollsBackAndPreservesPreviouslyCommittedRows()
    {
        var logger = new DataLogger(database);
        var now = DateTimeOffset.UtcNow;
        logger.LogSensorReadings([new("reef", "original", 20, null, now)]);
        using (var connection = OpenDatabase())
        using (var trigger = connection.CreateCommand())
        {
            trigger.CommandText = """
                CREATE TRIGGER reject_bad_sensor BEFORE INSERT ON sensor_readings
                WHEN NEW.device_id = 'bad'
                BEGIN SELECT RAISE(ABORT, 'simulated write failure'); END;
                """;
            trigger.ExecuteNonQuery();
        }

        Assert.ThrowsExactly<SqliteException>(() => logger.LogSensorReadings([
            new("reef", "first-in-batch", 21, null, now),
            new("reef", "bad", 22, null, now)]));
        var reopened = new DataLogger(database);
        Assert.AreEqual("original", reopened.ReadHabitatHistory("reef", now.AddDays(1), 0, 20).Single().DeviceId);
    }

    [TestMethod]
    public void LockedDatabaseReportsFailureAndAcceptsRetryAfterUnlock()
    {
        var logger = new DataLogger(database);
        logger.LogFeeding("reef", "feeder", DateTimeOffset.UtcNow);
        using (var connection = OpenDatabase())
        using (var transaction = connection.BeginTransaction())
        {
            Assert.ThrowsExactly<SqliteException>(() =>
                logger.LogEquipmentSetting("reef", "pump", "power", "On"));
            transaction.Rollback();
        }

        logger.LogEquipmentSetting("reef", "pump", "power", "On");
        Assert.HasCount(2, new DataLogger(database).ReadHabitatHistory(
            "reef", DateTimeOffset.UtcNow.AddDays(1), 0, 20));
    }

    [TestMethod]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void NonFiniteBatchIsRejectedBeforeAnyRecordIsWritten(double value)
    {
        var logger = new DataLogger(database);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => logger.LogSensorReadings([
            new("reef", "valid", 20, null, DateTimeOffset.UtcNow),
            new("reef", "invalid", value, null, DateTimeOffset.UtcNow)]));
        Assert.IsFalse(File.Exists(database));
    }

    [TestMethod]
    public void MissingDirectoryReportsFailureAndCanRecover()
    {
        string nested = Path.Combine(folder, "missing");
        var logger = new DataLogger(Path.Combine(nested, "history.db"));
        Assert.ThrowsExactly<SqliteException>(() => logger.LogFeeding("reef", "feeder", DateTimeOffset.UtcNow));
        Directory.CreateDirectory(nested);
        logger.LogFeeding("reef", "feeder", DateTimeOffset.UtcNow);
        Assert.HasCount(1, logger.ReadFeedings("reef", "feeder", DateTimeOffset.MinValue));
    }

    private SqliteConnection OpenDatabase()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = database,
            Pooling = false,
            DefaultTimeout = 1
        }.ToString());
        connection.Open();
        return connection;
    }
}
