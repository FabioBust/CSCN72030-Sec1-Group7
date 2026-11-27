using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using AquaGuard.Core;
using AquaGuard.Data;
using AquaGuard.Devices.Equipment;
using Microsoft.Data.Sqlite;

namespace AquaGuard.Tests;

[TestClass]
public class SQLiteLoggingVerificationTests
{
    private string folder = null!;
    private string databasePath = null!;

    private AuthenticationService auth = null!;
    private Hub hub = null!;

    private CirculationPump pump = null!;
    private FilterSystem filter = null!;
    private Aerator aerator = null!;

    [TestInitialize]
    public void Setup()
    {
        folder = Directory.CreateTempSubdirectory(
            "aquaguard-logging-").FullName;

        databasePath = Path.Combine(
            folder, "history.db");

        string hash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes("test-code")));

        auth = new AuthenticationService(hash);

        Assert.IsTrue(auth.TrySignIn("test-code"));

        var habitat = new Habitat("reef", "Reef");

        pump = new CirculationPump(
            "pump", "reef", "Pump");

        filter = new FilterSystem(
            "filter", "reef", "Filter");

        aerator = new Aerator(
            "air", "reef", "Aerator");

        habitat.AddEquipment(pump);
        habitat.AddEquipment(filter);
        habitat.AddEquipment(aerator);

        hub = new Hub(
            auth,
            new DataTransitBus(),
            [habitat],
            null,
            new DataLogger(databasePath));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }
    }

    private List<string> ReadEquipmentChanges()
    {
        using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Pooling = false
            }.ToString());

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT habitat_id, device_id, setting, value
            FROM equipment_setting_changes
            ORDER BY id
            """;

        using var rows = command.ExecuteReader();

        var changes = new List<string>();

        while (rows.Read())
        {
            changes.Add(
                $"{rows.GetString(0)}/" +
                $"{rows.GetString(1)}/" +
                $"{rows.GetString(2)}/" +
                $"{rows.GetString(3)}");
        }

        return changes;
    }

    // DB-01: Pump ON and OFF logging

    [TestMethod]
    public void PumpPowerChangesAreLogged()
    {
        hub.SetEquipmentPower("reef", "pump", true);
        hub.SetEquipmentPower("reef", "pump", false);

        Assert.IsFalse(pump.IsOn);

        CollectionAssert.AreEqual(
            new[]
            {
                "reef/pump/power/On",
                "reef/pump/power/Off"
            },
            ReadEquipmentChanges());
    }

    // DB-02: Filter ON and OFF logging

    [TestMethod]
    public void FilterPowerChangesAreLogged()
    {
        hub.SetEquipmentPower("reef", "filter", true);
        hub.SetEquipmentPower("reef", "filter", false);

        Assert.IsFalse(filter.IsOn);

        CollectionAssert.AreEqual(
            new[]
            {
                "reef/filter/power/On",
                "reef/filter/power/Off"
            },
            ReadEquipmentChanges());
    }

    // DB-03: Aerator ON and OFF logging

    [TestMethod]
    public void AeratorPowerChangesAreLogged()
    {
        hub.SetEquipmentPower("reef", "air", true);

        Assert.IsTrue(aerator.IsOn);
        Assert.AreEqual(100, aerator.OutputPercentage);

        hub.SetEquipmentPower("reef", "air", false);

        Assert.IsFalse(aerator.IsOn);
        Assert.AreEqual(0, aerator.OutputPercentage);

        CollectionAssert.AreEqual(
            new[]
            {
                "reef/air/power/On",
                "reef/air/power/Off"
            },
            ReadEquipmentChanges());
    }

    // DB-04: Combined aerator power and output logging

    [TestMethod]
    public void AeratorPowerAndOutputChangesAreLogged()
    {
        hub.SetEquipmentPower("reef", "air", true);

        hub.SetEquipmentPercentage("reef", "air", 65);

        Assert.AreEqual(65, aerator.OutputPercentage);

        hub.SetEquipmentPower("reef", "air", false);

        Assert.IsFalse(aerator.IsOn);

        CollectionAssert.AreEqual(
            new[]
            {
                "reef/air/power/On",
                "reef/air/output_percentage/65",
                "reef/air/power/Off"
            },
            ReadEquipmentChanges());
    }

    // DB-05: Repeated commands do not create duplicate logs

    [TestMethod]
    public void RepeatedPowerCommandsDoNotCreateDuplicateLogs()
    {
        hub.SetEquipmentPower("reef", "air", true);
        hub.SetEquipmentPower("reef", "air", true);

        hub.SetEquipmentPower("reef", "air", false);
        hub.SetEquipmentPower("reef", "air", false);

        CollectionAssert.AreEqual(
            new[]
            {
                "reef/air/power/On",
                "reef/air/power/Off"
            },
            ReadEquipmentChanges());
    }

    // DB-06: Equipment history survives a new Hub session

    [TestMethod]
    public void EquipmentHistorySurvivesRestart()
    {
        hub.SetEquipmentPower("reef", "pump", true);

        hub.SetEquipmentPercentage("reef", "pump", 50);

        hub.SetEquipmentPower("reef", "air", true);

        var restartedHabitat = new Habitat(
            "reef", "Reef");

        var restartedHub = new Hub(
            auth,
            new DataTransitBus(),
            [restartedHabitat],
            null,
            new DataLogger(databasePath));

        var history = restartedHub.ReadHabitatHistory(
            "reef",
            DateTimeOffset.UtcNow,
            0,
            20);

        Assert.AreEqual(3, history.Count);

        Assert.IsTrue(history.Any(row =>
            row.DeviceId == "pump" &&
            row.Setting == "power" &&
            row.Value == "On"));

        Assert.IsTrue(history.Any(row =>
            row.DeviceId == "pump" &&
            row.Setting == "speed_percentage" &&
            row.Value == "50"));

        Assert.IsTrue(history.Any(row =>
            row.DeviceId == "air" &&
            row.Setting == "power" &&
            row.Value == "On"));
    }

    // DB-07: Aerator database failure preserves state

    [TestMethod]
    public void AeratorDatabaseFailurePreservesState()
    {
        var habitat = new Habitat("reef", "Reef");

        var testAerator = new Aerator(
            "air", "reef", "Aerator");

        habitat.AddEquipment(testAerator);

        var bus = new DataTransitBus();

        var published = new List<string?>();

        bus.TelemetryReceived += message =>
            published.Add(message.Value);

        // A directory cannot be opened as a SQLite file.

        var failedHub = new Hub(
            auth,
            bus,
            [habitat],
            null,
            new DataLogger(folder));

        Assert.ThrowsExactly<SqliteException>(() =>
            failedHub.SetEquipmentPower(
                "reef", "air", true));

        Assert.IsFalse(testAerator.IsOn);
        Assert.AreEqual(0, testAerator.OutputPercentage);

        Assert.ThrowsExactly<SqliteException>(() =>
            failedHub.SetEquipmentPercentage(
                "reef", "air", 75));

        Assert.IsFalse(testAerator.IsOn);
        Assert.AreEqual(0, testAerator.OutputPercentage);

        Assert.HasCount(0, published);
    }
}

