
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


    }
}
