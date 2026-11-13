using AquaGuard.Devices.Sensors;

namespace AquaGuard.Tests
{
    [TestClass]
    public class SalinitySensorTests
    {
        private readonly SalinitySensor sensor = new("SAL001", "HAB001", "Salinity Sensor");

        [TestMethod]
        public void SalinitySensor_ShouldStoreDeviceInformation()
        {
            Assert.AreEqual("SAL001", sensor.DeviceId);
            Assert.AreEqual("HAB001", sensor.HabitatId);
            Assert.AreEqual("Salinity Sensor", sensor.Name);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(35.5)]
        public void SalinitySensor_ShouldStoreReading(double salinity)
        {
            sensor.UpdateReading(10.0);
            sensor.UpdateReading(salinity);

            Assert.AreEqual(salinity, sensor.CurrentSalinity);
        }

        [TestMethod]
        [DataRow(-1.0)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void SalinitySensor_ShouldRejectInvalidReading(double salinity)
        {
            sensor.UpdateReading(35.0);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sensor.UpdateReading(salinity));
            Assert.AreEqual(35.0, sensor.CurrentSalinity);
        }

        [TestMethod]
        public void SalinitySensor_ShouldGenerateTelemetry()
        {
            sensor.UpdateReading(35.5);
            var before = DateTime.UtcNow;

            var telemetry = sensor.GetTelemetry();

            Assert.AreEqual("SAL001", telemetry.DeviceId);
            Assert.AreEqual("HAB001", telemetry.HabitatId);
            Assert.AreEqual("35.5", telemetry.Value);
            Assert.AreEqual(DateTimeKind.Utc, telemetry.Timestamp.Kind);
            Assert.IsTrue(telemetry.Timestamp >= before && telemetry.Timestamp <= DateTime.UtcNow);
        }

        [TestMethod]
        public void SalinitySensor_ShouldRejectTelemetryWithoutReading()
        {
            Assert.IsNull(sensor.CurrentSalinity);
            Assert.ThrowsExactly<InvalidOperationException>(() => sensor.GetTelemetry());
        }
    }
}
