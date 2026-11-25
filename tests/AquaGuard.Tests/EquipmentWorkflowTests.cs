using System.IO;
using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Data;
using AquaGuard.Devices.Equipment;
using Microsoft.Data.Sqlite;

namespace AquaGuard.Tests;

[TestClass]
public sealed class EquipmentWorkflowTests
{
    private readonly AuthenticationService auth = new(
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("test-code"))));

    [TestMethod]
    public void HeaterChangesPersistWithIdentifiersAndRejectedCommandsDoNotLog()
    {
        string folder = Directory.CreateTempSubdirectory("aquaguard-").FullName;
        try
        {
            string file = Path.Combine(folder, "history.db");
            var logger = new DataLogger(file);
            var habitat = new Habitat("reef", "Reef");
            var heater = new HeaterChiller("heater", "reef", "Heater");
            habitat.AddEquipment(heater);
            var bus = new DataTransitBus();
            var published = new List<string?>();
            bus.TelemetryReceived += message => published.Add(message.Value);
            var hub = new Hub(auth, bus, [habitat]);

            Assert.ThrowsExactly<UnauthorizedAccessException>(
                () => hub.SetHeaterTarget("reef", "heater", 22.5m, logger));
            Assert.IsTrue(auth.TrySignIn("test-code"));
            hub.SetHeaterTarget("reef", "heater", 22.5m, logger);
            hub.SetHeaterTarget("reef", "heater", 22.5m, logger);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => hub.SetHeaterTarget("reef", "heater", 22.25m, logger));
            Assert.AreEqual(22.5m, heater.TargetTemperature);
            CollectionAssert.AreEqual(new[] { "22.5" }, published);

            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = file,
                Pooling = false
            }.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT habitat_id, device_id, setting, value, timestamp_utc FROM equipment_setting_changes";
            using var row = command.ExecuteReader();
            Assert.IsTrue(row.Read());
            Assert.AreEqual("reef", row.GetString(0));
            Assert.AreEqual("heater", row.GetString(1));
            Assert.AreEqual("target_temperature_c", row.GetString(2));
            Assert.AreEqual("22.5", row.GetString(3));
            Assert.IsTrue(DateTimeOffset.TryParse(row.GetString(4), out var timestamp));
            Assert.AreEqual(TimeSpan.Zero, timestamp.Offset);
            Assert.IsFalse(row.Read());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public void HeaterWriteFailurePreservesTargetAndDoesNotPublishChange()
    {
        var habitat = new Habitat("reef", "Reef");
        var heater = new HeaterChiller("heater", "reef", "Heater");
        heater.SetTargetTemperature(22.5m);
        habitat.AddEquipment(heater);
        var bus = new DataTransitBus();
        var published = new List<string?>();
        bus.TelemetryReceived += message => published.Add(message.Value);
        var hub = new Hub(auth, bus, [habitat]);
        Assert.IsTrue(auth.TrySignIn("test-code"));

        // SQLite cannot open a database when the configured filename is a directory.
        var logger = new DataLogger(AppContext.BaseDirectory);
        Assert.ThrowsExactly<SqliteException>(
            () => hub.SetHeaterTarget("reef", "heater", 23m, logger));

        Assert.AreEqual(22.5m, heater.TargetTemperature);
        Assert.HasCount(0, published);
    }

    [TestMethod]
    public void DispenserUsesPersistedCooldownAndExcludesFeedingsExactly24HoursOld()
    {
        string folder = Directory.CreateTempSubdirectory("aquaguard-").FullName;
        try
        {
            string file = Path.Combine(folder, "history.db");
            var logger = new DataLogger(file);
            var clock = new FeedingClock();
            var first = new FoodDispenser("food", "reef", "Feeder", clock) { Logger = logger };
            first.SetEnabled(true);
            Assert.IsTrue(first.TryDispense(2m));

            var reopened = new FoodDispenser("food", "reef", "Feeder", clock) { Logger = new DataLogger(file) };
            reopened.SetEnabled(true);
            Assert.IsFalse(reopened.TryDispense(2m));
            clock.Now += TimeSpan.FromHours(2);
            Assert.IsTrue(reopened.TryDispense(2m));

            for (int i = 0; i < 20; i++)
                logger.LogFeeding("kelp", "food", clock.Now - TimeSpan.FromHours(24) + TimeSpan.FromMinutes(i + 1));

            var seeded = new FoodDispenser("food", "kelp", "Feeder", clock) { Logger = logger };
            seeded.SetEnabled(true);
            Assert.IsFalse(seeded.TryDispense(2m));
            clock.Now += TimeSpan.FromMinutes(1);
            Assert.IsTrue(seeded.TryDispense(2m));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public void FeedingWriteFailureDoesNotConsumePortionOrStartCooldown()
    {
        string folder = Directory.CreateTempSubdirectory("aquaguard-").FullName;
        try
        {
            string live = Directory.CreateDirectory(Path.Combine(folder, "live")).FullName;
            string offline = Path.Combine(folder, "offline");
            var logger = new DataLogger(Path.Combine(live, "history.db"));
            var clock = new FeedingClock();
            var dispenser = new FoodDispenser("food", "reef", "Feeder", clock) { Logger = logger };
            dispenser.SetEnabled(true);
            Assert.IsTrue(dispenser.CanFeed);

            Directory.Move(live, offline);
            Assert.ThrowsExactly<SqliteException>(() => dispenser.TryDispense(2m));
            Directory.Move(offline, live);

            Assert.HasCount(0, logger.ReadFeedings("reef", "food", clock.Now.AddDays(-1)));
            Assert.IsTrue(dispenser.TryDispense(2m));
            Assert.IsFalse(dispenser.TryDispense(2m));
            CollectionAssert.AreEqual(new[] { clock.Now },
                logger.ReadFeedings("reef", "food", clock.Now.AddDays(-1)).ToArray());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private sealed class FeedingClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UnixEpoch;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [TestMethod]
    public void StaffEnablementAllowsOnlyEligibleVisitorFeedingInSelectedHabitat()
    {
        var reef = new Habitat("reef", "Reef");
        var kelp = new Habitat("kelp", "Kelp");
        reef.AddEquipment(new FoodDispenser("food", "reef", "Feeder"));
        kelp.AddEquipment(new FoodDispenser("food", "kelp", "Feeder"));
        var hub = new Hub(auth, new DataTransitBus(), [reef, kelp]);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => hub.SetEquipmentPower("reef", "food", true));
        Assert.IsTrue(auth.TrySignIn("test-code"));
        hub.SetEquipmentPower("reef", "food", true);
        auth.TrySignIn(null);

        Assert.IsFalse(hub.RequestVisitorFeeding("kelp", 2m));
        Assert.IsFalse(hub.RequestVisitorFeeding("reef", 1m));
        Assert.IsTrue(hub.RequestVisitorFeeding("reef", 2m));
        Assert.IsFalse(hub.RequestVisitorFeeding("reef", 2m));
    }
}
