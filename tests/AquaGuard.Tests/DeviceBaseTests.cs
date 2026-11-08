
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AquaGuard.Devices;
using System;

namespace AquaGuard.Tests
{
    [TestClass]
    public class DeviceBaseTests
    {
        // Concrete class used to test the abstract DeviceBase
        private class TestDevice : DeviceBase
        {
            public TestDevice(
                string deviceId,
                string habitatId,
                string name)
                : base(deviceId, habitatId, name)
            {
            }
        }

        [TestMethod]
        public void DeviceBase_ShouldStoreDeviceInformation()
        {
            // Arrange
            string deviceId = "DEV001";
            string habitatId = "HAB001";
            string name = "Temperature Sensor";

            // Act
            var device = new TestDevice(deviceId, habitatId, name);

            // Assert
            Assert.AreEqual(deviceId, device.DeviceId);
            Assert.AreEqual(habitatId, device.HabitatId);
            Assert.AreEqual(name, device.Name);
        }

        [TestMethod]
        public void DeviceBase_ShouldImplementIDevice()
        {
            // Arrange
            var device = new TestDevice(
                "DEV001",
                "HAB001",
                "Temperature Sensor");

            // Act & Assert
            Assert.IsInstanceOfType<IDevice>(device);
        }

        [TestMethod]
        [DataRow("", "HAB001", "Temperature Sensor", "deviceId")]
        [DataRow("DEV001", "", "Temperature Sensor", "habitatId")]
        [DataRow("DEV001", "HAB001", "", "name")]
        [DataRow(null, "HAB001", "Temperature Sensor", "deviceId")]
        [DataRow("   ", "HAB001", "Temperature Sensor", "deviceId")]
        [DataRow("DEV001", null, "Temperature Sensor", "habitatId")]
        [DataRow("DEV001", "   ", "Temperature Sensor", "habitatId")]
        [DataRow("DEV001", "HAB001", null, "name")]
        [DataRow("DEV001", "HAB001", "   ", "name")]
        public void DeviceBase_ShouldRejectInvalidRequiredFields(
            string? deviceId,
            string? habitatId,
            string? name,
            string expectedParameter)
        {
            // Arrange
            Action createDevice = () =>
                new TestDevice(deviceId!, habitatId!, name!);

            // Act & Assert
            var exception = Assert.ThrowsExactly<ArgumentException>(createDevice);

            Assert.AreEqual(expectedParameter, exception.ParamName);
        }



        [TestMethod]
        [DataRow(nameof(DeviceBase.DeviceId))]
        [DataRow(nameof(DeviceBase.HabitatId))]
        [DataRow(nameof(DeviceBase.Name))]
        public void DeviceBase_IdentityPropertiesShouldBeReadOnly(string propertyName)
        {
            // Arrange
            Type deviceType = typeof(DeviceBase);

            // Act
            var property = deviceType.GetProperty(propertyName);

            // Assert
            Assert.IsNotNull(property);
            Assert.IsNull(property.SetMethod);
        }

    }
}
