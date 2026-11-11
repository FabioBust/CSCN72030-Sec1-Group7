
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Devices.Sensors;

namespace AquaGuard.Tests
{
    [TestClass]
    public class PHSensorTests
    {
        [TestMethod]
        public void PHSensor_ShouldStoreDeviceInformation()
        {
            var sensor = new PHSensor(
                "PH001",
                "HAB001",
                "pH Sensor");

            Assert.AreEqual("PH001", sensor.DeviceId);
            Assert.AreEqual("HAB001", sensor.HabitatId);
            Assert.AreEqual("pH Sensor", sensor.Name);
        }

        [TestMethod]
        public void PHSensor_ShouldStorePHReading()
        {
            // Arrange
            var sensor = new PHSensor(
                "PH001",
                "HAB001",
                "pH Sensor");

            // Act
            sensor.UpdateReading(7.2);

            // Assert
            Assert.AreEqual(7.2, sensor.CurrentPH);
        }

        [TestMethod]
        public void PHSensor_ShouldRejectPHBelowZero()
        {
            // Arrange
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(-1.0));
        }

        [TestMethod]
        public void PHSensor_ShouldRejectPHAboveFourteen()
        {
            // Arrange
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(15.0));
        }

        [TestMethod]
        public void PHSensor_ShouldAcceptPHZero()
        {
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            sensor.UpdateReading(0.0);

            Assert.AreEqual(0.0, sensor.CurrentPH);
        }

        [TestMethod]
        public void PHSensor_ShouldAcceptPHFourteen()
        {
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            sensor.UpdateReading(14.0);

            Assert.AreEqual(14.0, sensor.CurrentPH);
        }

        [TestMethod]
        public void PHSensor_ShouldRejectNaN()
        {
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(double.NaN));
        }

        [TestMethod]
        public void PHSensor_ShouldRejectInfinity()
        {
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(double.PositiveInfinity));
        }


        [TestMethod]
        public void PHSensor_ShouldGenerateTelemetry()
        {
            // Arrange
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            sensor.UpdateReading(7.2);

            // Act
            var telemetry = sensor.GetTelemetry();

            // Assert
            Assert.AreEqual("PH001", telemetry.DeviceId);
            Assert.AreEqual("HAB001", telemetry.HabitatId);
            Assert.AreEqual("7.2", telemetry.Value);

            Assert.IsTrue(telemetry.Timestamp <= DateTime.UtcNow);
            Assert.IsTrue(
                telemetry.Timestamp > DateTime.UtcNow.AddMinutes(-1));
        }

        [TestMethod]
        public void PHSensor_ShouldRejectTelemetryWithoutReading()
        {
            // Arrange
            var sensor = new PHSensor(
                "PH001", "HAB001", "pH Sensor");

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(
                () => sensor.GetTelemetry());
        }

    }
}
