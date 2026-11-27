using System.Security.Cryptography;
using System.Text;
using AquaGuard.Core;
using AquaGuard.Devices.Equipment;
using AquaGuard.Devices.Sensors;
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
        public void PercentageCommandsRequireStaffAndOnlyPublishAcceptedChanges()
        {
            var pump = new CirculationPump("pump", "reef", "Pump");
            var aerator = new Aerator("air", "reef", "Aerator");
            habitat.AddEquipment(pump);
            habitat.AddEquipment(aerator);
            var otherHabitat = new Habitat("kelp", "Kelp");
            var otherPump = new CirculationPump("pump", "kelp", "Pump");
            otherHabitat.AddEquipment(otherPump);
            var hub = new Hub(authentication, bus, [habitat, otherHabitat]);
            var events = new List<SystemMessage>();
            bus.TelemetryReceived += events.Add;

            Assert.ThrowsExactly<UnauthorizedAccessException>(() => hub.SetEquipmentPercentage("reef", "pump", 40));
            Assert.IsTrue(authentication.TrySignIn("test-code"));
            hub.SetEquipmentPercentage("reef", "pump", 40);
            hub.SetEquipmentPercentage("reef", "air", 60);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => hub.SetEquipmentPercentage("reef", "air", 101));
            Assert.AreEqual(40, pump.SpeedPercentage);
            Assert.AreEqual(0, otherPump.SpeedPercentage);
            Assert.AreEqual(60, aerator.OutputPercentage);
            CollectionAssert.AreEqual(new[] { "Speed: 40%", "Output: 60%" }, events.Select(e => e.Value).ToArray());
        }

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



        [TestMethod]
        public void StaffCanConfigureSensorThresholds()
        {
            var sensor = new PHSensor(
                "ph", "reef", "pH Sensor");

            habitat.AddSensor(sensor);

            var hub = new Hub(authentication, bus, [habitat]);

            hub.RegisterSensorInput("reef", "ph", () => 7.2);

            Assert.IsTrue(authentication.TrySignIn("test-code"));

            var thresholds = new SensorThresholds
            {
                WarningLow = 6.0,
                NormalLow = 6.5,
                NormalHigh = 8.0,
                WarningHigh = 8.5
            };

            hub.SetSensorThresholds(
                "reef", "ph", thresholds);

            var result = hub.GetSensorThresholds(
                "reef", "ph");

            Assert.IsNotNull(result);
            Assert.AreEqual(6.0, result.WarningLow);
            Assert.AreEqual(6.5, result.NormalLow);
            Assert.AreEqual(8.0, result.NormalHigh);
            Assert.AreEqual(8.5, result.WarningHigh);
        }



        [TestMethod]
        public void VisitorCannotConfigureSensorThresholds()
        {
            var hub = new Hub(authentication, bus, [habitat]);

            var thresholds = new SensorThresholds
            {
                WarningLow = 20,
                NormalLow = 23,
                NormalHigh = 27,
                WarningHigh = 30
            };

            Assert.ThrowsExactly<UnauthorizedAccessException>(
                () => hub.SetSensorThresholds(
                    "reef", "temperature", thresholds));
        }


        [TestMethod]
        public void InvalidSensorThresholdsAreRejected()
        {
            var hub = new Hub(authentication, bus, [habitat]);

            Assert.IsTrue(authentication.TrySignIn("test-code"));

            var thresholds = new SensorThresholds
            {
                WarningLow = 20,
                NormalLow = 28,
                NormalHigh = 23,
                WarningHigh = 30
            };

            Assert.ThrowsExactly<ArgumentException>(
                () => hub.SetSensorThresholds(
                    "reef", "temperature", thresholds));
        }


        [TestMethod]
        public void UpdatedThresholdsChangeSensorClassification()
        {
            var sensor = new PHSensor(
                "ph", "reef", "pH Sensor");

            habitat.AddSensor(sensor);

            var hub = new Hub(authentication, bus, [habitat]);

            hub.RegisterSensorInput("reef", "ph", () => 7.2);

            Assert.IsTrue(authentication.TrySignIn("test-code"));

            hub.SetSensorThresholds("reef", "ph",
                new SensorThresholds
                {
                    WarningLow = 6.0,
                    NormalLow = 6.5,
                    NormalHigh = 8.0,
                    WarningHigh = 8.5
                });

            var reading = hub.PollHabitatSensors().Single();

            Assert.AreEqual(
                EnvironmentalSeverity.Normal,
                reading.Classification);

            hub.SetSensorThresholds("reef", "ph",
                new SensorThresholds
                {
                    WarningLow = 6.0,
                    NormalLow = 6.5,
                    NormalHigh = 7.0,
                    WarningHigh = 7.1
                });

            reading = hub.PollHabitatSensors().Single();

            Assert.AreEqual(
                EnvironmentalSeverity.Critical,
                reading.Classification);
        }


        [TestMethod]
        public void CannotConfigureUnknownSensor()
        {
            var hub = new Hub(authentication, bus, [habitat]);

            Assert.IsTrue(authentication.TrySignIn("test-code"));

            var thresholds = new SensorThresholds
            {
                WarningLow = 6.0,
                NormalLow = 6.5,
                NormalHigh = 8.0,
                WarningHigh = 8.5
            };

            Assert.ThrowsExactly<KeyNotFoundException>(
                () => hub.SetSensorThresholds(
                    "reef", "unknown", thresholds));
        }

    }
}
