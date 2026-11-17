using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;
using AquaGuard.Models;

namespace AquaGuard.Tests
{
    [TestClass]
    public class HubTests
    {
        private readonly AuthenticationService authentication = new(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("test-code"))));
        private readonly DataTransitBus bus = new();
        private readonly Habitat habitat = new("reef", "Reef");

        [TestMethod]
        public void SensorTelemetryPreservesTheMessageContract()
        {
            var hub = new Hub(authentication, bus, [habitat]);
            SystemMessage? received = null;
            bus.TelemetryReceived += message => received = message;
            var reading = new DeviceTelemetry
            {
                HabitatId = "reef", DeviceId = "temperature", Value = "23.5", Timestamp = DateTime.UtcNow
            };

            hub.PublishSensorReading(reading);

            Assert.AreEqual(new SystemMessage(TelemetryKind.SensorReading, "reef", "temperature",
                "23.5", reading.Timestamp), received);
        }

        [TestMethod]
        public void VisitorRequestsAreDeniedBeforeRouting()
        {
            var pump = new CirculationPump("pump", habitat.HabitatId, "Pump");
            habitat.AddEquipment(pump);
            bool fed = false;
            var hub = new Hub(authentication, bus, [habitat], _ => fed = true);

            Assert.ThrowsExactly<UnauthorizedAccessException>(() => hub.SetEquipmentPower("reef", "pump", true));
            Assert.ThrowsExactly<UnauthorizedAccessException>(() => hub.RequestFeeding("reef"));
            Assert.IsFalse(pump.IsOn);
            Assert.IsFalse(fed);
        }

        [TestMethod]
        public void StaffEquipmentCommandTargetsTheSelectedHabitatAndPublishesState()
        {
            var otherHabitat = new Habitat("other", "Other");
            var otherPump = new CirculationPump("pump", otherHabitat.HabitatId, "Pump");
            otherHabitat.AddEquipment(otherPump);
            var pump = new CirculationPump("pump", habitat.HabitatId, "Pump");
            habitat.AddEquipment(pump);
            var hub = new Hub(authentication, bus, [otherHabitat, habitat]);
            SystemMessage? received = null;
            bus.TelemetryReceived += message => received = message;
            Assert.IsTrue(authentication.TrySignIn("test-code"));

            hub.SetEquipmentPower("reef", "pump", true);

            Assert.IsTrue(pump.IsOn);
            Assert.IsFalse(otherPump.IsOn);
            Assert.IsNotNull(received);
            Assert.AreEqual(TelemetryKind.EquipmentState, received.Kind);
            Assert.AreEqual("reef", received.HabitatId);
            Assert.AreEqual("pump", received.DeviceId);
            Assert.AreEqual("On", received.Value);
            Assert.AreEqual(DateTimeKind.Utc, received.Timestamp.Kind);
        }

        [TestMethod]
        public void StaffFeedingRequestRoutesToTheHandlerAndReturnsItsResult()
        {
            Habitat? requested = null;
            var hub = new Hub(authentication, bus, [habitat], target =>
            {
                requested = target;
                return false;
            });
            Assert.IsTrue(authentication.TrySignIn("test-code"));

            Assert.IsFalse(hub.RequestFeeding("reef"));
            Assert.AreSame(habitat, requested);
        }
    }
}
