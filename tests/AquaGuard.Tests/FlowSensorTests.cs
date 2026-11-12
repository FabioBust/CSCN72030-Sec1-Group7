
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Devices.Sensors;

namespace AquaGuard.Tests
{
    [TestClass]
    public class FlowSensorTests
    {
        [TestMethod]
        public void FlowSensor_ShouldStoreDeviceInformation()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001",
                "HAB001",
                "Water Flow Sensor");

            // Assert
            Assert.AreEqual("FLOW001", sensor.DeviceId);
            Assert.AreEqual("HAB001", sensor.HabitatId);
            Assert.AreEqual("Water Flow Sensor", sensor.Name);
        }

        [TestMethod]
        public void FlowSensor_ShouldStoreFlowRate()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001",
                "HAB001",
                "Water Flow Sensor");

            // Act
            sensor.UpdateReading(25.5);

            // Assert
            Assert.AreEqual(25.5, sensor.CurrentFlowRate);
        }

        [TestMethod]
        public void FlowSensor_ShouldRejectNegativeFlowRate()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001",
                "HAB001",
                "Water Flow Sensor");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(-5.0));
        }

        [TestMethod]
        public void FlowSensor_ShouldAcceptZeroFlowRate()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001", "HAB001", "Water Flow Sensor");

            // Act
            sensor.UpdateReading(0.0);

            // Assert
            Assert.AreEqual(0.0, sensor.CurrentFlowRate);
        }

        [TestMethod]
        public void FlowSensor_ShouldRejectNaN()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001", "HAB001", "Water Flow Sensor");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(double.NaN));
        }

        [TestMethod]
        public void FlowSensor_ShouldRejectInfinity()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001", "HAB001", "Water Flow Sensor");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => sensor.UpdateReading(double.PositiveInfinity));
        }

        [TestMethod]
        public void FlowSensor_ShouldGenerateTelemetry()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001",
                "HAB001",
                "Water Flow Sensor");

            sensor.UpdateReading(25.5);

            // Act
            var telemetry = sensor.GetTelemetry();

            // Assert
            Assert.AreEqual("FLOW001", telemetry.DeviceId);
            Assert.AreEqual("HAB001", telemetry.HabitatId);
            Assert.AreEqual("25.5", telemetry.Value);

            Assert.IsTrue(telemetry.Timestamp <= DateTime.UtcNow);
            Assert.IsTrue(
                telemetry.Timestamp > DateTime.UtcNow.AddMinutes(-1));
        }

        [TestMethod]
        public void FlowSensor_ShouldRejectTelemetryWithoutReading()
        {
            // Arrange
            var sensor = new FlowSensor(
                "FLOW001",
                "HAB001",
                "Water Flow Sensor");

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(
                () => sensor.GetTelemetry());
        }


    }
}
