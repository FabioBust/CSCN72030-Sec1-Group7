using System.IO;
using AquaGuard.Core;
using AquaGuard.Models;

namespace AquaGuard.Tests;

[TestClass]
public class HabitatFileIntegrationTests
{
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
