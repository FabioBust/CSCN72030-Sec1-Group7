using System.IO;
using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices;
using AquaGuard.Devices.Equipment;

namespace AquaGuard.Tests;

[TestClass]
public sealed class ArchitectureAcceptanceTests
{
    [TestMethod]
    public void SevenMonitoredDevicesAndFiveControllableDevicesUseExistingContracts()
    {
        string root = Directory.CreateTempSubdirectory("aquaguard-architecture-").FullName;
        try
        {
            string reef = Directory.CreateDirectory(Path.Combine(root, "reef")).FullName;
            foreach (var (name, value) in new[]
            {
                ("temperature", "23.5"), ("oxygen", "7.2"), ("ph", "7.5"), ("flow", "12"),
                ("salinity", "35"), ("water-level", "80"), ("turbidity", "2")
            })
                File.WriteAllText(Path.Combine(reef, name + ".txt"), value);
            var authentication = new AuthenticationService(Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes("test-code"))));
            Assert.IsTrue(authentication.TrySignIn("test-code"));
            var hub = new Hub(authentication, new DataTransitBus(), []);
            HabitatFileSources.Load(hub, root);
            var habitat = hub.Habitats.Single();

            var readings = hub.PollHabitatSensors();
            Assert.HasCount(7, readings);
            Assert.IsTrue(readings.All(reading => reading.Value.HasValue));
            Assert.HasCount(5, habitat.Equipment);
            hub.SetEquipmentPower("reef", "circulation-pump", true);
            hub.SetEquipmentPercentage("reef", "circulation-pump", 40);
            hub.SetEquipmentPower("reef", "filter-system", true);
            hub.SetEquipmentPercentage("reef", "aerator", 60);
            var heater = (HeaterChiller)habitat.GetEquipment("heater-chiller")!;
            heater.SetTargetTemperature(22.5m);
            hub.SetEquipmentPower("reef", "food-dispenser", true);
            Assert.IsTrue(((CirculationPump)habitat.GetEquipment("circulation-pump")!).IsOn);
            Assert.IsTrue(((FilterSystem)habitat.GetEquipment("filter-system")!).IsOn);
            Assert.AreEqual(60, ((Aerator)habitat.GetEquipment("aerator")!).OutputPercentage);
            Assert.AreEqual(22.5m, heater.TargetTemperature);
            Assert.IsTrue(hub.CanFeed("reef"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void NewSensorImplementationCanBeRegisteredWithoutChangingExistingSensors()
    {
        var habitat = new Habitat("reef", "Reef");
        habitat.AddSensor(new ExtensionSensor("custom", "reef", "Custom sensor"));
        var hub = new Hub(new AuthenticationService(null), new DataTransitBus(), [habitat]);
        hub.RegisterSensorInput("reef", "custom", () => 42);

        Assert.AreEqual(42.0, hub.PollHabitatSensors().Single().Value);
    }

    private sealed record ExtensionSensor(string DeviceId, string HabitatId, string Name) : IDevice;
}
