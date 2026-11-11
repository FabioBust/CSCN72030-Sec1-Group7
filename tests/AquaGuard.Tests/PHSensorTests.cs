
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

    }
}
