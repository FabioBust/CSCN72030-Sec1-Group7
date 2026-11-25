using System.IO;
using AquaGuard.Core;
using AquaGuard.Data;
using AquaGuard.Models;
using Microsoft.Data.Sqlite;

namespace AquaGuard.Tests;

[TestClass]
public class HabitatFileIntegrationTests
{
    [TestMethod]
    public void SensorPollingKeepsPublishingWhenLoggingFailsAndPersistsValidReadingsAfterRecovery()
    {
        string root = Directory.CreateTempSubdirectory("aquaguard-").FullName;
        try
        {
            string inputs = Directory.CreateDirectory(Path.Combine(root, "inputs")).FullName;
            string reef = Directory.CreateDirectory(Path.Combine(inputs, "reef")).FullName;
            File.WriteAllText(Path.Combine(reef, "temperature.txt"), "21.5\n22.5");
            File.WriteAllText(Path.Combine(reef, "oxygen.txt"), "invalid");
            string storage = Path.Combine(root, "storage");
            string file = Path.Combine(storage, "history.db");
            var bus = new DataTransitBus();
            var messages = new List<SystemMessage>();
            bus.TelemetryReceived += messages.Add;
            var hub = new Hub(new AuthenticationService(null), bus, [], null, new DataLogger(file));
            HabitatFileSources.Load(hub, inputs);

            Assert.AreEqual(21.5, hub.PollHabitatSensors().Single(r => r.DeviceId == "temperature").Value);
            Assert.IsTrue(hub.SensorLoggingFailed);
            Assert.AreEqual("21.5", messages.Single(m => m.DeviceId == "temperature").Value);
            Assert.IsNull(messages.Single(m => m.DeviceId == "oxygen").Value);

            Directory.CreateDirectory(storage);
            Assert.AreEqual(22.5, hub.PollHabitatSensors().Single(r => r.DeviceId == "temperature").Value);
            Assert.IsFalse(hub.SensorLoggingFailed);

            using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = file,
                Pooling = false
            }.ToString());
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT habitat_id, device_id, value, unit, timestamp_utc FROM sensor_readings";
            using var row = command.ExecuteReader();
            Assert.IsTrue(row.Read());
            Assert.AreEqual("reef", row.GetString(0));
            Assert.AreEqual("temperature", row.GetString(1));
            Assert.AreEqual(22.5, row.GetDouble(2));
            Assert.AreEqual("°C", row.GetString(3));
            Assert.AreEqual(messages.Last(m => m.DeviceId == "temperature").Timestamp,
                DateTimeOffset.Parse(row.GetString(4), System.Globalization.CultureInfo.InvariantCulture).UtcDateTime);
            Assert.IsFalse(row.Read());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void FiveSensorsPublishChangingReadingsAndRecoverIndependently()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string reef = Path.Combine(root, "reef");
        Directory.CreateDirectory(reef);
        try
        {
            foreach (var (id, data) in new[] { ("temperature", "21\n22"), ("oxygen", "7\n8"),
                ("ph", "7\n8"), ("flow", "100\n101"), ("salinity", "30\n31") })
                File.WriteAllText(Path.Combine(reef, id + ".txt"), data);
            var bus = new DataTransitBus();
            var messages = new List<SystemMessage>();
            bus.TelemetryReceived += messages.Add;
            var hub = new Hub(new AuthenticationService(null), bus, []);
            HabitatFileSources.Load(hub, root);
            var first = hub.PollHabitatSensors();
            var second = hub.PollHabitatSensors();
            Assert.HasCount(5, first);
            Assert.HasCount(10, messages);
            foreach (var reading in first)
                Assert.AreNotEqual(reading.Value, second.Single(r => r.DeviceId == reading.DeviceId).Value);
            CollectionAssert.AreEqual(first.ToArray(), hub.PollHabitatSensors().ToArray());
            var units = first.ToDictionary(reading => reading.DeviceId, reading => reading.Unit);
            Assert.AreEqual("°C", units["temperature"]);
            Assert.AreEqual("mg/L", units["oxygen"]);
            Assert.AreEqual("L/min", units["flow"]);
            Assert.AreEqual("ppt", units["salinity"]);
            Assert.IsNull(units["ph"]);
            foreach (string id in new[] { "temperature", "oxygen", "ph", "flow", "salinity" })
            {
                string path = Path.Combine(reef, id + ".txt");
                File.Delete(path);
                Assert.IsNull(hub.PollHabitatSensors().Single(r => r.DeviceId == id).Value);
                File.WriteAllText(path, "bad");
                Assert.IsNull(hub.PollHabitatSensors().Single(r => r.DeviceId == id).Value);
                File.WriteAllText(path, "8");
                Assert.AreEqual(8d, hub.PollHabitatSensors().Single(r => r.DeviceId == id).Value);
            }
            Assert.IsTrue(messages.Any(m => m.Value is null));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestMethod]
    public void InvalidPhysicalValuesDoNotCrashPollingOrBlockOtherSensors()
    {
        string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string reef = Path.Combine(root, "reef");
        Directory.CreateDirectory(reef);
        try
        {
            File.WriteAllText(Path.Combine(reef, "ph.txt"), "15");
            File.WriteAllText(Path.Combine(reef, "oxygen.txt"), "-1");
            File.WriteAllText(Path.Combine(reef, "flow.txt"), "-1");
            File.WriteAllText(Path.Combine(reef, "salinity.txt"), "-1");
            File.WriteAllText(Path.Combine(reef, "temperature.txt"), "24");
            var hub = new Hub(new AuthenticationService(null), new DataTransitBus(), []);
            HabitatFileSources.Load(hub, root);
            var readings = hub.PollHabitatSensors();
            Assert.AreEqual(24d, readings.Single(r => r.DeviceId == "temperature").Value);
            Assert.AreEqual(4, readings.Count(r => r.Value is null));
        }
        finally { Directory.Delete(root, true); }
    }
}
